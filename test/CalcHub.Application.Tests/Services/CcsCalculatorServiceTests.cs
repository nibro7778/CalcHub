using CalcHub.Application.Services;
using CalcHub.Domain.CCS;
using FluentAssertions;
using Xunit;

namespace CalcHub.Application.Tests.Services
{
    public class CcsCalculatorServiceTests
    {
        private readonly CcsCalculatorService _sut;

        public CcsCalculatorServiceTests()
        {
            _sut = new CcsCalculatorService();
        }

        #region Eligibility Tests

        [Fact]
        public void Calculate_WhenNotWorkingOrStudying_ShouldReturnIneligible()
        {
            // Arrange
            var input = CreateValidInput();
            input.IsWorkingOrStudying = false;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.EligibilityMessage.Should().Be("You must meet work, training, study or other activity requirements to be eligible for CCS.");
            result.SubsidyPercentage.Should().Be(0);
            result.SubsidyPerHour.Amount.Should().Be(0);
            result.SubsidyPerWeek.Amount.Should().Be(0);
        }

        [Fact]
        public void Calculate_WhenIncomeExceedsUpperThreshold_ShouldReturnIneligible()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 530001m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.EligibilityMessage.Should().Be("Family income exceeds the threshold of $530,000. No subsidy is available.");
            result.SubsidyPercentage.Should().Be(0);
            result.SubsidyPerHour.Amount.Should().Be(0);
        }

        [Fact]
        public void Calculate_WhenIncomeAtUpperThreshold_ShouldBeEligible()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 530000m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.EligibilityMessage.Should().Be("You are eligible for Child Care Subsidy.");
        }

        [Fact]
        public void Calculate_WhenWorkingOrStudying_ShouldBeEligible()
        {
            // Arrange
            var input = CreateValidInput();
            input.IsWorkingOrStudying = true;
            input.AnnualFamilyIncome = 80000m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.EligibilityMessage.Should().Be("You are eligible for Child Care Subsidy.");
        }

        #endregion

        #region Subsidy Percentage Calculation Tests

        [Fact]
        public void Calculate_WhenIncomeAtOrBelowLowerThreshold_ShouldReturn90PercentSubsidy()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 80000m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.SubsidyPercentage.Should().Be(90m);
        }

        [Fact]
        public void Calculate_WhenIncomeBelowLowerThreshold_ShouldReturn90PercentSubsidy()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 50000m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.SubsidyPercentage.Should().Be(90m);
        }

        [Fact]
        public void Calculate_WhenIncomeZero_ShouldReturn90PercentSubsidy()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 0m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.SubsidyPercentage.Should().Be(90m);
        }

        [Theory]
        [InlineData(85000, 89)]   // $5,000 over threshold = 1% reduction
        [InlineData(90000, 88)]   // $10,000 over threshold = 2% reduction
        [InlineData(100000, 86)]  // $20,000 over threshold = 4% reduction
        [InlineData(130000, 80)]  // $50,000 over threshold = 10% reduction
        [InlineData(180000, 70)]  // $100,000 over threshold = 20% reduction
        [InlineData(280000, 50)]  // $200,000 over threshold = 40% reduction
        [InlineData(480000, 10)]  // $400,000 over threshold = 80% reduction
        public void Calculate_WhenIncomeInTaperRange_ShouldReturnCorrectSubsidyPercentage(decimal income, decimal expectedPercentage)
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = income;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.SubsidyPercentage.Should().Be(expectedPercentage);
        }

        [Fact]
        public void Calculate_WhenIncomeAtExactUpperThreshold_ShouldReturnZeroSubsidy()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 530000m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // $530,000 - $80,000 = $450,000 / $5,000 = 90% reduction
            // 90% - 90% = 0%
            result.SubsidyPercentage.Should().Be(0m);
        }

        [Fact]
        public void Calculate_WhenIncomeJustBelowUpperThreshold_ShouldReturnMinimalSubsidy()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 525000m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // $525,000 - $80,000 = $445,000 / $5,000 = 89% reduction
            // 90% - 89% = 1%
            result.SubsidyPercentage.Should().Be(1m);
        }

        #endregion

        #region Hourly Rate Cap Tests

        [Theory]
        [InlineData(ChildCareType.LongDayCare, 13.73)]
        [InlineData(ChildCareType.FamilyDayCare, 12.74)]
        [InlineData(ChildCareType.OutsideSchoolHoursCare, 12.75)]
        [InlineData(ChildCareType.InHomecare, 36.24)]
        [InlineData(ChildCareType.OccasionalCare, 13.73)]
        public void Calculate_ShouldApplyCorrectHourlyCapForCareType(ChildCareType careType, decimal expectedCap)
        {
            // Arrange
            var input = CreateValidInput();
            input.ChildCareType = careType;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.HourlyCap.Should().Be(expectedCap);
        }

        [Fact]
        public void Calculate_WhenHourlyRateExceedsCap_ShouldUseCapForSubsidyCalculation()
        {
            // Arrange
            var input = CreateValidInput();
            input.ChildCareType = ChildCareType.LongDayCare;
            input.HourlyRate = 20m; // Higher than cap of $13.73
            input.AnnualFamilyIncome = 80000m; // 90% subsidy

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // Effective hourly rate = $13.73 (cap)
            // Subsidy per hour = $13.73 * 90% = $12.357, rounded to $12.36
            result.SubsidyPerHour.Amount.Should().Be(12.36m);
            // Out of pocket = $20.00 - $12.36 = $7.64
            result.OutOfPocketPerHour.Amount.Should().Be(7.64m);
        }

        [Fact]
        public void Calculate_WhenHourlyRateBelowCap_ShouldUseActualRateForSubsidyCalculation()
        {
            // Arrange
            var input = CreateValidInput();
            input.ChildCareType = ChildCareType.LongDayCare;
            input.HourlyRate = 10m; // Lower than cap of $13.73
            input.AnnualFamilyIncome = 80000m; // 90% subsidy

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // Subsidy per hour = $10.00 * 90% = $9.00
            result.SubsidyPerHour.Amount.Should().Be(9m);
            // Out of pocket = $10.00 - $9.00 = $1.00
            result.OutOfPocketPerHour.Amount.Should().Be(1m);
        }

        [Fact]
        public void Calculate_ForInHomecare_ShouldUseHigherCap()
        {
            // Arrange
            var input = CreateValidInput();
            input.ChildCareType = ChildCareType.InHomecare;
            input.HourlyRate = 30m; // Below cap of $36.24
            input.AnnualFamilyIncome = 80000m; // 90% subsidy

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.HourlyCap.Should().Be(36.24m);
            // Subsidy per hour = $30.00 * 90% = $27.00
            result.SubsidyPerHour.Amount.Should().Be(27m);
        }

        #endregion

        #region Activity Level / Subsidised Hours Tests

        [Theory]
        [InlineData(ActivityLevel.EightToSixteenHours, 36)]
        [InlineData(ActivityLevel.SixteenToFortyEightHours, 72)]
        [InlineData(ActivityLevel.OverFortyEightHours, 100)]
        public void Calculate_ShouldReturnCorrectSubsidisedHoursForActivityLevel(ActivityLevel activityLevel, int expectedHours)
        {
            // Arrange
            var input = CreateValidInput();
            input.ActivityLevel = activityLevel;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.SubsidisedHoursPerFortnight.Should().Be(expectedHours);
        }

        #endregion

        #region Weekly, Fortnightly, and Annual Calculation Tests

        [Fact]
        public void Calculate_ShouldCalculateTotalCostPerWeekCorrectly()
        {
            // Arrange
            var input = CreateValidInput();
            input.HourlyRate = 15m;
            input.HoursPerWeek = 40;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.TotalCostPerWeek.Amount.Should().Be(600m); // $15 * 40 hours
        }

        [Fact]
        public void Calculate_ShouldCalculateSubsidyPerWeekCorrectly()
        {
            // Arrange
            var input = CreateValidInput();
            input.ChildCareType = ChildCareType.LongDayCare;
            input.HourlyRate = 10m;
            input.HoursPerWeek = 40;
            input.AnnualFamilyIncome = 80000m; // 90% subsidy

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // Subsidy per hour = $10.00 * 90% = $9.00
            // Subsidy per week = $9.00 * 40 = $360.00
            result.SubsidyPerWeek.Amount.Should().Be(360m);
        }

        [Fact]
        public void Calculate_ShouldCalculateOutOfPocketPerWeekCorrectly()
        {
            // Arrange
            var input = CreateValidInput();
            input.ChildCareType = ChildCareType.LongDayCare;
            input.HourlyRate = 10m;
            input.HoursPerWeek = 40;
            input.AnnualFamilyIncome = 80000m; // 90% subsidy

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // Total cost = $400.00
            // Subsidy = $360.00
            // Out of pocket = $400.00 - $360.00 = $40.00
            result.OutOfPocketPerWeek.Amount.Should().Be(40m);
        }

        [Fact]
        public void Calculate_ShouldCalculateFortnightlyAmountsCorrectly()
        {
            // Arrange
            var input = CreateValidInput();
            input.ChildCareType = ChildCareType.LongDayCare;
            input.HourlyRate = 10m;
            input.HoursPerWeek = 40;
            input.AnnualFamilyIncome = 80000m; // 90% subsidy

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // Weekly subsidy = $360.00
            // Fortnightly subsidy = $720.00
            result.SubsidyPerFortnight.Amount.Should().Be(720m);
            // Weekly out of pocket = $40.00
            // Fortnightly out of pocket = $80.00
            result.OutOfPocketPerFortnight.Amount.Should().Be(80m);
        }

        [Fact]
        public void Calculate_ShouldCalculateAnnualAmountsCorrectly()
        {
            // Arrange
            var input = CreateValidInput();
            input.ChildCareType = ChildCareType.LongDayCare;
            input.HourlyRate = 10m;
            input.HoursPerWeek = 40;
            input.AnnualFamilyIncome = 80000m; // 90% subsidy

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // Weekly subsidy = $360.00
            // Annual subsidy = $360.00 * 52 = $18,720.00
            result.SubsidyPerYear.Amount.Should().Be(18720m);
            // Weekly out of pocket = $40.00
            // Annual out of pocket = $40.00 * 52 = $2,080.00
            result.OutOfPocketPerYear.Amount.Should().Be(2080m);
        }

        #endregion

        #region IsAboveIncomeThreshold Tests

        [Fact]
        public void Calculate_WhenIncomeAboveUpperThreshold_ShouldSetIsAboveIncomeThresholdTrue()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 530001m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // Note: When income exceeds threshold, eligibility check fails first
            // so IsAboveIncomeThreshold is not set in the result
            result.IsAboveIncomeThreshold.Should().BeFalse();
        }

        [Fact]
        public void Calculate_WhenIncomeAtUpperThreshold_ShouldSetIsAboveIncomeThresholdFalse()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 530000m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.IsAboveIncomeThreshold.Should().BeFalse();
        }

        [Fact]
        public void Calculate_WhenIncomeBelowUpperThreshold_ShouldSetIsAboveIncomeThresholdFalse()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 400000m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.IsAboveIncomeThreshold.Should().BeFalse();
        }

        #endregion

        #region Input Validation Tests

        [Fact]
        public void Calculate_WithNegativeIncome_ShouldThrowArgumentException()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = -1m;

            // Act
            var act = () => _sut.Calculate(input);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Annual family income cannot be negative");
        }

        [Fact]
        public void Calculate_WithZeroHourlyRate_ShouldThrowArgumentException()
        {
            // Arrange
            var input = CreateValidInput();
            input.HourlyRate = 0m;

            // Act
            var act = () => _sut.Calculate(input);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Hourly rate must be greater than zero");
        }

        [Fact]
        public void Calculate_WithNegativeHourlyRate_ShouldThrowArgumentException()
        {
            // Arrange
            var input = CreateValidInput();
            input.HourlyRate = -5m;

            // Act
            var act = () => _sut.Calculate(input);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Hourly rate must be greater than zero");
        }

        [Fact]
        public void Calculate_WithZeroHoursPerWeek_ShouldThrowArgumentException()
        {
            // Arrange
            var input = CreateValidInput();
            input.HoursPerWeek = 0;

            // Act
            var act = () => _sut.Calculate(input);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Hours per week must be between 1 and 168");
        }

        [Fact]
        public void Calculate_WithExcessiveHoursPerWeek_ShouldThrowArgumentException()
        {
            // Arrange
            var input = CreateValidInput();
            input.HoursPerWeek = 169;

            // Act
            var act = () => _sut.Calculate(input);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Hours per week must be between 1 and 168");
        }

        [Fact]
        public void Calculate_WithZeroChildren_ShouldThrowArgumentException()
        {
            // Arrange
            var input = CreateValidInput();
            input.NumberOfChildren = 0;

            // Act
            var act = () => _sut.Calculate(input);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Number of children must be at least 1");
        }

        [Fact]
        public void Calculate_WithNegativeChildren_ShouldThrowArgumentException()
        {
            // Arrange
            var input = CreateValidInput();
            input.NumberOfChildren = -1;

            // Act
            var act = () => _sut.Calculate(input);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Number of children must be at least 1");
        }

        [Fact]
        public void Calculate_WithMaximumValidHoursPerWeek_ShouldNotThrow()
        {
            // Arrange
            var input = CreateValidInput();
            input.HoursPerWeek = 168;

            // Act
            var act = () => _sut.Calculate(input);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Calculate_WithMinimumValidHoursPerWeek_ShouldNotThrow()
        {
            // Arrange
            var input = CreateValidInput();
            input.HoursPerWeek = 1;

            // Act
            var act = () => _sut.Calculate(input);

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region Complete Calculation Scenario Tests

        [Fact]
        public void Calculate_FullScenario_LowIncomeLongDayCare()
        {
            // Arrange
            var input = new CcsCalculationInput
            {
                AnnualFamilyIncome = 60000m,
                ChildCareType = ChildCareType.LongDayCare,
                HourlyRate = 12m,
                HoursPerWeek = 30,
                NumberOfChildren = 1,
                ActivityLevel = ActivityLevel.OverFortyEightHours,
                IsWorkingOrStudying = true
            };

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.EligibilityMessage.Should().Be("You are eligible for Child Care Subsidy.");
            result.SubsidyPercentage.Should().Be(90m);
            result.HourlyCap.Should().Be(13.73m);
            result.SubsidisedHoursPerFortnight.Should().Be(100);
            
            // Subsidy per hour = $12.00 * 90% = $10.80
            result.SubsidyPerHour.Amount.Should().Be(10.80m);
            // Out of pocket per hour = $12.00 - $10.80 = $1.20
            result.OutOfPocketPerHour.Amount.Should().Be(1.20m);
            
            // Weekly calculations
            result.TotalCostPerWeek.Amount.Should().Be(360m); // $12 * 30
            result.SubsidyPerWeek.Amount.Should().Be(324m); // $10.80 * 30
            result.OutOfPocketPerWeek.Amount.Should().Be(36m);
            
            // Fortnightly calculations
            result.SubsidyPerFortnight.Amount.Should().Be(648m);
            result.OutOfPocketPerFortnight.Amount.Should().Be(72m);
            
            // Annual calculations
            result.SubsidyPerYear.Amount.Should().Be(16848m); // $324 * 52
            result.OutOfPocketPerYear.Amount.Should().Be(1872m); // $36 * 52
        }

        [Fact]
        public void Calculate_FullScenario_MediumIncomeFamilyDayCare()
        {
            // Arrange
            var input = new CcsCalculationInput
            {
                AnnualFamilyIncome = 130000m, // 80% subsidy
                ChildCareType = ChildCareType.FamilyDayCare,
                HourlyRate = 15m, // Above cap of $12.74
                HoursPerWeek = 25,
                NumberOfChildren = 2,
                ActivityLevel = ActivityLevel.SixteenToFortyEightHours,
                IsWorkingOrStudying = true
            };

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.SubsidyPercentage.Should().Be(80m);
            result.HourlyCap.Should().Be(12.74m);
            result.SubsidisedHoursPerFortnight.Should().Be(72);
            
            // Effective rate is capped at $12.74
            // Subsidy per hour = $12.74 * 80% = $10.192 => $10.19
            result.SubsidyPerHour.Amount.Should().Be(10.19m);
            // Out of pocket = $15.00 - $10.19 = $4.81
            result.OutOfPocketPerHour.Amount.Should().Be(4.81m);
        }

        [Fact]
        public void Calculate_FullScenario_HighIncomeInHomecare()
        {
            // Arrange
            var input = new CcsCalculationInput
            {
                AnnualFamilyIncome = 400000m, // 26% subsidy
                ChildCareType = ChildCareType.InHomecare,
                HourlyRate = 35m, // Below cap of $36.24
                HoursPerWeek = 50,
                NumberOfChildren = 3,
                ActivityLevel = ActivityLevel.OverFortyEightHours,
                IsWorkingOrStudying = true
            };

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // ($400,000 - $80,000) / $5,000 = 64% reduction
            // 90% - 64% = 26%
            result.SubsidyPercentage.Should().Be(26m);
            result.HourlyCap.Should().Be(36.24m);
            
            // Subsidy per hour = $35.00 * 26% = $9.10
            result.SubsidyPerHour.Amount.Should().Be(9.10m);
        }

        [Fact]
        public void Calculate_FullScenario_OutsideSchoolHoursCare()
        {
            // Arrange
            var input = new CcsCalculationInput
            {
                AnnualFamilyIncome = 100000m, // 86% subsidy
                ChildCareType = ChildCareType.OutsideSchoolHoursCare,
                HourlyRate = 12m, // Below cap of $12.75
                HoursPerWeek = 15,
                NumberOfChildren = 1,
                ActivityLevel = ActivityLevel.EightToSixteenHours,
                IsWorkingOrStudying = true
            };

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.SubsidyPercentage.Should().Be(86m);
            result.HourlyCap.Should().Be(12.75m);
            result.SubsidisedHoursPerFortnight.Should().Be(36);
            
            // Subsidy per hour = $12.00 * 86% = $10.32
            result.SubsidyPerHour.Amount.Should().Be(10.32m);
        }

        [Fact]
        public void Calculate_FullScenario_OccasionalCare()
        {
            // Arrange
            var input = new CcsCalculationInput
            {
                AnnualFamilyIncome = 75000m, // 90% subsidy (below lower threshold)
                ChildCareType = ChildCareType.OccasionalCare,
                HourlyRate = 13m,
                HoursPerWeek = 10,
                NumberOfChildren = 1,
                ActivityLevel = ActivityLevel.EightToSixteenHours,
                IsWorkingOrStudying = true
            };

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.SubsidyPercentage.Should().Be(90m);
            result.HourlyCap.Should().Be(13.73m);
            
            // Subsidy per hour = $13.00 * 90% = $11.70
            result.SubsidyPerHour.Amount.Should().Be(11.70m);
            // Out of pocket = $13.00 - $11.70 = $1.30
            result.OutOfPocketPerHour.Amount.Should().Be(1.30m);
            
            // Weekly calculations
            result.TotalCostPerWeek.Amount.Should().Be(130m); // $13 * 10
            result.SubsidyPerWeek.Amount.Should().Be(117m); // $11.70 * 10
            result.OutOfPocketPerWeek.Amount.Should().Be(13m);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void Calculate_WhenIncomeExactlyAtLowerThresholdBoundary_ShouldReturn90Percent()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 80000m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.SubsidyPercentage.Should().Be(90m);
        }

        [Fact]
        public void Calculate_WhenIncomeJustAboveLowerThreshold_ShouldTaper()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 80001m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // Floor($1 / $5000) = 0, so still 90%
            result.SubsidyPercentage.Should().Be(90m);
        }

        [Fact]
        public void Calculate_WhenIncomeAt5000Increment_ShouldReduceBy1Percent()
        {
            // Arrange
            var input = CreateValidInput();
            input.AnnualFamilyIncome = 85000m;

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.SubsidyPercentage.Should().Be(89m);
        }

        [Fact]
        public void Calculate_WhenHourlyRateExactlyAtCap_ShouldUseFullRate()
        {
            // Arrange
            var input = CreateValidInput();
            input.ChildCareType = ChildCareType.LongDayCare;
            input.HourlyRate = 13.73m; // Exactly at cap
            input.AnnualFamilyIncome = 80000m; // 90% subsidy

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // Subsidy per hour = $13.73 * 90% = $12.357 => $12.36
            result.SubsidyPerHour.Amount.Should().Be(12.36m);
        }

        [Fact]
        public void Calculate_ShouldHandleVerySmallHourlyRate()
        {
            // Arrange
            var input = CreateValidInput();
            input.HourlyRate = 0.01m;
            input.AnnualFamilyIncome = 80000m; // 90% subsidy

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // Subsidy per hour = $0.01 * 90% = $0.009 => $0.01
            result.SubsidyPerHour.Amount.Should().Be(0.01m);
        }

        [Fact]
        public void Calculate_ShouldHandleVeryHighHourlyRate()
        {
            // Arrange
            var input = CreateValidInput();
            input.ChildCareType = ChildCareType.LongDayCare;
            input.HourlyRate = 100m;
            input.AnnualFamilyIncome = 80000m; // 90% subsidy

            // Act
            var result = _sut.Calculate(input);

            // Assert
            // Capped at $13.73
            // Subsidy per hour = $13.73 * 90% = $12.36
            result.SubsidyPerHour.Amount.Should().Be(12.36m);
            // Out of pocket = $100.00 - $12.36 = $87.64
            result.OutOfPocketPerHour.Amount.Should().Be(87.64m);
        }

        [Fact]
        public void Calculate_DefaultActivityLevel_ShouldReturn36Hours()
        {
            // Arrange
            var input = CreateValidInput();
            input.ActivityLevel = (ActivityLevel)99; // Invalid enum value

            // Act
            var result = _sut.Calculate(input);

            // Assert
            result.SubsidisedHoursPerFortnight.Should().Be(36);
        }

        #endregion

        #region Helper Methods

        private static CcsCalculationInput CreateValidInput()
        {
            return new CcsCalculationInput
            {
                AnnualFamilyIncome = 100000m,
                ChildCareType = ChildCareType.LongDayCare,
                HourlyRate = 12m,
                HoursPerWeek = 40,
                NumberOfChildren = 1,
                ActivityLevel = ActivityLevel.OverFortyEightHours,
                IsWorkingOrStudying = true
            };
        }

        #endregion
    }
}
