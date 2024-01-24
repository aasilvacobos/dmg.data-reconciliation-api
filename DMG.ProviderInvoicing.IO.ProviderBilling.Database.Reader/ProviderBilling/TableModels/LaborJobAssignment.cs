using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record LaborJobAssignment(
                    Guid JobWorkId,
                    Guid ProviderBillingId,
                    Guid ItemId)
{
    internal static string Sql { get; } = @"SELECT job_work_id, provider_billing_id, item_id
	FROM provider_billing.labor_job_assignment 
    where provider_billing_id = @id;";

    internal static async Task<Lst<LaborJobAssignment>> ReadAsync(NpgsqlDataReader reader)
    {
        List<LaborJobAssignment> items = new List<LaborJobAssignment>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.LaborJobAssignment(
                reader.GetGuid("job_work_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("item_id")));
        }

        return items.Freeze();
    }
}
