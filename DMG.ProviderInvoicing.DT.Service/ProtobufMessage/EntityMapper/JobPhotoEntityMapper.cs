using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.EntityMapper; 

/// Provides deterministic mapping from job photo entities to Protobuf messages. 
public static class JobPhotoEntityMapper 
{
    public static Dmg.Work.Billing.V1.JobPhotoChronology ToMessage(Domain.PhotoChronology jobPhotoChronology) =>
        jobPhotoChronology
            switch 
            {
                Domain.PhotoChronology.BeforePhoto => Dmg.Work.Billing.V1.JobPhotoChronology.BeforePhoto,
                Domain.PhotoChronology.AfterPhoto => Dmg.Work.Billing.V1.JobPhotoChronology.AfterPhoto,
                Domain.PhotoChronology.DuringPhoto => Dmg.Work.Billing.V1.JobPhotoChronology.DuringPhoto,
                Domain.PhotoChronology.OtherPhoto => Dmg.Work.Billing.V1.JobPhotoChronology.Other,
                _ => Dmg.Work.Billing.V1.JobPhotoChronology.Other
            };
}