using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Rule;

/// Rules and calculations regarding job billing photos
public static class JobBillingPhotoRule
{
    public static int GetChronologyBeforeCount(Lst<JobPhoto> jobBillingPhotos) => // TODO change to return domain type
        jobBillingPhotos.Filter(x => x.Base.PhotoChronology.Equals(PhotoChronology.BeforePhoto)).Count;
    public static int GetChronologyAfterCount(Lst<JobPhoto> jobBillingPhotos) => // TODO change to return domain type
        jobBillingPhotos.Filter(x => x.Base.PhotoChronology.Equals(PhotoChronology.AfterPhoto)).Count;
}