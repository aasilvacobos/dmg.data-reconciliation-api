using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record LaborLineItem(
                    Guid ProviderBillingId,
                    Guid ItemId,
                    string RateType,
                    string TechnicianType,
                    decimal Hours,
                    decimal LaborRate,
                    decimal Cost)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, item_id, rate_type, technician_type, hours, labor_rate, cost
	FROM provider_billing.labor_line_item 
    where provider_billing_id = @id;";

    internal static async Task<Lst<LaborLineItem>> ReadAsync(NpgsqlDataReader reader)
    {
        List<LaborLineItem> items = new List<LaborLineItem>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.LaborLineItem(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("item_id"),
                reader.GetString("rate_type"),
                reader.GetString("technician_type"),
                reader.GetDecimal("hours"),
                reader.GetDecimal("labor_rate"),
                reader.GetDecimal("cost")));
        }

        return items.Freeze();
    }
}
