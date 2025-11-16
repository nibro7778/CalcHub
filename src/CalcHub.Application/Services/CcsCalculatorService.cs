using CalcHub.Application.Usecases;
using CalcHub.Domain;
using CalcHub.Domain.CCS;

namespace CalcHub.Application.Services
{
    public class CcsCalculatorService : ICcsCalculatorService
    {
        // 2024-25 Financial Year CCS rates (as of July 2024)
        private const decimal IncomeThresholdLower = 80000m;
        private const decimal IncomeThresholdUpper = 530000m;
        private const decimal MaxSubsidyPercentage = 90m;
        private const decimal MinSubsidyPercentage = 0m;

        // Hourly rate caps (2024-25)
        private readonly Dictionary<ChildCareType, decimal> _hourlyCaps = new()
        {
            { ChildCareType.LongDayCare, 13.73m },
            { ChildCareType.FamilyDayCare, 12.74m },
            { ChildCareType.OutsideSchoolHoursCare, 12.75m },
            { ChildCareType.InHomecare, 36.24m },
            { ChildCareType.OccasionalCare, 13.73m }
        };

        public CcsCalculationResult Calculate(CcsCalculationInput input)
        {
            input.Validate();

            var result = new CcsCalculationResult();

            // Check eligibility
            if (!CheckEligibility(input, result))
            {
                return result;
            }

            // Calculate subsidy percentage based on income
            result.SubsidyPercentage = CalculateSubsidyPercentage(input.AnnualFamilyIncome);

            // Get hourly cap for the care type
            result.HourlyCap = _hourlyCaps[input.ChildCareType];

            // Determine subsidised hours per fortnight based on activity level
            result.SubsidisedHoursPerFortnight = GetSubsidisedHours(input.ActivityLevel);

            // Calculate subsidy per hour (lesser of actual rate or hourly cap)
            var effectiveHourlyRate = Math.Min(input.HourlyRate, result.HourlyCap);
            var subsidyAmountPerHour = effectiveHourlyRate * (result.SubsidyPercentage / 100m);
            result.SubsidyPerHour = new Money(Math.Round(subsidyAmountPerHour, 2));

            // Calculate out of pocket per hour
            result.OutOfPocketPerHour = new Money(Math.Round(input.HourlyRate - subsidyAmountPerHour, 2));

            // Calculate weekly costs
            var hoursPerWeek = input.HoursPerWeek;
            result.TotalCostPerWeek = new Money(input.HourlyRate * hoursPerWeek);
            result.SubsidyPerWeek = new Money(subsidyAmountPerHour * hoursPerWeek);
            result.OutOfPocketPerWeek = result.TotalCostPerWeek - result.SubsidyPerWeek;

            // Calculate fortnightly costs
            result.SubsidyPerFortnight = new Money(result.SubsidyPerWeek.Amount * 2);
            result.OutOfPocketPerFortnight = new Money(result.OutOfPocketPerWeek.Amount * 2);

            // Calculate annual costs (52 weeks)
            result.SubsidyPerYear = new Money(result.SubsidyPerWeek.Amount * 52);
            result.OutOfPocketPerYear = new Money(result.OutOfPocketPerWeek.Amount * 52);

            // Check if above income threshold
            result.IsAboveIncomeThreshold = input.AnnualFamilyIncome > IncomeThresholdUpper;

            return result;
        }

        private bool CheckEligibility(CcsCalculationInput input, CcsCalculationResult result)
        {
            if (!input.IsWorkingOrStudying)
            {
                result.EligibilityMessage = "You must meet work, training, study or other activity requirements to be eligible for CCS.";
                return false;
            }

            if (input.AnnualFamilyIncome > IncomeThresholdUpper)
            {
                result.EligibilityMessage = $"Family income exceeds the threshold of ${IncomeThresholdUpper:N0}. No subsidy is available.";
                return false;
            }

            result.EligibilityMessage = "You are eligible for Child Care Subsidy.";
            return true;
        }

        private decimal CalculateSubsidyPercentage(decimal annualIncome)
        {
            // Income up to $80,000: 90% subsidy
            if (annualIncome <= IncomeThresholdLower)
            {
                return MaxSubsidyPercentage;
            }

            // Income between $80,000 and $530,000: Tapers down
            // Reduces by 1% for every $5,000 over $80,000
            if (annualIncome <= IncomeThresholdUpper)
            {
                var incomeOverThreshold = annualIncome - IncomeThresholdLower;
                var reductionPercentage = Math.Floor(incomeOverThreshold / 5000m);
                var subsidyPercentage = MaxSubsidyPercentage - reductionPercentage;

                return Math.Max(subsidyPercentage, MinSubsidyPercentage);
            }

            // Income over $530,000: No subsidy
            return MinSubsidyPercentage;
        }

        private int GetSubsidisedHours(ActivityLevel activityLevel)
        {
            return activityLevel switch
            {
                ActivityLevel.EightToSixteenHours => 36,      // Up to 36 hours per fortnight
                ActivityLevel.SixteenToFortyEightHours => 72, // Up to 72 hours per fortnight
                ActivityLevel.OverFortyEightHours => 100,     // Up to 100 hours per fortnight
                _ => 36
            };
        }
    }
}
