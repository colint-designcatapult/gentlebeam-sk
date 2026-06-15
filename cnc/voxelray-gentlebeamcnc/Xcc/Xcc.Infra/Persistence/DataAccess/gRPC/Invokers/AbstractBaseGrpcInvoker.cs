using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Xcc.Infra.Networking.gRPC.Channels;

namespace Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

public abstract class AbstractBaseGrpcInvoker<TProtoType> 
    where TProtoType : class, IMessage<TProtoType>, new()
{
    protected AbstractBaseGrpcInvoker(IGrpcChannelManager grpcSettings)
    {
        GrpcSettings = grpcSettings;
    }

    protected IGrpcChannelManager GrpcSettings { get; }
    protected CallInvoker? Channel => GrpcSettings.Channel;
    protected uint Timeout => GrpcSettings.RpcTimeoutMs;
        
    #region Protected methods
    protected CallOptions GetCallOptions()
    {
        return new CallOptions(deadline: GetDeadline(), headers: GrpcSettings.Headers);
    }

    protected static FieldMask GetMask(TProtoType? oldValue, TProtoType newValue)
    {
        IEnumerable<string> paths;

        if (oldValue == null)
        {
            paths = newValue.GetType().GetProperties().Select(p => CaseConverter.Converters.ToSnakeCase(p.Name));
        }
        else
        {
            paths = GetNonEqualPropertiesSnakeCase(oldValue, newValue)!;
        }

        if (paths == null)
            throw new NullReferenceException($"Failed to build update mask of {newValue.GetType()}");

        paths = paths.Where(p =>
            !p.StartsWith("has_") &&
            p != "parser" &&
            p != "descriptor" &&
            p != "create_date" &&
            p != "creation_date");

        var mask = new FieldMask();
        mask.Paths.Add(paths);
        return mask;
    }

    protected TResult CallWithOptions<TInput, TResult>(Func<TInput, CallOptions, TResult> method, TInput input)
    {
        return method.Invoke(input, GetCallOptions());
    }

    protected DateTime GetDeadline()
    {
        return DateTime.UtcNow.AddMilliseconds(Timeout);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="toSnakeCase"></param>
    /// <returns>Returns a list of different property names</returns>
    protected static ICollection<string>? GetNonEqualPropertiesSnakeCase<T>(T? a, T? b, bool toSnakeCase = true) where T : class
    {
        if (a == null || b == null)
            return null;

        var fieldMask = new List<string>();

        var aProperties = a.GetType().GetProperties();
        var bProperties = b.GetType().GetProperties();

        foreach (var aProperty in aProperties)
        {
            foreach (var bProperty in bProperties)
            {
                if (aProperty.Name == bProperty.Name)
                {

                    var aValue = aProperty.GetValue(a);
                    var bValue = bProperty.GetValue(b);

                    if (!Equals(aValue, bValue))
                    {
                        if (toSnakeCase)
                            fieldMask.Add(CaseConverter.Converters.ToSnakeCase(aProperty.Name));
                        else
                            fieldMask.Add(aProperty.Name);
                    }

                    break;
                }
            }
        }

        return fieldMask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns>Returns a list of property names</returns>
    protected static ICollection<string>? GetPropertiesSnakeCase<T>(T? obj) where T : class
    {
        if (obj == null)
            return null;

        var propertyNames = Core.Common.GenericExtensions.GetProperties(obj)?.ToList();

        return propertyNames?.Select(x => CaseConverter.Converters.ToSnakeCase(x)).ToList();
    }
    #endregion Private methods
}