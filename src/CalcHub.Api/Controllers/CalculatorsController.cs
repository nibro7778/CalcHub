using CalcHub.Api.Models;
using CalcHub.Application.Usecases;
using CalcHub.Domain.CCS;
using Microsoft.AspNetCore.Mvc;

namespace CalcHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculatorsController : ControllerBase
    {
        private readonly ICcsCalculatorService _ccsCalculatorService;
        private readonly ILogger<CalculatorsController> _logger;

        public CalculatorsController(
            ICcsCalculatorService ccsCalculatorService,
            ILogger<CalculatorsController> logger)
        {
            _ccsCalculatorService = ccsCalculatorService;
            _logger = logger;
        }

        /// <summary>
        /// Calculate Child Care Subsidy based on family income and circumstances
        /// </summary>
        [HttpPost("child-care-subsidy")]
        [ProducesResponseType(typeof(CcsCalculationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<CcsCalculationResponseDto> CalculateChildCareSubsidy(
            [FromBody] CcsCalculationRequestDto request)
        {
            try
            {
                _logger.LogInformation("Calculating CCS for income: {Income}", request.AnnualFamilyIncome);

                // Map DTO to domain entity
                var input = new CcsCalculationInput
                {
                    AnnualFamilyIncome = request.AnnualFamilyIncome,
                    ChildCareType = (ChildCareType)request.ChildCareType,
                    HourlyRate = request.HourlyRate,
                    HoursPerWeek = request.HoursPerWeek,
                    NumberOfChildren = request.NumberOfChildren,
                    ActivityLevel = (ActivityLevel)request.ActivityLevel,
                    IsWorkingOrStudying = request.IsWorkingOrStudying
                };

                // Calculate
                var result = _ccsCalculatorService.Calculate(input);

                // Map result to DTO
                var response = new CcsCalculationResponseDto
                {
                    SubsidyPercentage = result.SubsidyPercentage,
                    SubsidyPerHour = result.SubsidyPerHour.Amount,
                    SubsidyPerWeek = result.SubsidyPerWeek.Amount,
                    SubsidyPerFortnight = result.SubsidyPerFortnight.Amount,
                    SubsidyPerYear = result.SubsidyPerYear.Amount,
                    OutOfPocketPerHour = result.OutOfPocketPerHour.Amount,
                    OutOfPocketPerWeek = result.OutOfPocketPerWeek.Amount,
                    OutOfPocketPerFortnight = result.OutOfPocketPerFortnight.Amount,
                    OutOfPocketPerYear = result.OutOfPocketPerYear.Amount,
                    TotalCostPerWeek = result.TotalCostPerWeek.Amount,
                    SubsidisedHoursPerFortnight = result.SubsidisedHoursPerFortnight,
                    HourlyCap = result.HourlyCap,
                    IsAboveIncomeThreshold = result.IsAboveIncomeThreshold,
                    EligibilityMessage = result.EligibilityMessage
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input for CCS calculation");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating CCS");
                return StatusCode(500, new { error = "An error occurred while calculating the subsidy" });
            }
        }

        /// <summary>
        /// Get information about Child Care Subsidy rates and thresholds
        /// </summary>
        [HttpGet("child-care-subsidy/info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object> GetCcsInfo()
        {
            return Ok(new
            {
                financialYear = "2024-25",
                incomeThresholds = new
                {
                    lowerThreshold = 80000,
                    upperThreshold = 530000,
                    maxSubsidyPercentage = 90,
                    description = "Subsidy tapers down by 1% for every $5,000 over $80,000"
                },
                hourlyCaps = new
                {
                    longDayCare = 13.73m,
                    familyDayCare = 12.74m,
                    outsideSchoolHoursCare = 12.75m,
                    inHomeCare = 36.24m,
                    occasionalCare = 13.73m
                },
                activityLevels = new[]
                {
                new { level = 1, name = "8-16 hours per fortnight", subsidisedHours = 36 },
                new { level = 2, name = "16-48 hours per fortnight", subsidisedHours = 72 },
                new { level = 3, name = "48+ hours per fortnight", subsidisedHours = 100 }
            },
                childCareTypes = new[]
                {
                new { id = 1, name = "Long Day Care" },
                new { id = 2, name = "Family Day Care" },
                new { id = 3, name = "Outside School Hours Care" },
                new { id = 4, name = "In Home Care" },
                new { id = 5, name = "Occasional Care" }
            }
            });
        }
    }
}
