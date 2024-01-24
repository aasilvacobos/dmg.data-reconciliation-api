using System.Data;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using LanguageExt;
using static LanguageExt.Prelude;
using Npgsql;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record CatalogedEquipmentLineItem(
                    Guid ProviderBillingId,
                    Guid ItemId,
                    decimal ItemCost,
                    decimal Quantity,
                    string CreationSource,
                    string? Reason,
                    string EquipmentKind,
                    Guid CatalogItemReference,
                    Guid? JobWorkId)
{
    public static string Sql { get; } = @"select e.provider_billing_id, e.item_id, e.item_cost, e.quantity, e.creation_source, e.reason, e.equipment_kind, c.catalog_item_reference, a.job_work_id
from provider_billing.equipment_line_item e
join provider_billing.cataloged_equipment c on e.item_id = c.equipment_line_item_id and e.provider_billing_id = c.provider_billing_id
left join provider_billing.equipment_job_assignment a on e.item_id = a.item_id
    where c.provider_billing_id = @id;";

    internal static async Task<Lst<CatalogedEquipmentLineItem>> ReadAsync(NpgsqlDataReader reader)
    {
        List<CatalogedEquipmentLineItem> equipmentLineItems = new List<CatalogedEquipmentLineItem>();

        while (await reader.ReadAsync())
        {
            equipmentLineItems.Add(new CatalogedEquipmentLineItem(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("item_id"),
                reader.GetDecimal("item_cost"),
                reader.GetDecimal("quantity"),
                reader.GetString("creation_source"),
                reader.SafeGetString("reason"),
                reader.GetString("equipment_kind"),
                reader.GetGuid("catalog_item_reference"),
                reader.SafeGetGuid("job_work_id")
                ));
        }

        return equipmentLineItems.Freeze();
    }
}