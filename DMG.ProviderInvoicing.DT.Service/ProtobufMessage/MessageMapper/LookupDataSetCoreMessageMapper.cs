using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

public static class LookupDataSetCoreMessageMapper
{
    public static DT.Domain.LookupItemCore ToEntity(DMG.DataServices.LookupItemCore lookupItemCoreMessage) =>
        new LookupItemCore(
            NonEmptyText.NewUnsafe(lookupItemCoreMessage.Name.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)),
            NonEmptyText.NewUnsafe(lookupItemCoreMessage.Value.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)));
    
    public static DT.Domain.LookupDataSetCore ToEntity(DMG.DataServices.LookupDataSetCore lookupDataSetCoreMessage) =>
        new LookupDataSetCore(
            new LookupDataSetId(lookupDataSetCoreMessage.DataSetId),
            NonEmptyText.NewOptionUnvalidated(lookupDataSetCoreMessage.DataSetName),
            lookupDataSetCoreMessage.Values.Map(ToEntity).Freeze());
}