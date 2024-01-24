using System.Data;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using LanguageExt;
using static LanguageExt.Prelude;
using Npgsql;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record CatalogedMaterialPartLineItem(
                    Guid ProviderBillingId,
                    Guid ItemId,
                    decimal ItemCost,
                    decimal Quantity,
                    string CreationSource,
                    string? Reason,
                    string MaterialPartKind,
                    Guid CatalogItemReference,
                    Guid? JobWorkId)
{
    public static string Sql { get; } = @"select c.catalog_item_reference, m.provider_billing_id, m.item_id, m.item_cost, m.quantity, m.creation_source, m.reason, m.material_part_kind, a.job_work_id 
from provider_billing.material_part_line_item m 
join provider_billing.cataloged_material_part_line_item c on m.item_id = c.material_part_line_item_id and m.provider_billing_id = c.provider_billing_id
left join provider_billing.equipment_job_assignment a on m.item_id = a.item_id
where c.provider_billing_id = @id;";

    internal static async Task<Lst<CatalogedMaterialPartLineItem>> ReadAsync(NpgsqlDataReader reader)
    {
        List<CatalogedMaterialPartLineItem> materialParts = new List<CatalogedMaterialPartLineItem>();

        while (await reader.ReadAsync())
        {
            materialParts.Add(new CatalogedMaterialPartLineItem(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("item_id"),
                reader.GetDecimal("item_cost"),
                reader.GetDecimal("quantity"),
                reader.GetString("creation_source"),
                reader.SafeGetString("reason"),
                reader.GetString("material_part_kind"),
                reader.GetGuid("catalog_item_reference"),
                reader.SafeGetGuid("job_work_id")
                ));
        }

        return materialParts.Freeze();
    }
}