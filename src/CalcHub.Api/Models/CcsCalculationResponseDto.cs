namespace CalcHub.Api.Models
{
    public class CcsCalculationResponseDto
    {
        public decimal SubsidyPercentage { get; set; }
        public decimal SubsidyPerHour { get; set; }
        public decimal SubsidyPerWeek { get; set; }
        public decimal SubsidyPerFortnight { get; set; }
        public decimal SubsidyPerYear { get; set; }
        public decimal OutOfPocketPerHour { get; set; }
        public decimal OutOfPocketPerWeek { get; set; }
        public decimal OutOfPocketPerFortnight { get; set; }
        public decimal OutOfPocketPerYear { get; set; }
        public decimal TotalCostPerWeek { get; set; }
        public int SubsidisedHoursPerFortnight { get; set; }
        public decimal HourlyCap { get; set; }
        public bool IsAboveIncomeThreshold { get; set; }
        public string EligibilityMessage { get; set; } = string.Empty;
    }
}
