using System;
using System.Linq;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Domain.Validation;

/// Validations related to the job billing
public static class JobDocumentValidator
{
    public static Validation<ErrorMessage, JobDocumentPut> ValidatePut(JobDocumentPutUnvalidated unvalidated)
    {
        // NonEmptyText prevents an empty string which can get through GraphQL
        var mimeTypeValidation = NonEmptyText.New(unvalidated.Base.MimeType)
            .MapLeft(_ => ErrorMessage.NewRequiredField(nameof(unvalidated.Base.MimeType)))
            .ToValidation();
        var fileNameValidation = NonEmptyText.NewOption(unvalidated.Base.FileName)
            .MapLeft(_ => ErrorMessage.NewRequiredField(nameof(unvalidated.Base.FileName)))
            .ToValidation();
        var descriptionValidation = NonEmptyText.NewOption(unvalidated.Base.Description)
            .MapLeft(_ => ErrorMessage.NewRequiredField(nameof(unvalidated.Base.Description)))
            .ToValidation();

        return (mimeTypeValidation, fileNameValidation, descriptionValidation).Apply((mimeTypeValid, fileNameValid, descriptionValid) =>
             new JobDocumentPut(unvalidated.Meta,
                 new JobDocumentBase(
                     new(unvalidated.Base.JobWorkId),
                     new(unvalidated.Base.JobDocumentId),
                     mimeTypeValid,
                     fileNameValid,
                     descriptionValid)));
    }
}