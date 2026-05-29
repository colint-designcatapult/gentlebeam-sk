using System.Threading.Tasks;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Core.Models;
using Heracles.Core.Models.RDBMS;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Domain.DataManagement.System.QualityAssurance;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess
{
    public interface ISettingsCommands
    {
        Task<ISystemSettings> GetSettingsAsync();
        Task<ISystemSettings> UpdateSettingsAsync(ISystemSettings oldValue, ISystemSettings newValue);
    }

    public interface ISafetyCheckCommands : IAsyncRootEntryCommands<ISafetyCheck>
    {
    }

    public interface IQcSampleFieldCommands : IAsyncChildEntryCommands<IQcSampleField>
    {
    }

    public interface IQcSampleCommands 
        : IAsyncChildEntryCommands<IQcSampleHeader>
        , IAsyncApprovalCommands<IQcSampleHeader>
    {
    }

    public interface IIntensityCommands : IAsyncChildEntryCommands<IIntensity>
    {
    }

    public interface IHeadCommands : IAsyncRootEntryCommands<IHead>
    {
    }

    public interface ICollimatorConfigurationCommands : IAsyncRootEntryCommands<ICollimatorConfiguration>
    {
    }

    public interface ICollimatorCommands : IAsyncChildEntryCommands<ICollimator>
    {
    }

    public interface IPresetConfigurationCommands 
        : IAsyncChildEntryCommands<IPresetConfiguration>
        , IAsyncApprovalCommands<IPresetConfiguration>
    {
    }

    public interface ICoilConfigurationCommands : IAsyncChildEntryCommands<ICoilConfigurationEntry>
    {
    }

    public interface ICorrectionMatrixCommands : IAsyncChildEntryCommands<ICorrectionMatrixEntry>
    {
    }

    public interface IHeaterCurrentConfigCommands : IAsyncChildEntryCommands<IHeaterCurrentConfig>
    {
    }
    public interface IReferenceFieldCommands : IAsyncChildEntryCommands<IReferenceFieldEntry>
    {
    }

    public interface IOutputFactorCommands : IAsyncChildEntryCommands<IOutputFactor>
    {
    }

}
