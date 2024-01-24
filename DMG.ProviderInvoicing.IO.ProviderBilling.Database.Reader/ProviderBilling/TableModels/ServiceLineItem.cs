using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record ServiceLineItem(
                    Guid PerOccurrenceItemId,
                    Guid ItemId,
                    int ItemSequenceNumber,
                    Guid ProviderBillingId,
                    Guid VisitId,
                    string? ItemName,
                    DateTime DateTime,
                    decimal ServiceRate,
                    string? Reason,
                    decimal Cost,
                    string Type,
                    string? RemovedReason,
                    string Source,
                    string Status,
                    Guid ServiceTypeId,
                    bool? Verified,
                    string ModifyAction,
                    string ModifyBy)
{
    internal static string Sql { get; } = @"SELECT per_occurrence_item_id, item_id, item_sequence_number, provider_billing_id, visit_id, item_name, date_time, service_rate, reason, cost, type, removed_reason, source, status, service_type_id, verified, modify_action, modify_by
    FROM provider_billing.service_line_item 
    where provider_billing_id = @id
    order by date_time asc;";

    internal static async Task<Lst<ServiceLineItem>> ReadAsync(NpgsqlDataReader reader)
    {
        List<ServiceLineItem> items = new List<ServiceLineItem>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.ServiceLineItem(
                reader.GetGuid("per_occurrence_item_id"),
                reader.GetGuid("item_id"),
                reader.GetInt32("item_sequence_number"),
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("visit_id"),
                reader.SafeGetString("item_name"),
                reader.GetDateTime("date_time"),
                reader.GetDecimal("service_rate"),
                reader.SafeGetString("reason"),
                reader.GetDecimal("cost"),
                reader.GetString("type"),
                reader.SafeGetString("removed_reason"),
                reader.GetString("source"),
                reader.GetString("status"),
                reader.GetGuid("service_type_id"),
                reader.GetBoolean("verified"),
                reader.GetString("modify_action"),
                reader.GetString("modify_by")));
        }

        return items.Freeze();
    }
}
