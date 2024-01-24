using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record JobBillingDisputeResponse(
                    Guid ProviderBillingId,
                    Guid DisputeResponseId,
                    Guid DisputeConversationId,
                    string Message)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, dispute_response_id, dispute_conversation_id, message
	FROM provider_billing.jobbilling_dispute_response 
    where provider_billing_id = @id;";

    internal static async Task<Lst<JobBillingDisputeResponse>> ReadAsync(NpgsqlDataReader reader)
    {
        List<JobBillingDisputeResponse> items = new List<JobBillingDisputeResponse>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.JobBillingDisputeResponse(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("dispute_response_id"),
                reader.GetGuid("dispute_conversation_id"),
                reader.GetString("message")
                ));
        }

        return items.Freeze();
    }
}
