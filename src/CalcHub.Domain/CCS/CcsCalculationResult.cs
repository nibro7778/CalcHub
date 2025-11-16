using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcHub.Domain.CCS
{
    public class CcsCalculationResult
    {
        public decimal SubsidyPercentage { get; set; }
        public Money SubsidyPerHour { get; set; }
        public Money SubsidyPerWeek { get; set; }
        public Money SubsidyPerFortnight { get; set; }
        public Money SubsidyPerYear { get; set; }
        public Money OutOfPocketPerHour { get; set; }
        public Money OutOfPocketPerWeek { get; set; }
        public Money OutOfPocketPerFortnight { get; set; }
        public Money OutOfPocketPerYear { get; set; }
        public Money TotalCostPerWeek { get; set; }
        public int SubsidisedHoursPerFortnight { get; set; }
        public decimal HourlyCap { get; set; }
        public bool IsAboveIncomeThreshold { get; set; }
        public string EligibilityMessage { get; set; }

        public CcsCalculationResult()
        {
            SubsidyPerHour = new Money(0);
            SubsidyPerWeek = new Money(0);
            SubsidyPerFortnight = new Money(0);
            SubsidyPerYear = new Money(0);
            OutOfPocketPerHour = new Money(0);
            OutOfPocketPerWeek = new Money(0);
            OutOfPocketPerFortnight = new Money(0);
            OutOfPocketPerYear = new Money(0);
            TotalCostPerWeek = new Money(0);
            EligibilityMessage = string.Empty;
        }
    }
}
