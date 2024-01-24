using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record JobBillingSubmissionSource(
                    Guid ProviderBillingId,
                    DateTime SubmitOnDate,
                    string SubmissionSource)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, submit_on_date, submission_source
	FROM provider_billing.jobbilling_submission_source 
    where provider_billing_id = @id;";

    internal static async Task<Lst<JobBillingSubmissionSource>> ReadAsync(NpgsqlDataReader reader)
    {
        List<JobBillingSubmissionSource> items = new List<JobBillingSubmissionSource>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.JobBillingSubmissionSource(
                reader.GetGuid("provider_billing_id"),
                reader.GetDateTime("submit_on_date"),
                reader.GetString("submission_source")
                ));
        }

        return items.Freeze();
    }
}
