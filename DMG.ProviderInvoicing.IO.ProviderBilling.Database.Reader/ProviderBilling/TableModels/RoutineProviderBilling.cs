using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record RoutineProviderBilling(
                    Guid ProviderBillingId,
                    Guid TicketId,
                    string TicketNumber,
                    Guid ProviderOrgId,
                    Guid CustomerId,
                    Guid PropertyId,
                    Guid ServiceLineId,
                    string CostingScheme,
                    string Status,
                    decimal TotalCost,
                    string? ProviderInvoiceNumber,
                    int Version,
                    Guid PsaId,
                    string? Description,
                    string? Notes,
                    string ProviderBillingNumber,
                    DateTime? InvoiceCreatedAt)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, ticket_id, ticket_number, provider_org_id, customer_id, property_id, service_line_id, costing_scheme, status, total_cost, provider_invoice_number, version, psa_id, description, notes, provider_billing_number, invoice_created_at
	FROM provider_billing.routine_provider_billing 
    where provider_billing_id = @id;";

    internal static async Task<Option<RoutineProviderBilling>> ReadAsync(NpgsqlDataReader reader)
    {
        while (await reader.ReadAsync())
        {
            return new TableModels.RoutineProviderBilling(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("ticket_id"),
                reader.GetString("ticket_number"),
                reader.GetGuid("provider_org_id"),
                reader.GetGuid("customer_id"),
                reader.GetGuid("property_id"),
                reader.GetGuid("service_line_id"),
                reader.GetString("costing_scheme"),
                reader.GetString("status"),
                reader.GetDecimal("total_cost"),
                reader.SafeGetString("provider_invoice_number"),
                reader.GetInt32("version"),
                reader.GetGuid("psa_id"),
                reader.SafeGetString("description"),
                reader.SafeGetString("notes"),
                reader.GetString("provider_billing_number"),
                reader.SafeGetDateTime("invoice_created_at")
                );
        }

        return Option<RoutineProviderBilling>.None;
    }
}
