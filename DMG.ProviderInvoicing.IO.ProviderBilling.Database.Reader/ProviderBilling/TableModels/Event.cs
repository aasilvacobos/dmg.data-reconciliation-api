using DMG.ProviderInvoicing.IO.Utility.Postgres;
using LanguageExt;
using Npgsql;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record Event(
                    Guid EventId,
                    Guid EventLineItemId,
                    Guid ServiceItemId,
                    Guid ProviderBillingId,
                    string Name,
                    string Source,
                    string? Reason,
                    decimal Amount,
                    DateTime EventStart,
                    DateTime EventEnd,
                    string Description,
                    Guid SourceId,
                    string ModifyBy,
                    string ModifyAction)
{
    internal static string Sql { get; } = @"SELECT event_id, event_line_item_id, service_item_id, provider_billing_id, name, source, reason, amount, event_start, event_end, description, source_id, modify_by, modify_action
	FROM provider_billing.event
    WHERE provider_billing_id = @id;";

    internal static async Task<Lst<Event>> ReadAsync(NpgsqlDataReader reader)
    {
        List<Event> items = new List<Event>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.Event(
                reader.GetGuid("event_id"),
                reader.GetGuid("event_line_item_id"),
                reader.GetGuid("service_item_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetString("name"),
                reader.GetString("source"),
                reader.SafeGetString("reason"),
                reader.GetDecimal("amount"),
                reader.GetDateTime("event_start"),
                reader.GetDateTime("event_end"),
                reader.GetString("description"),
                reader.GetGuid("source_id"),
                reader.GetString("modify_by"),
                reader.GetString("modify_action")
                ));
        }

        return items.Freeze();
    }
}