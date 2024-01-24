using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record Dispute(
                    Guid ProviderBillingId,
                    Guid DisputeId,
                    Guid EntityId,
                    string EntityType,
                    string? ReasonText,
                    DateTime? ResolvedAt)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, dispute_id, entity_id, entity_type, reason_text, resolved_at
	FROM provider_billing.dispute 
    where provider_billing_id = @id;";

    internal static async Task<Lst<Dispute>> ReadAsync(NpgsqlDataReader reader)
    {
        List<Dispute> items = new List<Dispute>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.Dispute(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("dispute_id"),
                reader.GetGuid("entity_id"),
                reader.GetString("entity_type"),
                reader.SafeGetString("reason_text"),
                reader.SafeGetDateTime("resolved_at")));
        }

        return items.Freeze();
    }
}
