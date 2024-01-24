using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record Visit(
    Guid ProviderBillingId,
    DateTime CheckIn,
    DateTime CheckOut,
    string SourceTicketStage,
    Guid VisitId,
    bool MissedCheckIn,
    bool Addendum,
    Guid? JobWorkId)
{
    internal static string Sql { get; } =
        @"SELECT provider_billing_id, check_in, check_out, source_ticket_stage, visit_id, missed_check_in, addendum, job_work_id
	FROM provider_billing.visit 
    where provider_billing_id = @id
    order by check_in asc;";

    internal static async Task<Lst<Visit>> ReadAsync(NpgsqlDataReader reader)
    {
        List<Visit> items = new List<Visit>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.Visit(
                reader.GetGuid("provider_billing_id"),
                reader.GetDateTime("check_in"),
                reader.GetDateTime("check_out"),
                reader.GetString("source_ticket_stage"),
                reader.GetGuid("visit_id"),
                reader.GetBoolean("missed_check_in"),
                reader.GetBoolean("addendum"),
                reader.SafeGetGuid("job_work_id")));
        }

        return items.Freeze();
    }
}