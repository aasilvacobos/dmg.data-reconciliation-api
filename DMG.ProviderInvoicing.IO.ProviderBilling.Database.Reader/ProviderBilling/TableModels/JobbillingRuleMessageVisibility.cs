using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record JobBillingRuleMessageVisibility(
                    Guid ProviderBillingId,
                    Guid MessageId,
                    string MessageVisibility)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, message_id, message_visibility
	FROM provider_billing.jobbilling_rule_message_visibility 
    where provider_billing_id = @id;";

    internal static async Task<Lst<JobBillingRuleMessageVisibility>> ReadAsync(NpgsqlDataReader reader)
    {
        List<JobBillingRuleMessageVisibility> items = new List<JobBillingRuleMessageVisibility>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.JobBillingRuleMessageVisibility(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("message_id"),
                reader.GetString("message_visibility")
                ));
        }

        return items.Freeze();
    }
}
