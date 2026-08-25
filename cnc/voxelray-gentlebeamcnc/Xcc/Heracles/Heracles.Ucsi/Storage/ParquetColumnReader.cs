using System.Reflection;
using Heracles.Ucsi.Models;
using Parquet;
using Parquet.Schema;

namespace Heracles.Ucsi.Storage;

internal interface IParquetColumnReader
{
    Task ReadAsync(ParquetRowGroupReader reader, int count, CancellationToken cancellationToken);
    object? GetValue(int index);
}

internal static class ParquetColumnReaderFactory
{
    private static readonly MethodInfo CreateValueMethod = GetFactoryMethod(nameof(CreateValue));
    private static readonly MethodInfo CreateNullableValueMethod = GetFactoryMethod(nameof(CreateNullableValue));
    private static readonly MethodInfo CreateEnumMethod = GetFactoryMethod(nameof(CreateEnum));
    private static readonly MethodInfo CreateNullableEnumMethod = GetFactoryMethod(nameof(CreateNullableEnum));

    public static IParquetColumnReader Create(TelemetryParameterDescriptor descriptor, DataField field)
    {
        Type type = descriptor.ValueType;
        if (type == typeof(string))
            return new StringParquetColumnReader(field);

        Type? nullableType = Nullable.GetUnderlyingType(type);
        Type actualType = nullableType ?? type;
        MethodInfo method = actualType.IsEnum
            ? nullableType is null ? CreateEnumMethod : CreateNullableEnumMethod
            : nullableType is null ? CreateValueMethod : CreateNullableValueMethod;
        return (IParquetColumnReader)method.MakeGenericMethod(actualType).Invoke(null, [field])!;
    }

    private static MethodInfo GetFactoryMethod(string name) => typeof(ParquetColumnReaderFactory)
        .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

    private static IParquetColumnReader CreateValue<T>(DataField field)
        where T : struct => new ValueParquetColumnReader<T>(field);

    private static IParquetColumnReader CreateNullableValue<T>(DataField field)
        where T : struct => new NullableValueParquetColumnReader<T>(field);

    private static IParquetColumnReader CreateEnum<T>(DataField field)
        where T : struct, Enum => new EnumParquetColumnReader<T>(field);

    private static IParquetColumnReader CreateNullableEnum<T>(DataField field)
        where T : struct, Enum => new NullableEnumParquetColumnReader<T>(field);
}

internal sealed class ValueParquetColumnReader<T>(DataField field) : IParquetColumnReader
    where T : struct
{
    private T[] _values = [];

    public async Task ReadAsync(ParquetRowGroupReader reader, int count, CancellationToken cancellationToken)
    {
        _values = new T[count];
        await reader.ReadAsync<T>(field, _values, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public object GetValue(int index) => _values[index];
}

internal sealed class NullableValueParquetColumnReader<T>(DataField field) : IParquetColumnReader
    where T : struct
{
    private T?[] _values = [];

    public async Task ReadAsync(ParquetRowGroupReader reader, int count, CancellationToken cancellationToken)
    {
        _values = new T?[count];
        await reader.ReadAsync<T>(field, _values, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public object? GetValue(int index) => _values[index];
}

internal sealed class EnumParquetColumnReader<T>(DataField field) : IParquetColumnReader
    where T : struct, Enum
{
    private int[] _values = [];

    public async Task ReadAsync(ParquetRowGroupReader reader, int count, CancellationToken cancellationToken)
    {
        _values = new int[count];
        await reader.ReadAsync<int>(field, _values, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public object GetValue(int index) => (T)Enum.ToObject(typeof(T), _values[index]);
}

internal sealed class NullableEnumParquetColumnReader<T>(DataField field) : IParquetColumnReader
    where T : struct, Enum
{
    private int?[] _values = [];

    public async Task ReadAsync(ParquetRowGroupReader reader, int count, CancellationToken cancellationToken)
    {
        _values = new int?[count];
        await reader.ReadAsync<int>(field, _values, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public object? GetValue(int index) => _values[index] is int value
        ? (T)Enum.ToObject(typeof(T), value)
        : null;
}

internal sealed class StringParquetColumnReader(DataField field) : IParquetColumnReader
{
    private string?[] _values = [];

    public async Task ReadAsync(ParquetRowGroupReader reader, int count, CancellationToken cancellationToken)
    {
        _values = new string?[count];
        await reader.ReadAsync(field, _values, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public object? GetValue(int index) => _values[index];
}
