using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMG.ProviderInvoicing.IO.Utility.Postgres;

public static class NpgsqlReader
{
    public static string? SafeGetString(this NpgsqlDataReader reader, string colName) =>
        reader.IsDBNull(colName) ? null : reader.GetString(colName);

    public static Guid? SafeGetGuid(this NpgsqlDataReader reader, string colName) =>
        reader.IsDBNull(colName) ? null : reader.GetGuid(colName);

    public static DateTime? SafeGetDateTime(this NpgsqlDataReader reader, string colName) =>
        reader.IsDBNull(colName) ? null : reader.GetDateTime(colName);

    public static decimal? SafeGetDecimal(this NpgsqlDataReader reader, string colName) =>
        reader.IsDBNull(colName) ? null : reader.GetDecimal(colName);
}