using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record TimeAndMaterialLineItem(
    Guid ProviderBillingId,
    Guid VisitId,
    string? Reason,
    Guid ServiceItemId,
    Guid ItemId,
    DateTime ClientTime,
    int ItemSequenceNumber,
    string Source,
    decimal Quantity,
    decimal Amount,
    string ServiceItemSource,
    string UnitType,
    decimal Rate,
    string RateUnitType,
    string ModifyAction,
    string ModifyBy)
{
    internal static string Sql { get; } =
        @"SELECT provider_billing_id, visit_id, reason, service_item_id, item_id, date_time, item_sequence_number, source, quantity, amount, service_item_source, unit_type,rate,rate_unit_type,modify_action, modify_by
    FROM provider_billing.time_and_material_line_item
    WHERE provider_billing_id = @id;";

    internal static async Task<Lst<TimeAndMaterialLineItem>> ReadAsync(NpgsqlDataReader reader)
    {
        List<TimeAndMaterialLineItem> items = new List<TimeAndMaterialLineItem>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.TimeAndMaterialLineItem(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("visit_id"),
                reader.SafeGetString("reason"),
                reader.GetGuid("service_item_id"),
                reader.GetGuid("item_id"),
                reader.GetDateTime("date_time"),
                reader.GetInt32("item_sequence_number"),
                reader.GetString("source"),
                reader.GetDecimal("quantity"),
                reader.GetDecimal("amount"),
                reader.GetString("service_item_source"),
                reader.GetString("unit_type"),
                reader.GetDecimal("rate"),
                reader.GetString("rate_unit_type"),
                reader.GetString("modify_action"),
                reader.GetString("modify_by")));
        }

        return items.Freeze();
    }
}