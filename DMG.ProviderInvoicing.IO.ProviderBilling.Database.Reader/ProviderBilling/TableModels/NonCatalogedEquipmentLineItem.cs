using System.Data;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using LanguageExt;
using static LanguageExt.Prelude;
using Npgsql;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record NonCatalogedEquipmentLineItem(
                    Guid ProviderBillingId,
                    Guid ItemId,
                    decimal ItemCost,
                    decimal Quantity,
                    string CreationSource,
                    string? Reason,
                    string EquipmentKind,
                    string Name,
                    Guid? JobWorkId)
{
    internal static string? Sql { get; } = @"select e.provider_billing_id, e.item_id, e.item_cost, e.quantity, e.creation_source, e.reason, e.equipment_kind, n.name, a.job_work_id
from provider_billing.equipment_line_item e 
join provider_billing.non_cataloged_equipment_line_item n on e.item_id = n.equipment_line_item_id and e.provider_billing_id = n.provider_billing_id
left join provider_billing.equipment_job_assignment a on e.item_id = a.item_id
    where n.provider_billing_id = @id;";

    internal static async Task<Lst<NonCatalogedEquipmentLineItem>> ReadAsync(NpgsqlDataReader reader)
    {
        List<NonCatalogedEquipmentLineItem> equipmentLineItems = new List<NonCatalogedEquipmentLineItem>();

        while (await reader.ReadAsync())
        {
            equipmentLineItems.Add(new NonCatalogedEquipmentLineItem(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("item_id"),
                reader.GetDecimal("item_cost"),
                reader.GetDecimal("quantity"),
                reader.GetString("creation_source"),
                reader.SafeGetString("reason"),
                reader.GetString("equipment_kind"),
                reader.GetString("name"),
                reader.SafeGetGuid("job_work_id")
                ));
        }

        return equipmentLineItems.Freeze();
    }
}