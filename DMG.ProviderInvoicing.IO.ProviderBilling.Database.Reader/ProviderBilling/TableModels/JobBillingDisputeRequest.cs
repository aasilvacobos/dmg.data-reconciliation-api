using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record JobBillingDisputeRequest(
                    Guid ProviderBillingId,
                    Guid DisputeRequestId,
                    Guid DisputeConversationId,
                    string DisputeRequestReason,
                    string? ReasonText,
                    string? AdditionalText)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, dispute_request_id, dispute_conversation_id, dispute_request_reason, reason_text, additional_text
	FROM provider_billing.jobbilling_dispute_request 
    where provider_billing_id = @id;";

    internal static async Task<Lst<JobBillingDisputeRequest>> ReadAsync(NpgsqlDataReader reader)
    {
        List<JobBillingDisputeRequest> items = new List<JobBillingDisputeRequest>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.JobBillingDisputeRequest(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("dispute_request_id"),
                reader.GetGuid("dispute_conversation_id"),
                reader.GetString("dispute_request_reason"),
                reader.SafeGetString("reason_text"),
                reader.SafeGetString("additional_text")
                ));
        }

        return items.Freeze();
    }
}
