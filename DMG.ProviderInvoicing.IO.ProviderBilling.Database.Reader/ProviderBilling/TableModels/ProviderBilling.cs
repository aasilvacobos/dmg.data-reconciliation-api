using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMG.ProviderInvoicing.DT.Domain;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;
public record ProviderBilling
(
    Guid ProviderBillingId,
    Guid TicketId,
    string TicketNumber,
    Guid ProviderOrgId,
    Guid CustomerId,
    Guid PropertyId,
    Guid ServiceLineId,
    Guid? ServiceTypeId,
    string CostingScheme,
    string Status,
    decimal TotalCost,
    string? ProviderInvoiceNumber,
    int Version,
    string? JobSummary,
    Guid? PsaId,
    string? Description,
    string? Notes,
    string? ProviderBillingNumber,
    BillingType BillingType,
    DateTime? InvoiceCreatedAt
);
