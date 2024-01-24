using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record LaborTimeAdjustment(
                    Guid ItemId,
                    Guid ProviderBillingId,
                    int TimeAdjustmentInMinutes,
                    bool IsProviderConfirmed,
                    string CreationSource,
                    string? Reason)
{
    internal static string Sql { get; } = @"SELECT item_id, provider_billing_id, time_adjustment_in_minutes, is_provider_confirmed, creation_source, reason
	FROM provider_billing.labor_time_adjustment 
    where provider_billing_id = @id;";

    internal static async Task<Lst<LaborTimeAdjustment>> ReadAsync(NpgsqlDataReader reader)
    {
        List<LaborTimeAdjustment> items = new List<LaborTimeAdjustment>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.LaborTimeAdjustment(
                reader.GetGuid("item_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetInt32("time_adjustment_in_minutes"),
                reader.GetBoolean("is_provider_confirmed"),
                reader.GetString("creation_source"),
                reader.SafeGetString("reason")));
        }

        return items.Freeze();
    }
}
