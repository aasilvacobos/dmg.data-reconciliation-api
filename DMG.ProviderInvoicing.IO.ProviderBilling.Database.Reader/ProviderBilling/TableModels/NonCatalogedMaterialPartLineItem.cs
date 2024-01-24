using System.Data;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using LanguageExt;
using static LanguageExt.Prelude;
using Npgsql;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record NonCatalogedMaterialPartLineItem(
                    Guid ProviderBillingId,
                    Guid ItemId,
                    decimal ItemCost,
                    decimal Quantity,
                    string CreationSource,
                    string? Reason,
                    string MaterialPartKind,
                    string Name,
                    Guid? JobWorkId)
{
    public static string Sql { get;  } = @"select n.name, m.provider_billing_id, m.item_id, m.item_cost, m.quantity, m.creation_source, m.reason, m.material_part_kind, a.job_work_id
from provider_billing.material_part_line_item m 
join provider_billing.non_cataloged_material_part_line_item n on m.item_id = n.material_part_line_item_id and m.provider_billing_id = n.provider_billing_id
left join provider_billing.equipment_job_assignment a on m.item_id = a.item_id
    where n.provider_billing_id = @id;";

    internal static async Task<Lst<NonCatalogedMaterialPartLineItem>> ReadAsync(NpgsqlDataReader reader)
    {
        List<NonCatalogedMaterialPartLineItem> nonMaterialParts = new List<NonCatalogedMaterialPartLineItem>();

        while (await reader.ReadAsync())
        {
            nonMaterialParts.Add(new NonCatalogedMaterialPartLineItem(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("item_id"),
                reader.GetDecimal("item_cost"),
                reader.GetDecimal("quantity"),
                reader.GetString("creation_source"),
                reader.SafeGetString("reason"),
                reader.GetString("material_part_kind"),
                reader.GetString("name"),
                reader.SafeGetGuid("job_work_id")
                ));
        }

        return nonMaterialParts.Freeze();
    }
}
