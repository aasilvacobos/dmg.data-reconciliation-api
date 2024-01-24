using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper; 

/// Provides deterministic mapping between job photo protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class JobPhotoMessageMapper 
{
    public static DT.Domain.PhotoChronology ToEntityPhotoChronology(Dmg.Work.Billing.V1.JobPhotoChronology jobPhotoChronologyMessage) =>
        jobPhotoChronologyMessage
            switch 
            {
                Dmg.Work.Billing.V1.JobPhotoChronology.BeforePhoto => DT.Domain.PhotoChronology.BeforePhoto,
                Dmg.Work.Billing.V1.JobPhotoChronology.AfterPhoto => DT.Domain.PhotoChronology.AfterPhoto,
                Dmg.Work.Billing.V1.JobPhotoChronology.DuringPhoto => DT.Domain.PhotoChronology.DuringPhoto,
                Dmg.Work.Billing.V1.JobPhotoChronology.Other => DT.Domain.PhotoChronology.OtherPhoto,
                _ => DT.Domain.PhotoChronology.OtherPhoto
            };

    private static DT.Domain.JobPhoto BuildJobPhotoEmpty() =>
        new(new(new JobWorkId(Guid.Empty),
                new(Guid.Empty),
                NonEmptyText.NewUnsafe(DefaultRequiredStringValueIfMissing),  
                DT.Domain.PhotoChronology.OtherPhoto,
                NonEmptyText.NewUnsafe(DefaultRequiredStringValueIfMissing),
                NonEmptyText.NewUnsafe(DefaultRequiredStringValueIfMissing),
                Option<ServiceLineId>.None,
                Option<VisitId>.None),
                new RecordMeta(Option<UserId>.None, Option<DateTimeOffset>.None, Option<UserId>.None, Option<DateTimeOffset>.None));
    
    /// Map job photo message to job photo entity 
    public static DT.Domain.JobPhoto ToEntity(Dmg.Work.Billing.V1.JobPhoto jobPhotoMessage, JobWorkId jobWorkId) 
    {
        var recordMeta = Optional(jobPhotoMessage)
            .Match(x => RecordMetaMessageMapper.ToEntity(x.MetaData), RecordMetaMessageMapper.ToEntity(default!));
        
        return Optional(jobPhotoMessage)
            .Map(jpm => new DT.Domain.JobPhoto(new(jobWorkId,
                        new(ParseGuidStringDefaultToEmptyGuid(jpm.JobPhotoId)),
                        NonEmptyText.NewUnsafe(jpm.MimeType.DefaultIfNullOrWhiteSpace(MessageMapperUtility.DefaultRequiredStringValueIfMissing)),
                        ToEntityPhotoChronology(jpm.JobPhotoChronology),
                        NonEmptyText.NewOptionUnvalidated(jpm.Filename),
                        NonEmptyText.NewOptionUnvalidated(jpm.Description),
                        Option<ServiceLineId>.None,
                        Option<VisitId>.None),
                        recordMeta))
            .Match(x => x, BuildJobPhotoEmpty());
    }
}