using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record ProviderBillingAssignment(
                    Guid ProviderBillingId,
                    DateTime? ProviderFirstSubmittedOnDate,
                    DateTime? ProviderLastSubmittedOnDate,
                    string AssignedTo)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, provider_first_submitted_on_date, provider_last_submitted_on_date, assigned_to
	FROM provider_billing.provider_billing_assignment 
    where provider_billing_id = @id;";

    internal static async Task<Option<ProviderBillingAssignment>> ReadAsync(NpgsqlDataReader reader)
    {
        while (await reader.ReadAsync())
        {
            return new ProviderBillingAssignment(
                reader.GetGuid("provider_billing_id"),
                reader.SafeGetDateTime("provider_first_submitted_on_date"),
                reader.SafeGetDateTime("provider_last_submitted_on_date"),
                reader.GetString("assigned_to")
                );
        }

        return Option<ProviderBillingAssignment>.None;
    }
}
