using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMG.ProviderInvoicing.DT.Domain;

public record ProviderBillingCrudMutateMeta(
    UserId SessionUserId,
    TaskId TaskId);

public record ProviderBillingCrudMutateProviderBillingMeta(
    ProviderBillingId ProviderBillingId,
    ProviderBillingVersion Version);

public record ProviderBillingCrud(
    Option<NonEmptyText> ProviderInvoiceNumber,
    Option<NonEmptyText> Note);