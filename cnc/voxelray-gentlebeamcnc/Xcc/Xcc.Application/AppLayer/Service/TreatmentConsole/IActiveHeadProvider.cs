
using Xcc.Core.Domain.DataManagement.System;

namespace Xcc.Application.AppLayer.Service.TreatmentConsole;

public interface IActiveHeadProvider
{
    IHead ActiveHead { get; }
}