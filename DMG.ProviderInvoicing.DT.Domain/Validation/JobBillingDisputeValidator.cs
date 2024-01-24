using LanguageExt;
using static LanguageExt.Prelude;
using LanguageExt.Common;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Domain.Validation;

public static class JobBillingDisputeValidator
{
    /// Validate entity used for creation of a dispute response on a job billing
    public static Validation<ErrorMessage, JobBillingDisputeResponsePut> ValidateResponseCreate(JobBillingDisputeResponsePutUnvalidated unvalidated) 
    {
        // validate required fields
        var requestMessageValidation = NonEmptyText.New(unvalidated.DisputeResponseMessage!)
            .MapLeft(_ => ErrorMessage.NewRequiredField(nameof(unvalidated.DisputeResponseMessage)))
            .ToValidation();

        return requestMessageValidation
            .Map(requestMessageValid => new JobBillingDisputeResponsePut(
                new JobBillingId(unvalidated.JobBillingId),
                requestMessageValid,
                new LineItemId(unvalidated.LineItemId),
                unvalidated.LineItemType,
                unvalidated.Meta));
    }
}