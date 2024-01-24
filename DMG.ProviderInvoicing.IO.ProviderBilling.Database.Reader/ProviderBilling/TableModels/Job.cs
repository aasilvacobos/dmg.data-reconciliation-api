using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record Job(
                    DateTime? JobCompleteDateTime,
                    Guid JobWorkId,
                    Guid ProviderBillingId,
                    string Urgency,
                    string JobWorkNumber,
                    string Scope,
                    Guid ServiceTypeId)
{
    internal static string Sql { get; } = @"SELECT job_complete_date_time, job_work_id, provider_billing_id, urgency, job_work_number, scope, service_type_id
	FROM provider_billing.job 
    where provider_billing_id = @id;";

    internal static async Task<Lst<Job>> ReadAsync(NpgsqlDataReader reader)
    {
        List<Job> items = new List<Job>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.Job(
                reader.SafeGetDateTime("job_complete_date_time"),
                reader.GetGuid("job_work_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetString("urgency"),
                reader.GetString("job_work_number"),
                reader.GetString("scope"),
                reader.GetGuid("service_type_id")));
        }

        return items.Freeze();
    }
}
