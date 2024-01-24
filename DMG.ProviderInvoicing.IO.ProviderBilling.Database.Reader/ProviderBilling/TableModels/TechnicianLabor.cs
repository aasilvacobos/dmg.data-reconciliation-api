using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record TechnicianLabor(
                    Guid TechnicianLaborOnSiteId,
                    long TotalBillableTimeSeconds,
                    Guid TechnicianUserId,
                    Guid TechnicianLaborLineId,
                    string LineItemSource,
                    bool IsPayable,
                    string TechnicianType,
                    string RateType,
                    decimal LaborRate,
                    Guid ProviderBillingId,
                    Guid LaborItemId)
{
    internal static string Sql { get; } = @"SELECT technician_labor_on_site_id, total_billable_time_seconds, technician_user_id, technician_labor_line_id, line_item_source, is_payable, technician_type, rate_type, labor_rate, provider_billing_id, labor_item_id
	FROM provider_billing.technician_labor 
    where provider_billing_id = @id;";

    internal static async Task<Lst<TechnicianLabor>> ReadAsync(NpgsqlDataReader reader)
    {
        List<TechnicianLabor> items = new List<TechnicianLabor>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.TechnicianLabor(
                reader.GetGuid("technician_labor_on_site_id"),
                reader.GetInt64("total_billable_time_seconds"),
                reader.GetGuid("technician_user_id"),
                reader.GetGuid("technician_labor_line_id"),
                reader.GetString("line_item_source"),
                reader.GetBoolean("is_payable"),
                reader.GetString("technician_type"),
                reader.GetString("rate_type"),
                reader.GetDecimal("labor_rate"),
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("labor_item_id")));
        }

        return items.Freeze();
    }
}
