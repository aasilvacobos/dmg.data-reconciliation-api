using DMG.Common;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Domain.Rule;
using LanguageExt;
using static LanguageExt.Prelude;
using RateType = DMG.ProviderInvoicing.DT.Domain.RateType;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper; 

/// Provides deterministic mapping from costing protobuf messages to domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class CostingMessageMapper 
{
    private static DT.Domain.TimeAndMaterialCostingScheme ToEntity(Dmg.Work.Commons.V1.TimeAndMaterialCosting message) =>
        new (message.IsLatest,
            new ProviderNotToExceedAmount(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(message.ProviderNte))),
            new LaborRate(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(message.RegularRate))),
            new LaborRate(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(message.HelperRate))),
            new TripChargeRate(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(message.TripRate))));
    private static DT.Domain.TimeAndMaterialCostingScheme BuildEmptyTimeAndMaterialCostingScheme() =>
        new (false, new ProviderNotToExceedAmount(0.0M), new LaborRate(0.0M), new LaborRate(0.0M), new TripChargeRate(0.0M));

    private static DT.Domain.FlatRateCostingSchemeItem ToEntity(Dmg.Work.Commons.V1.FlatRateItem message) =>
        new (new FlatRateCostingSchemeItemId(ParseGuidStringDefaultToEmptyGuid(message.FlatRateItemId)),
            message.UnitType.Equals(UnitType.Job) // TODO refine logic if Fulfillment provides an improved API
                ? CostingSchemeFlatRateItemType.Job 
                : CostingSchemeFlatRateItemType.CatalogItem,
            CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(message.Rate)),
            Optional(message.CatalogItemId)
                .Bind(TryParseGuidString)
                .Map(x => new CatalogItemId(x)));
    
    private static DT.Domain.FlatRateCostingScheme ToEntity(Dmg.Work.Commons.V1.FlatRateCosting message) =>
        new (new ProviderNotToExceedAmount(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(message.ProviderNte))),
            message.FlatRateItems.Freeze().Map(ToEntity));

    private static ICostingScheme ToEntityJobBillingFlatRateItem(Dmg.Work.Commons.V1.CostingData message) =>
        message.CostingDataCase switch
        {
            Dmg.Work.Commons.V1.CostingData.CostingDataOneofCase.TimeAndMaterialCosting => ToEntity(message.TimeAndMaterialCosting),
            Dmg.Work.Commons.V1.CostingData.CostingDataOneofCase.FlatRateCosting => ToEntity(message.FlatRateCosting),
            _ => BuildEmptyTimeAndMaterialCostingScheme()
        };
    
    public static DT.Domain.RateType ToEntity(DMG.Common.RateType message)
        => message switch
        {
            DMG.Common.RateType.Emergency => RateType.Emergency,
            DMG.Common.RateType.Holiday => RateType.Holiday,
            DMG.Common.RateType.Regular => RateType.Regular,
            DMG.Common.RateType.Unspecified => CostingRule.GetRateTypeDefault(),
            _ => CostingRule.GetRateTypeDefault()
        };
    
    public static DT.Domain.Costing ToEntity(Dmg.Work.Commons.V1.CostingData message) =>
        new (new JobCostingId(ParseGuidStringDefaultToEmptyGuid(message.CostingId)),
            ToEntity(message.RateType),
            ToEntityJobBillingFlatRateItem(message));
}