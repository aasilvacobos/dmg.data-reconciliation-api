using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record JobBillingDisputeConversation(
                    Guid ProviderBillingId,
                    Guid DisputeConversationId,
                    Guid DisputeId,
                    long DisputeConversationOrder)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, dispute_conversation_id, dispute_id, dispute_conversation_order
	FROM provider_billing.jobbilling_dispute_conversation 
    where provider_billing_id = @id;";

    internal static async Task<Lst<JobBillingDisputeConversation>> ReadAsync(NpgsqlDataReader reader)
    {
        List<JobBillingDisputeConversation> items = new List<JobBillingDisputeConversation>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.JobBillingDisputeConversation(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("dispute_conversation_id"),
                reader.GetGuid("dispute_id"),
                reader.GetInt64("dispute_conversation_order")
                ));
        }

        return items.Freeze();
    }
}
