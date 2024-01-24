using System.Data;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using LanguageExt;
using Npgsql;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels; 

public record NonRoutineProviderBilling(
    Guid ProviderBillingId,
    Guid TicketId,
    string TicketNumber,
    Guid ProviderOrgId,
    Guid CustomerId,
    Guid PropertyId,
    Guid ServiceLineId,
    Guid ServiceTypeId,
    string CostingScheme,
    string Status,
    decimal TotalCost,
    string? ProviderInvoiceNumber,
    int Version,
    string? JobSummary)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, ticket_id, ticket_number, provider_org_id, customer_id, property_id, service_line_id, service_type_id, costing_scheme, status, total_cost, provider_invoice_number, version, job_summary
	FROM provider_billing.non_routine_provider_billing 
    where provider_billing_id = @id;";

    internal static async Task<Option<NonRoutineProviderBilling>> ReadAsync(NpgsqlDataReader reader)
    {
        while (await reader.ReadAsync())
        {
            return new NonRoutineProviderBilling(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("ticket_id"),
                reader.GetString("ticket_number"),
                reader.GetGuid("provider_org_id"),
                reader.GetGuid("customer_id"),
                reader.GetGuid("property_id"),
                reader.GetGuid("service_line_id"),
                reader.GetGuid("service_type_id"),
                reader.GetString("costing_scheme"),
                reader.GetString("status"),
                reader.GetDecimal("total_cost"),
                reader.SafeGetString("provider_invoice_number"),
                reader.GetInt16("version"), //TODO this might be changed to int32 to as it matches int32 on the jobBilling
                reader.SafeGetString("job_summary")
            );
        }

        return Option<NonRoutineProviderBilling>.None;
    }
}
