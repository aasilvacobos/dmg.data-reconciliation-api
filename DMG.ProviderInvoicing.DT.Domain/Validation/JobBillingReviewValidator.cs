using System;
using System.Linq;
using LanguageExt;
using static LanguageExt.Prelude;
using LanguageExt.Common;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Domain.Validation; 

/// Validations related to the job billing review
public static class JobBillingReviewValidator 
{
    public static Validation<ErrorMessage, JobBillingReviewRequestCreate> ValidateRequest(JobBillingReviewRequestCreateUnvalidated unvalidated) 
    {
        // validate required fields
        var requestMessageValidation = NonEmptyText.New(unvalidated.Message)
            .MapLeft(_ => ErrorMessage.NewRequiredField(nameof(unvalidated.Message)))
            .ToValidation();

        return requestMessageValidation
            .Map(requestMessageValid => new JobBillingReviewRequestCreate(
                requestMessageValid,
                unvalidated.Meta));
    }
}