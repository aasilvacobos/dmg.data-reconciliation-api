using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record TripChargeLineItem(
                    Guid ProviderBillingId,
                    string Description,
                    DateTime RequestedByDate,
                    DateTime ArrivalDate,
                    decimal TripCost,
                    bool IsTripPayable,
                    bool IsRequiredByDateMissed,
                    string CreationSource,
                    string Rate)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, description, requested_by_date, arrival_date, trip_cost, is_trip_payable, is_required_by_date_missed, creation_source, rate
	FROM provider_billing.job_billing_trip_charge_line_item 
    where provider_billing_id = @id;";

    internal static async Task<Lst<TripChargeLineItem>> ReadAsync(NpgsqlDataReader reader)
    {
        List<TripChargeLineItem> items = new List<TripChargeLineItem>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.TripChargeLineItem(
                reader.GetGuid("provider_billing_id"),
                reader.GetString("description"),
                reader.GetDateTime("requested_by_date"),
                reader.GetDateTime("arrival_date"),
                reader.GetDecimal("trip_cost"),
                reader.GetBoolean("is_trip_payable"),
                reader.GetBoolean("is_required_by_date_missed"),
                reader.GetString("creation_source"),
                reader.GetString("rate")));
        }

        return items.Freeze();
    }
}
