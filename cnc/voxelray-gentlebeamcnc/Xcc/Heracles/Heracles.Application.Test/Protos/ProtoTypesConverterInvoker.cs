using Heracles.Application.Protos;
using Heracles.Application.Test.TestUtils;

namespace Heracles.Application.Test.Protos
{
    ///// <summary>
    ///// Particular static method invocation utility class via reflection
    ///// for ProtoTypesConverterInvokerTemplate's overloaded ToProto/FromProto methods
    ///// </summary>
    ///// <typeparam name="TClass"></typeparam>
    ///// <typeparam name="TConverterClass"></typeparam>
    public class ProtoTypesConverterInvokerTemplate<TClass, TConverterClass>
        where TConverterClass : class
    {
        protected static object? InvokeConverter(string methodName, TClass value)
        {
            return ReflectionHelper<TConverterClass>.InvokeStaticWithParam(methodName, value);
        }

        public virtual object? ToProto(TClass value)
        {
            return InvokeConverter("ToProto", value);
        }

        public virtual object? FromProto(TClass value)
        {
            return InvokeConverter("FromProto", value);
        }
    }

    /// <summary>
    /// Particular static method invocation utility class via reflection
    /// for ProtoTypeConverter's overloaded ToProto/FromProto methods
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    public class ProtoTypesConverterInvoker<TClass> : ProtoTypesConverterInvokerTemplate<TClass, ProtoTypesConverter>
    { }
}
