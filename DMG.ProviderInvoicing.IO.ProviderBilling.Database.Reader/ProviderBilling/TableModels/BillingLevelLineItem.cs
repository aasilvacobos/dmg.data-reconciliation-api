using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record BillingLevelLineItem(
                    int SequenceNumber,
                    Guid ItemId,
                    Guid ProviderBillingId,
                    string ItemName,
                    string Type,
                    decimal Price,
                    decimal Quantity,
                    DateTime AddedAt,
                    string Source,
                    string Status,
                    string ModifyBy,
                    string ModifyAction)
{
    internal static string Sql { get; } = @"SELECT sequence_number, item_id, provider_billing_id, item_name, type, price, quantity, added_at, source, status, modify_by, modify_action
	FROM provider_billing.billing_level_line_item 
    where provider_billing_id = @id;";

    internal static async Task<Lst<BillingLevelLineItem>> ReadAsync(NpgsqlDataReader reader)
    {
        List<BillingLevelLineItem> items = new List<BillingLevelLineItem>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.BillingLevelLineItem(
                reader.GetInt32("sequence_number"),
                reader.GetGuid("item_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetString("item_name"),
                reader.GetString("type"),
                reader.GetDecimal("price"),
                reader.GetDecimal("quantity"),
                reader.GetDateTime("added_at"),
                reader.GetString("source"),
                reader.GetString("status"),
                reader.GetString("modify_by"),
                reader.GetString("modify_action")));
        }

        return items.Freeze();
    }
}
