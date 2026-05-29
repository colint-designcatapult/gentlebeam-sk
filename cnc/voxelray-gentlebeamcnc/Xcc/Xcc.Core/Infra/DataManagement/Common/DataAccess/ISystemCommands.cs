using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.System;

namespace Xcc.Core.Infra.DataManagement.Common.DataAccess;

public interface ISystemCommands
{
    Task<MosesSystemInfo> GetSystemInfoAsync();
}