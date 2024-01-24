using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record MultiVisitJob(
                    Guid MultiVisitJobId,
                    Guid ProviderBillingId,
                    Guid ServiceBasedCostingId,
                    string Name,
                    decimal Rate,
                    string UnitType)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, multi_visit_job_id, service_based_costing_id, name, rate,unit_type
	FROM provider_billing.multi_visit_job 
    where provider_billing_id = @id;";

    internal static async Task<Lst<MultiVisitJob>> ReadAsync(NpgsqlDataReader reader)
    {
        List<MultiVisitJob> items = new List<MultiVisitJob>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.MultiVisitJob(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("multi_visit_job_id"),
                reader.GetGuid("service_based_costing_id"),
                reader.GetString("name"),
                reader.GetDecimal("rate"),
                reader.GetString("unit_type")));
        }
        return items.Freeze();
    }
}
