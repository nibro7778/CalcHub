
namespace CalcHub.Domain.CCS
{
    public class CcsCalculationInput
    {
        public decimal AnnualFamilyIncome { get; set; }
        public ChildCareType ChildCareType { get; set; }
        public decimal HourlyRate { get; set; }
        public int HoursPerWeek { get; set; }
        public int NumberOfChildren { get; set; }
        public ActivityLevel ActivityLevel { get; set; }
        public bool IsWorkingOrStudying { get; set; }

        public void Validate()
        {
            if (AnnualFamilyIncome < 0)
                throw new ArgumentException("Annual family income cannot be negative");

            if (HourlyRate <= 0)
                throw new ArgumentException("Hourly rate must be greater than zero");

            if (HoursPerWeek <= 0 || HoursPerWeek > 168)
                throw new ArgumentException("Hours per week must be between 1 and 168");

            if (NumberOfChildren <= 0)
                throw new ArgumentException("Number of children must be at least 1");
        }
    }
}
