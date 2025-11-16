
using CalcHub.Domain.CCS;

namespace CalcHub.Application.Usecases
{
    public interface ICcsCalculatorService
    {
        CcsCalculationResult Calculate(CcsCalculationInput input);
    }
}
