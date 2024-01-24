using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record PaymentCreditCard(
                    string CreditCardProvider,
                    DateTime PaidAtDateTime,
                    string Last4Digits,
                    string? TransactionReferenceCode,
                    Guid JobWorkId,
                    Guid ProviderBillingId,
                    string Method)
{
    internal static string Sql { get; } = @"SELECT credit_card_provider, paid_at_date_time, last4_digits, transaction_reference_code, job_work_id, provider_billing_id, method
	FROM provider_billing.payment_credit_card 
    where provider_billing_id = @id;";

    internal static async Task<Lst<PaymentCreditCard>> ReadAsync(NpgsqlDataReader reader)
    {
        List<PaymentCreditCard> items = new List<PaymentCreditCard>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.PaymentCreditCard(
                reader.GetString("credit_card_provider"),
                reader.GetDateTime("paid_at_date_time"),
                reader.GetString("last4_digits"),
                reader.SafeGetString("transaction_reference_code"),
                reader.GetGuid("job_work_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetString("method")));
        }

        return items.Freeze();
    }
}
