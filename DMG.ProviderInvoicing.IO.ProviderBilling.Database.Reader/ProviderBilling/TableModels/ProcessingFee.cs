using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record ProcessingFee(
                    string Description,
                    decimal ProcessingFeeValue,
                    Guid ProviderBillingId)
{
    internal static string Sql { get; } = @"SELECT description, processing_fee, provider_billing_id
	FROM provider_billing.processing_fee 
    where provider_billing_id = @id;";

    internal static async Task<Lst<ProcessingFee>> ReadAsync(NpgsqlDataReader reader)
    {
        List<ProcessingFee> items = new List<ProcessingFee>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.ProcessingFee(
                reader.GetString("description"),
                reader.GetDecimal("processing_fee"),
                reader.GetGuid("provider_billing_id")));
        }

        return items.Freeze();
    }
}
