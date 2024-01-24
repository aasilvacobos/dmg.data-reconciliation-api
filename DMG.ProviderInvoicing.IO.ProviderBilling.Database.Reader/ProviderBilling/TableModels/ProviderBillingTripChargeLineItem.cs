using LanguageExt;
using Npgsql;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record ProviderBillingTripChargeLineItem(
                    Guid ProviderBillingId,
                    Guid LineItemId,
                    string Name,
                    decimal Rate,
                    decimal Amount,
                    decimal Quantity,
                    int MaximumChargeableTrips,
                    string Source)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, line_item_id, name, rate, amount, quantity, maximum_chargeable_trips, source
	FROM provider_billing.trip_charge_line_item 
    where provider_billing_id = @id;";

    internal static async Task<Lst<ProviderBillingTripChargeLineItem>> ReadAsync(NpgsqlDataReader reader)
    {
        List<ProviderBillingTripChargeLineItem> items = new List<ProviderBillingTripChargeLineItem>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.ProviderBillingTripChargeLineItem(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("line_item_id"),
                reader.GetString("name"),
                reader.GetDecimal("rate"),
                reader.GetDecimal("amount"),
                reader.GetDecimal("quantity"),
                reader.GetInt32("maximum_chargeable_trips"),
                reader.GetString("source")));
        }

        return items.Freeze();
    }
}
