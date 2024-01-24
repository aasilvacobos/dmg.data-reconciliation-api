using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record EquipmentLineItemRule(
                    string Name,
                    Guid EquipmentLineItemId,
                    Guid ProviderBillingId,
                    Guid CatalogItemReference,
                    string? Value)
{
    internal static string Sql { get; } = @"SELECT name, equipment_line_item_id, provider_billing_id, catalog_item_reference, value
	FROM provider_billing.equipment_line_item_rule 
    where provider_billing_id = @id;";

    internal static async Task<Lst<EquipmentLineItemRule>> ReadAsync(NpgsqlDataReader reader)
    {
        List<EquipmentLineItemRule> items = new List<EquipmentLineItemRule>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.EquipmentLineItemRule(
                reader.GetString("name"),
                reader.GetGuid("equipment_line_item_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("catalog_item_reference"),
                reader.SafeGetString("value")));
        }

        return items.Freeze();
    }
}
