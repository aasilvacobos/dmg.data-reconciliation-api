using System;
using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain;

/// Transactional Data **AGGREGATE**: The customer invoice entity.
public record CustomerInvoice(
    CustomerInvoiceExternalId                   ExternalId,
    TicketId                                    DmgTicketId,
    NonEmptyText                                DmgCustomerInvoiceStatus,
    int                                         ItemLineLineNumber,
    int                                         MaxItemCount);