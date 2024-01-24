using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record Discount(
                    Guid ProviderBillingId,
                    Guid DiscountId,
                    decimal DiscountAmount,
                    string Source,
                    string Reason,
                    string Name,
                    DateTime ClientTime)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, discount_id, discount, source, reason, name, client_time
	FROM provider_billing.discount 
    where provider_billing_id = @id;";

    internal static async Task<Lst<Discount>> ReadAsync(NpgsqlDataReader reader)
    {
        List<Discount> items = new List<Discount>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.Discount(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("discount_id"),
                reader.GetDecimal("discount"),
                reader.GetString("source"),
                reader.GetString("reason"),
                reader.GetString("name"),
                reader.GetDateTime("client_time")));
        }

        return items.Freeze();
    }
}
