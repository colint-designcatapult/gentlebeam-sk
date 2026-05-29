using System;
using System.Collections.Generic;
using System.Linq;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Helpers;

namespace Xcc.Application.Domain.System;

public struct MagnetometerConfiguration
{
    public Matrix2x3 FrontMatrix, BackMatrix;
    public Vector3 FrontReferenceField, BackReferenceField;

    public DeflectionCurrentCorrection CalculateCorrection(
        MagnetometerType magnetometerType,
        Vector3 readOutValue)
    {
        return magnetometerType switch
        {
            MagnetometerType.Front => CalcCorrection(FrontMatrix, FrontReferenceField, readOutValue),
            MagnetometerType.Back => CalcCorrection(BackMatrix, BackReferenceField, readOutValue),
            _ => throw new ArgumentException($"Magnetometer correction calculation error: invalid magnetometer type value {magnetometerType}")
        };
    }

    public static MagnetometerConfiguration Create(
        IEnumerable<ICorrectionMatrix> matrices,
        IEnumerable<IReferenceField> fields)
    {
        MagnetometerConfiguration config = new();

        var matrixTypes = matrices.Select(x => x.MagnetometerType).Distinct();
        var fieldTypes = fields.Select(x => x.MagnetometerType).Distinct();
        if (matrixTypes.Count() < 2 || fieldTypes.Count() < 2)
            throw new ArgumentException("Incomplete magnetometer data");

        foreach (var matrix in matrices)
        {
            Matrix2x3 matrix3x2 = GetMatrix2x3(matrix);

            _ = matrix.MagnetometerType switch
            {
                MagnetometerType.Front => config.FrontMatrix = matrix3x2,
                MagnetometerType.Back => config.BackMatrix = matrix3x2,
                _ => throw new InvalidOperationException($"Wrong input magnetometer type {matrix.MagnetometerType}"),
            };
        }

        foreach (var field in fields)
        {
            Vector3 value = GetVector3(field);

            _ = field.MagnetometerType switch
            {
                MagnetometerType.Front => config.FrontReferenceField = value,
                MagnetometerType.Back => config.BackReferenceField = value,
                _ => throw new InvalidOperationException($"Wrong input magnetometer type {field.MagnetometerType}"),
            };
        }
        return config;
    }

    private static DeflectionCurrentCorrection CalcCorrection(
        Matrix2x3 correctionMatrix,
        Vector3 referenceValue,
        Vector3 readOutValue)
    {
        const double normFactor = 1 / 1000.0;

        var calculatedCorrection = (normFactor * correctionMatrix) * (referenceValue - readOutValue);

        return new(calculatedCorrection[0, 0], calculatedCorrection[1, 0]);
    }

    public static Matrix2x3 GetMatrix2x3(ICorrectionMatrix matrix)
    {
        Matrix2x3 result = new();
        result[0, 0] = (float)matrix.Cm11;
        result[0, 1] = (float)matrix.Cm12;
        result[0, 2] = (float)matrix.Cm13;
        result[1, 0] = (float)matrix.Cm21;
        result[1, 1] = (float)matrix.Cm22;
        result[1, 2] = (float)matrix.Cm23;
        return result;
    }

    public static Vector3 GetVector3(IReferenceField field)
    {
        Vector3 result = new();
        result[0, 0] = (float)field.Rf11;
        result[1, 0] = (float)field.Rf21;
        result[2, 0] = (float)field.Rf31;
        return result;
    }
}