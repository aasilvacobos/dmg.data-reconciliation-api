using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record JobBillingRuleMessage(
                    Guid ProviderBillingId,
                    Guid EntityId,
                    string EntityType,
                    Guid MessageId,
                    string MessageType,
                    string MessageRule,
                    string? MessageText)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, entity_id, entity_type, message_id, message_type, message_rule, message_text
	FROM provider_billing.jobbilling_rule_message 
    where provider_billing_id = @id;";

    internal static async Task<Lst<JobBillingRuleMessage>> ReadAsync(NpgsqlDataReader reader)
    {
        List<JobBillingRuleMessage> items = new List<JobBillingRuleMessage>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.JobBillingRuleMessage(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("entity_id"),
                reader.GetString("entity_type"),
                reader.GetGuid("message_id"),
                reader.GetString("message_type"),
                reader.GetString("message_rule"),
                reader.SafeGetString("message_text")
                ));
        }

        return items.Freeze();
    }
}
