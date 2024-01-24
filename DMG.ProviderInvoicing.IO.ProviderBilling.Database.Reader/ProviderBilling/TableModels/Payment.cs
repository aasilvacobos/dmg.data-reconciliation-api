using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record Payment(
                    Guid JobWorkId,
                    Guid ProviderBillingId,
                    string Method,
                    decimal TotalAmountPaid,
                    string? PaymentTerms)
{
    internal static string Sql { get; } = @"SELECT job_work_id, provider_billing_id, method, total_amount_paid, payment_terms
	FROM provider_billing.payment 
    where provider_billing_id = @id;";

    internal static async Task<Lst<Payment>> ReadAsync(NpgsqlDataReader reader)
    {
        List<Payment> items = new List<Payment>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.Payment(
                reader.GetGuid("job_work_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetString("method"),
                reader.GetDecimal("total_amount_paid"),
                reader.SafeGetString("payment_terms")));
        }

        return items.Freeze();
    }
}
