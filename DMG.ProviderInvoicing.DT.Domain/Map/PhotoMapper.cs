using DMG.ProviderInvoicing.DT.Domain.Rule;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Map;

public static class PhotoMapper
{
    public static PhotoFolder ToPhotoFolder(Lst<JobPhoto> jobPhotos) =>
        new PhotoFolder(
            new PhotoBeforeCount(JobBillingPhotoRule.GetChronologyBeforeCount(jobPhotos)),
            new PhotoAfterCount(JobBillingPhotoRule.GetChronologyAfterCount(jobPhotos)));
}