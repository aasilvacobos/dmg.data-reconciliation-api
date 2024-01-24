using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record MaterialPartLineItemRule(
                    string Name,
                    Guid MaterialPartLineItemId,
                    Guid ProviderBillingId,
                    Guid CatalogItemReference,
                    string? Value)
{
    internal static string Sql { get; } = @"SELECT name, material_part_line_item_id, provider_billing_id, catalog_item_reference, value
	FROM provider_billing.material_part_line_item_rule 
    where provider_billing_id = @id;";

    internal static async Task<Lst<MaterialPartLineItemRule>> ReadAsync(NpgsqlDataReader reader)
    {
        List<MaterialPartLineItemRule> items = new List<MaterialPartLineItemRule>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.MaterialPartLineItemRule(
                reader.GetString("name"),
                reader.GetGuid("material_part_line_item_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("catalog_item_reference"),
                reader.SafeGetString("value")));
        }

        return items.Freeze();
    }
}
