using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record PaymentElectronicFundTransfer(
                    DateTime PaidAtDateTime,
                    string? TransactionReferenceCode,
                    Guid JobWorkId,
                    Guid ProviderBillingId,
                    string Method)
{
    internal static string Sql { get; } = @"SELECT paid_at_date_time, transaction_reference_code, job_work_id, provider_billing_id, method
	FROM provider_billing.payment_electronic_fund_transfer 
    where provider_billing_id = @id;";

    internal static async Task<Lst<PaymentElectronicFundTransfer>> ReadAsync(NpgsqlDataReader reader)
    {
        List<PaymentElectronicFundTransfer> items = new List<PaymentElectronicFundTransfer>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.PaymentElectronicFundTransfer(
                reader.GetDateTime("paid_at_date_time"),
                reader.SafeGetString("transaction_reference_code"),
                reader.GetGuid("job_work_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetString("method")));
        }

        return items.Freeze();
    }
}
