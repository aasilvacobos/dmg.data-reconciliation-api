using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record Adjustment(
                    Guid AdjustmentId,
                    Guid ProviderBillingId,
                    Guid EntityId,
                    string EntityType,
                    decimal Amount,
                    decimal Quantity,
                    string CreationSource)
{
    internal static string Sql { get; } = @"SELECT adjustment_id, provider_billing_id, entity_id, entity_type, amount, quantity, creation_source
	FROM provider_billing.adjustment 
    where provider_billing_id = @id;";

    internal static async Task<Lst<Adjustment>> ReadAsync(NpgsqlDataReader reader)
    {
        List<Adjustment> items = new List<Adjustment>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.Adjustment(
                reader.GetGuid("adjustment_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("entity_id"),
                reader.GetString("entity_type"),
                reader.GetDecimal("amount"),
                reader.GetDecimal("quantity"),
                reader.GetString("creation_source")));
        }

        return items.Freeze();
    }
}
