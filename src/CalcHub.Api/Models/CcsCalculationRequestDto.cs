using System.ComponentModel.DataAnnotations;

namespace CalcHub.Api.Models
{
    public class CcsCalculationRequestDto
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Annual income must be positive")]
        public decimal AnnualFamilyIncome { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Invalid child care type")]
        public int ChildCareType { get; set; }

        [Required]
        [Range(0.01, 1000, ErrorMessage = "Hourly rate must be between $0.01 and $1000")]
        public decimal HourlyRate { get; set; }

        [Required]
        [Range(1, 168, ErrorMessage = "Hours per week must be between 1 and 168")]
        public int HoursPerWeek { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Number of children must be between 1 and 20")]
        public int NumberOfChildren { get; set; }

        [Required]
        [Range(1, 3, ErrorMessage = "Invalid activity level")]
        public int ActivityLevel { get; set; }

        [Required]
        public bool IsWorkingOrStudying { get; set; }
    }
}
