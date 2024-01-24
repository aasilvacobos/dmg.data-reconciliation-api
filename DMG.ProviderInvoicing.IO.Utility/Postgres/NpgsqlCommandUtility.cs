using LanguageExt;
using static LanguageExt.Prelude;
using Npgsql;
using NpgsqlTypes;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.IO.Utility.Postgres;


/// Utility to improve type safety when adding Postgres parameters
public static class NpgsqlCommandUtility
{
    /// Add parameter value of type Char (string) to a NpgsqlParameterCollection.
    public static void ParametersAddValueChar(NpgsqlParameterCollection parameters, string parameterName, Option<string> parameterValue) =>
        parameters.AddWithValue(parameterName, NpgsqlDbType.Char, parameterValue.Match(val => val, () => string.Empty)); // Postgres requires empty string
    
    public static void ParametersAddValueChar(NpgsqlParameterCollection parameters, string parameterName, Option<NonEmptyText> parameterValue) =>
        parameters.AddWithValue(parameterName, NpgsqlDbType.Char, parameterValue.Match(val => val.Value, () => string.Empty)); // Postgres requires empty string

    /// Add parameter value of type Jsonb to a NpgsqlParameterCollection.
    public static void ParametersAddValueJsonb(NpgsqlParameterCollection parameters, string parameterName, Option<string> parameterValue) =>
        parameters.AddWithValue(parameterName, NpgsqlDbType.Jsonb, parameterValue.Match(val => val, () => DBNull.Value as object));

    /// Add parameter value of type Uuid to a NpgsqlParameterCollection.
    public static void ParametersAddValueUuid(NpgsqlParameterCollection parameters, string parameterName, Guid parameterValue) => // TODO use Option
        parameters.AddWithValue(parameterName, NpgsqlDbType.Uuid, parameterValue);

    /// Add parameter value of type Uuid to a NpgsqlParameterCollection.
    public static void ParametersAddValueUuid(NpgsqlParameterCollection parameters, string parameterName, Option<Guid> parameterValue) =>
        parameters.AddWithValue(parameterName, NpgsqlDbType.Uuid, parameterValue.Match(val => val, () => DBNull.Value as object));

    public static void ParametersAddValueUuid(NpgsqlParameterCollection parameters, string parameterName, Guid? parameterValue) =>
        parameters.AddWithValue(parameterName, NpgsqlDbType.Uuid, parameterValue.Match(val => val, () => DBNull.Value as object));


    /// Add parameter value of type Numeric to a NpgsqlParameterCollection.
    public static void ParametersAddValueNumeric(NpgsqlParameterCollection parameters, string parameterName, Option<decimal> parameterValue) =>
        parameters.AddWithValue(parameterName, NpgsqlDbType.Numeric, parameterValue.Match(val => val, () => DBNull.Value as object));

    /// Add parameter value of type Integer to a NpgsqlParameterCollection.
    public static void ParametersAddValueInt(NpgsqlParameterCollection parameters, string parameterName, Option<int> parameterValue) =>
        parameters.AddWithValue(parameterName, NpgsqlDbType.Integer, parameterValue.Match(val => val, () => DBNull.Value as object));

    /// Add parameter value of type Bigint to a NpgsqlParameterCollection.
    public static void ParametersAddValueBigInt(NpgsqlParameterCollection parameters, string parameterName, Option<long> parameterValue) =>
        parameters.AddWithValue(parameterName, NpgsqlDbType.Bigint, parameterValue.Match(val => val, () => DBNull.Value as object));

    /// Add parameter value of type Date to a NpgsqlParameterCollection.
    public static void ParametersAddValueDate(NpgsqlParameterCollection parameters, string parameterName, DateTime parameterValue) => // TODO use Option
        parameters.AddWithValue(parameterName, NpgsqlDbType.Date, parameterValue);

    /// Add parameter value of type Date to a NpgsqlParameterCollection.
    public static void ParametersAddValueDate(NpgsqlParameterCollection parameters, string parameterName, Option<DateTimeOffset> parameterValue) => 
        parameters.AddWithValue(parameterName, NpgsqlDbType.Date, parameterValue.Match(val => val.UtcDateTime, () => DBNull.Value as object));

    /// Add parameter value of type Timestamp to a NpgsqlParameterCollection.
    public static void ParametersAddValueTimestamp(NpgsqlParameterCollection parameters, string parameterName, DateTimeOffset? parameterValue) => // TODO use Option
        parameters.AddWithValue(parameterName, NpgsqlDbType.TimestampTz, parameterValue != null ? parameterValue.Value.UtcDateTime : DBNull.Value as object);

    /// Add parameter value of type Bool to a NpgsqlParameterCollection.
    public static void ParametersAddValueBool(NpgsqlParameterCollection parameters, string parameterName, Option<bool> parameterValue) => 
        parameters.AddWithValue(parameterName, NpgsqlDbType.Boolean, parameterValue.Match(val => val, () => DBNull.Value as object));

    public static void ParametersAddValueBit(NpgsqlParameterCollection parameters, string parameterName, Option<bool> parameterValue) =>
        parameters.AddWithValue(parameterName, NpgsqlDbType.Bit, parameterValue.Match(val => val, () => DBNull.Value as object));
}
