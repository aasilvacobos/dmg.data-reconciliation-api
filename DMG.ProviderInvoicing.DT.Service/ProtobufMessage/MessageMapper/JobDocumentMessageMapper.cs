using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper; 

/// Provides deterministic mapping between job document protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class JobDocumentMessageMapper 
{
    private record File(
        string?      FileName,
        string      MimeType );
    
    private static File ToEntityFile(DMG.Common.File fileMessage) 
    {
        return Optional(fileMessage)
            .Match(
                f => new File(f.FileName, f.ContentType.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)),
                () => new File(null, DefaultRequiredStringValueIfMissing)
            );
    }

    /// Map job document message to job document entity 
    public static DT.Domain.JobDocument ToEntity(Dmg.Work.Billing.V1.JobDocument jobDocumentMessage, JobWorkId jobWorkId) 
    {
        var recordMeta = RecordMetaMessageMapper.ToEntity(jobDocumentMessage.MetaData);
        var file = ToEntityFile(jobDocumentMessage.Document);
        return new(
            new JobDocumentBase(
                jobWorkId,
                new JobDocumentId(ParseGuidStringDefaultToEmptyGuid(jobDocumentMessage.JobDocumentId)),
                NonEmptyText.NewUnsafe(file.MimeType.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)),
                NonEmptyText.NewOptionUnvalidated(file.FileName),
                NonEmptyText.NewOptionUnvalidated(jobDocumentMessage.Description)),
                recordMeta); 
    }
}