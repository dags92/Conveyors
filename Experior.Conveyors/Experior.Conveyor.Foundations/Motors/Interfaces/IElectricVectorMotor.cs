using Experior.Conveyor.Foundations.Motors.Collections;
using Experior.Core.Communication.PLC;
using Experior.Interfaces;

namespace Experior.Conveyor.Foundations.Motors.Interfaces
{
    public interface IElectricVectorMotor : IElectricMotor
    {
        VectorPartCollection Parts { get; }

        VectorAssemblyCollection Assemblies { get; }

        AuxiliaryData.TranslationAutomaticLimits AutomaticLimitType { get; set; }

        AuxiliaryData.TranslationPositions ResetPosition { get; set; }

        Output OutputMaxLimit { get; set; }

        Output OutputMidLimit { get; set; }

        Output OutputMinLimit { get; set; }

        void Calibrate();
    }
}
