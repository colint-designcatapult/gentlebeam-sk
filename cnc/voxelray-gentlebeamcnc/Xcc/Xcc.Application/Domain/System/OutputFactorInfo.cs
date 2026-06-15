using System;

namespace Xcc.Application.Domain.System;

public struct OutputFactorInfo
{
    public double ReferencedDoseRate { get; }
    public double OutputFactor { get; }
        
    public OutputFactorInfo(double outputFactor, double referencedDoseRate)
    {
        OutputFactor = outputFactor;
        ReferencedDoseRate = referencedDoseRate;
    }

    public double Dose(double dwellTime)
    {
        return OutputFactor * ReferencedDoseRate * dwellTime / 60;
    }

    public double Duration(double dose)
    {
        // PrescribedDose / (FieldOutputFactor * DoseRate) - rounded up, according to H10SG-20
        return Math.Ceiling(dose / (OutputFactor * ReferencedDoseRate) * 60);
    }

    public double DurationUpTo10th(double dose)
    {
        // PrescribedDose / (FieldOutputFactor * DoseRate) - rounded up, according to H10SG-20
        return Math.Ceiling(dose / (OutputFactor * ReferencedDoseRate) * 600) / 10.0;
    }


    public double CalculatedDose(double prescribedDose)
    {
        return Dose(Duration(prescribedDose));
    }

    public double CalculatedDoseUpTo10th(double prescribedDose)
    {
        return Dose(DurationUpTo10th(prescribedDose));
    }

}