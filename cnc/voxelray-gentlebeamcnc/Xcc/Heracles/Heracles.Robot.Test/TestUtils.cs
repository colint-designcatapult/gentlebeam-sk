using System.Reflection;

namespace Heracles.Application.Test.TestUtils
{
    /// <summary>
    /// The utility class to provide indirect calls for class methods via reflection
    /// as a workaround for testing particular overloaded methods from a generic test class
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    public class ReflectionHelper<TClass>
        where TClass : class
    {
        public static TOut? InvokeStaticWithParam<TParam, TOut>(string methodName, TParam param)
        {
            return (TOut?)InvokeStaticWithParam<TParam>(methodName, param);
        }

        public static object? InvokeStaticWithParam<TParam>(string methodName, TParam param)
        {
            try
            {
                var method = typeof(TClass).GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.Static,
                    new Type[] { typeof(TParam) }
                    );
                if (method == null)
                {
                    var message = $"Can't find a public method {methodName} in the {nameof(TClass)}";
                    Assert.Fail(message);
                    throw new InvalidOperationException(message);
                }
                else
                {
                    return method.Invoke(null, new object[] { param! });
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                else
                    throw;
            }
        }
    }
}
