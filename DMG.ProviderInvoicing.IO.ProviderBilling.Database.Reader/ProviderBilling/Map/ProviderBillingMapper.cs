using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.DT.Domain.Rule;
using DMG.ProviderInvoicing.IO.SorConcentrator;
using System.Collections.Generic;
using System;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.Map;

public class ProviderBillingMapper
{
	private const string MissingValue = "MISSING_VALUE";
	private static DateTime MissingDate = new(2000, 01, 01);

	public enum EntityType
	{
		EQUIPMENT,
		EQUIPMENT_HEADER,
		FLAT_RATE_EQUIPMENT,
		FLAT_RATE_EQUIPMENT_HEADER,
		MATERIAL,
		MATERIAL_HEADER,
		FLAT_RATE_MATERIAL,
		FLAT_RATE_MATERIAL_HEADER,
		FLAT_RATE,
		FLAT_RATE_HEADER,
		LABOR,
		LABOR_HEADER,
		DISCOUNT,
		JOB_BILLING_DISPUTE_REQUEST,
		JOB_BILLING_DISPUTE_RESPONSE,
		JOB_BILLING_DISPUTE_CONVERSATION,
		JOB_BILLING_DISPUTE,
		TIME_ADJUSTMENT,
		PROVIDER_BILLING,
		SERVICE_LINE_ITEM,
		VISIT,
		PHOTO,
		TIME_AND_MATERIAL_LINE_ITEM

	}

	public static EntityType EntityTypeToEntity(string entityType) =>
		entityType switch
		{
			"EQUIPMENT" => EntityType.EQUIPMENT,
			"EQUIPMENT_HEADER" => EntityType.EQUIPMENT_HEADER,
			"FLAT_RATE_EQUIPMENT" => EntityType.FLAT_RATE_EQUIPMENT,
			"FLAT_RATE_EQUIPMENT_HEADER" => EntityType.FLAT_RATE_EQUIPMENT_HEADER,
			"MATERIAL" => EntityType.MATERIAL,
			"MATERIAL_HEADER" => EntityType.MATERIAL_HEADER,
			"FLAT_RATE_MATERIAL" => EntityType.FLAT_RATE_MATERIAL,
			"FLAT_RATE_MATERIAL_HEADER" => EntityType.FLAT_RATE_MATERIAL_HEADER,
			"FLAT_RATE" => EntityType.FLAT_RATE,
			"FLAT_RATE_HEADER" => EntityType.FLAT_RATE_HEADER,
			"LABOR" => EntityType.LABOR,
			"LABOR_HEADER" => EntityType.LABOR_HEADER,
			"DISCOUNT" => EntityType.DISCOUNT,
			"JOB_BILLING_DISPUTE_REQUEST" => EntityType.JOB_BILLING_DISPUTE_REQUEST,
			"JOB_BILLING_DISPUTE_RESPONSE" => EntityType.JOB_BILLING_DISPUTE_RESPONSE,
			"JOB_BILLING_DISPUTE_CONVERSATION" => EntityType.JOB_BILLING_DISPUTE_CONVERSATION,
			"JOB_BILLING_DISPUTE" => EntityType.JOB_BILLING_DISPUTE,
			"TIME_ADJUSTMENT" => EntityType.TIME_ADJUSTMENT,
			"PROVIDER_BILLING" => EntityType.PROVIDER_BILLING,
			"SERVICE_LINE_ITEM" => EntityType.SERVICE_LINE_ITEM,
			"VISIT" => EntityType.VISIT,
			"PHOTO" => EntityType.PHOTO,
			_ => EntityType.SERVICE_LINE_ITEM
		};

	public static JobBillingElementCreationSource CreationSourceToEntity(string creationSource) =>
	  creationSource switch
	  {
		  "SYSTEM" => DT.Domain.JobBillingElementCreationSource.System,
		  "PROVIDER" => DT.Domain.JobBillingElementCreationSource.Provider,
		  "TECHNICIAN" => DT.Domain.JobBillingElementCreationSource.Technician,
		  "OPERATIONS" => DT.Domain.JobBillingElementCreationSource.Operations,
		  "JOB_COSTING" => DT.Domain.JobBillingElementCreationSource.JobCosting,
		  _ => DT.Domain.JobBillingElementCreationSource.Unspecified
	  };

	public static ProviderBillingSource ProviderBillingSourceToEntity(string source) =>
	  source switch
	  {
		  "DMG" => ProviderBillingSource.DMG,
		  "PROVIDER" => ProviderBillingSource.Provider,
		  "PSA" => ProviderBillingSource.PSA,
		  "VISIT_LOGS" => ProviderBillingSource.VisitLogs,
		  "SYSTEM" => ProviderBillingSource.System,
		  _ => ProviderBillingSource.PSA
	  };

	public static EquipmentCatalogItemType EquipmentCatalogItemTypeEntity(string equipmentKind) =>
		equipmentKind switch
		{
			"OWNED_EQUIPMENT" => EquipmentCatalogItemType.EquipmentOwned,
			"RENTAL_EQUIPMENT" => EquipmentCatalogItemType.EquipmentRental,
			_ => EquipmentCatalogItemType.EquipmentRental
		};

	public static MaterialPartCatalogItemType MaterialPartCatalogItemTypeEntity(string materialPartKind) =>
		materialPartKind switch
		{
			"Material" => MaterialPartCatalogItemType.Material,
			"Part" => MaterialPartCatalogItemType.Part,
			_ => MaterialPartCatalogItemType.Part
		};

	public static RateType RateTypeToEntity(string rateType) =>
		rateType switch
		{
			"RATE_TYPE_REGULAR" => DT.Domain.RateType.Regular,
			"RATE_TYPE_EMERGENCY" => DT.Domain.RateType.Emergency,
			"RATE_TYPE_HOLIDAY" => DT.Domain.RateType.Holiday,
			_ => CostingRule.GetRateTypeDefault()
		};

	public static TechnicianType TechnicianTypeToEntity(string technicianType) =>
		technicianType switch
		{
			"REGULAR" => DT.Domain.TechnicianType.TechnicianTypeRegular,
			"HELPER" => DT.Domain.TechnicianType.TechnicianTypeHelper,
			_ => DT.Domain.TechnicianType.TechnicianTypeHelper
		};

	public static ProviderBillingCostingScheme CostingSchemeToEntity(string costingScheme) =>
		 costingScheme switch
		 {
			 "FLAT_RATE" => DT.Domain.ProviderBillingCostingScheme.FlatRate,
			 "TIME_AND_MATERIAL" => DT.Domain.ProviderBillingCostingScheme.TimeAndMaterial,
			 "SERVICE_BASED" => DT.Domain.ProviderBillingCostingScheme.ServiceBased,
			 "PER_OCCURRENCE" => DT.Domain.ProviderBillingCostingScheme.PerOccurrence,
			 "SEASONAL" => DT.Domain.ProviderBillingCostingScheme.Seasonal,
			 "NON_ROUTINE" => DT.Domain.ProviderBillingCostingScheme.NonRoutine,
			 _ => DT.Domain.ProviderBillingCostingScheme.TimeAndMaterial
		 };

	public static ProviderBillingStatus ProviderBillingStatusToEntity(string status) =>
		status switch
		{
			"WAITING_ON_DATA" => DT.Domain.ProviderBillingStatus.WaitingOnData,
			"WAITING_DMG" => DT.Domain.ProviderBillingStatus.WaitingDmg,
			"WAITING_PROVIDER" => DT.Domain.ProviderBillingStatus.WaitingProvider,
			"APPROVED" => DT.Domain.ProviderBillingStatus.Approved,
			"CANCELLED" => DT.Domain.ProviderBillingStatus.Cancelled,
			"NO_PAY" => DT.Domain.ProviderBillingStatus.NoPay,
			"TODO" => DT.Domain.ProviderBillingStatus.Todo,
			"IN_PROGRESS" => DT.Domain.ProviderBillingStatus.InProgress,
			"VERIFIED" => DT.Domain.ProviderBillingStatus.Verified,
			_ => DT.Domain.ProviderBillingStatus.Unspecified
		};

	public static ProviderBillingAssignee AssignedToEntity(string assignedTo) =>
		assignedTo switch
		{
			"PROVIDER" => ProviderBillingAssignee.Provider,
			"OPERATIONS" => ProviderBillingAssignee.Operations,
			"DISTRICT_MANAGER" => ProviderBillingAssignee.DistrictManager,
			_ => ProviderBillingAssignee.DistrictManager
		};

	public static CreditCardProvider CreditCardProviderToEntity(string creditCardProvider) =>
		 creditCardProvider switch
		 {
			 "CORPORATE_CARD" => CreditCardProvider.CorporateCard,
			 "STRIPE" => CreditCardProvider.Stripe,
			 "PNC_BANK" => CreditCardProvider.PncBank,
			 _ => CreditCardProvider.Unspecified
		 };

	public static JobUrgency UrgencyToEntity(string urgency) =>
		urgency switch
		{
			"WORK_URGENCY_EMERGENCY" => JobUrgency.Emergency,
			"WORK_URGENCY_HIGH" => JobUrgency.High,
			"WORK_URGENCY_NORMAL" => JobUrgency.Normal,
			_ => JobUrgency.Unknown
		};

	public static ProviderBillingVisitTicketStageSource VisitSourceToEntity(string visitSource) =>
		visitSource switch
		{
			"WORK_FULFILLMENT" => ProviderBillingVisitTicketStageSource.Fulfillment,
			"PROVIDER_BILLING" => ProviderBillingVisitTicketStageSource.ProviderBilling,
			_ => ProviderBillingVisitTicketStageSource.ProviderBilling
		};

	public static ProviderBillingServiceLineItemType ServiceLineItemToEntity(string type) =>
		type switch
		{
			"PER_SERVICE" => ProviderBillingServiceLineItemType.PerService,
			"PER_EVENT" => ProviderBillingServiceLineItemType.PerEvent,
			_ => ProviderBillingServiceLineItemType.PerService
		};

	public static ProviderBillingBillingLineItemType BillingLineItemTypeToEntity(string type) =>
	  type switch
	  {
		  "PER_EVENT_CALCULATED" => ProviderBillingBillingLineItemType.PerEventCalculated,
		  "SEASONAL" => ProviderBillingBillingLineItemType.Season,
		  "DISCOUNT" => ProviderBillingBillingLineItemType.Discount,
		  "PROCESSING_FEE" => ProviderBillingBillingLineItemType.ProcessingFee,
		  _ => ProviderBillingBillingLineItemType.ProcessingFee
	  };

	public static JobBillingMessageType JobBillingMessageTypeToEntity(string type) =>
		type switch
		{
			"WARNING" => JobBillingMessageType.Warning,
			"NOTE" => JobBillingMessageType.Note,
			"ALERT" => JobBillingMessageType.Alert,
			_ => JobBillingMessageType.Note
		};

	public static JobBillingMessageRule JobBillingMessageRuleToEntity(string type) =>
		type switch
		{
			"UNSPECIFIED" => JobBillingMessageRule.Unspecified,
			"REMOVE_MIN_CHECK_IN_CHECK_OUT_LABOR" => JobBillingMessageRule.RemoveMinCheckInCheckOutLabor,
			"ROUND_UP_TO_NEXT_MIN_LABOR" => JobBillingMessageRule.RoundUpToNextMinLabor,
			"QTY_OF_LABOR_FOR_THIS_SERVICE_TYPE" => JobBillingMessageRule.QtyOfLaborForThisServiceType,
			"LABOR_TOTAL_CALCULATION" => JobBillingMessageRule.LaborTotalCalculation,
			"REMOVE_EXTRA_TRIPS_FOR_SAME_DAY_SAME_PROPERTY" => JobBillingMessageRule.RemoveExtraTripsForSameDaySameProperty,
			"REMOVE_EXTRA_TRIPS" => JobBillingMessageRule.RemoveExtraTrips,
			"ADD_MISSED_ARRIVAL_LINE_ITEM_BASED_ON_URGENCY_TRIP" => JobBillingMessageRule.AddMissedArrivalLineItemBasedOnUrgencyTrip,
			"TRIP_TOTAL_CALCULATION" => JobBillingMessageRule.TripTotalCalculation,
			"REMOVE_UN_PAYABLE_PARTS_AND_MATERIAL" => JobBillingMessageRule.RemoveUnPayablePartsAndMaterial,
			"CHECK_VARIANCE_OF_QTY_USAGE_FOR_SERVICE_TYPE_PARTS_AND_MATERIAL" => JobBillingMessageRule.CheckVarianceOfQtyUsageForServiceTypePartsAndMaterial,
			"CHECK_FOR_NON_CATALOGUE_PARTS_AND_MATERIAL" => JobBillingMessageRule.CheckForNonCataloguePartsAndMaterial,
			"CHECK_VARIANCE_FOR_RATE_COMPARED_TO_OUR_STANDARDS_PER_SERVICE_TYPE_FOR_PARTS_AND_MATERIAL" => JobBillingMessageRule.CheckVarianceForRateComparedToOurStandardsPerServiceTypeForPartsAndMaterial,
			"PARTS_AND_MATERIAL_TOTAL_CALCULATION" => JobBillingMessageRule.PartsAndMaterialTotalCalculation,
			"REMOVE_UN_PAYABLE_EQUIPMENTS" => JobBillingMessageRule.RemoveUnPayableEquipments,
			"CHECK_VARIANCE_OF_QTY_USAGE_FOR_SERVICE_TYPE_EQUIPMENT" => JobBillingMessageRule.CheckVarianceOfQtyUsageForServiceTypeEquipment,
			"CHECK_FOR_NON_CATALOGUE_EQUIPMENT" => JobBillingMessageRule.CheckForNonCatalogueEquipment,
			"CHECK_VARIANCE_FOR_RATE_COMPARED_TO_OUR_STANDARDS_PER_SERVICE_TYPE_EQUIPMENT" => JobBillingMessageRule.CheckVarianceForRateComparedToOurStandardsPerServiceTypeEquipment,
			"EQUIPMENT_TOTAL_CALCULATION" => JobBillingMessageRule.EquipmentTotalCalculation,
			"TOTAL_AMOUNT_CALCULATION" => JobBillingMessageRule.TotalAmountCalculation,
			"CHECK_TOTAL_AMOUNT_AGAINST_NTE" => JobBillingMessageRule.CheckTotalAmountAgainstNte,
			"CHECK_AVERAGE_TOTAL_AMOUNT_AGAINST_PREVIOUS_WORK_OF_SAME_SERVICE_LINE" => JobBillingMessageRule.CheckAverageTotalAmountAgainstPreviousWorkOfSameServiceLine,
			"CHECK_PARTS_AND_MATERIAL_CATALOG_ITEM_RATE" => JobBillingMessageRule.CheckPartsAndMaterialCatalogItemRate,
			"CHECK_EQUIPMENT_CATALOG_ITEM_RATE" => JobBillingMessageRule.CheckEquipmentCatalogItemRate,
			"FLAT_RATE_JOB_TOTAL_CALCULATION" => JobBillingMessageRule.FlatRateJobTotalCalculation,
			"CHECK_FLAT_RATE_JOB_TOTAL_AMOUNT_IS_NEGATIVE" => JobBillingMessageRule.CheckFlatRateJobTotalAmountIsNegative,
			"FLAT_RATE_PARTS_AND_MATERIAL_TOTAL_CALCULATION" => JobBillingMessageRule.FlatRatePartsAndMaterialTotalCalculation,
			"CHECK_FLAT_RATE_PARTS_AND_MATERIAL_TOTAL_AMOUNT_IS_NEGATIVE" => JobBillingMessageRule.CheckFlatRatePartsAndMaterialTotalAmountIsNegative,
			"FLAT_RATE_EQUIPMENT_TOTAL_CALCULATION" => JobBillingMessageRule.FlatRateEquipmentTotalCalculation,
			"CHECK_FLAT_RATE_EQUIPMENT_TOTAL_AMOUNT_IS_NEGATIVE" => JobBillingMessageRule.CheckFlatRateEquipmentTotalAmountIsNegative,
			"MAX_USAGE_HOURS_CALCULATION" => JobBillingMessageRule.MaxUsageHoursCalculation,
			"CHECK_IS_CREDIT_CARD_PROVIDER" => JobBillingMessageRule.CheckIsCreditCardProvider,
			"FLAG_NON_CATALOG_ITEMS_NAME_PARTS_AND_MATERIAL" => JobBillingMessageRule.FlagNonCatalogItemsNamePartsAndMaterial,
			"FLAG_NON_CATALOG_ITEMS_NAME_EQUIPMENT" => JobBillingMessageRule.FlagNonCatalogItemsNameEquipment,
			"AUTO_DEDUCT_LABOR" => JobBillingMessageRule.AutoDeductLabor,
			_ => JobBillingMessageRule.Unspecified
		};

	public static JobBillingMessageVisibility JobBillingMessageVisibilityToEntity(string type) =>
		type switch
		{
			"DMG_INTERNAL_ONLY" => JobBillingMessageVisibility.Operations,
			"PROVIDER" => JobBillingMessageVisibility.Provider,
			_ => JobBillingMessageVisibility.Operations,
		};

	public static JobBillingSubmittedBySource JobBillingSubmittedBySourceToEntity(string type) =>
		type switch
		{
			"INTERNAL_API" => JobBillingSubmittedBySource.InternalApi,
			"AUTO_SUBMIT_SYSTEM_ACTION" => JobBillingSubmittedBySource.AutoSubmitSystemAction,
			"OPERATIONS" => JobBillingSubmittedBySource.Operations,
			"PROVIDER_USER_ACTION" => JobBillingSubmittedBySource.ProviderUserAction,
			"TAKE_CONTROL_TASK_ACTION" => JobBillingSubmittedBySource.TakeControlTaskAction,
			"COMPLAINT_JOB_INVOICING_SYSTEM_ACTION" => JobBillingSubmittedBySource.ComplaintJobInvoicingSystemAction,
			"UNSPECIFIED" => JobBillingSubmittedBySource.Unspecified,
			_ => JobBillingSubmittedBySource.Unspecified
		};

	public static JobBillingLineItemDisputeRequestReason ToJobBillingLineItemDisputeRequestReasonEntity(string type) =>
		type switch
		{
			"WHY_WERE_SO_MANY_HOURS_USED_TO_DO_THE_JOB" => JobBillingLineItemDisputeRequestReason.WhyWereSoManyHoursUsedToDoTheJob,
			"PLEASE_SHARE_THE_REASON_WHY_THIS_MUCH_QUANTITY_WAS_USED" => JobBillingLineItemDisputeRequestReason.PleaseShareTheReasonWhyThisMuchQuantityWasUsed,
			"THE_RATE_IS_HIGH_CAN_YOU_SHARE_THE_PROOF_OF_PURCHASE" => JobBillingLineItemDisputeRequestReason.TheRateIsHighCanYouShareTheProofOfPurchase,
			"THE_RATE_IS_HIGH_CAN_YOU_EXPLAIN_WHY_AND_SHARE_PROOF" => JobBillingLineItemDisputeRequestReason.TheRateIsHighCanYouExplainWhyAndShareProof,
			"OTHER" => JobBillingLineItemDisputeRequestReason.Other,
			"UNSPECIFIED" => JobBillingLineItemDisputeRequestReason.Unspecified,
			_ => JobBillingLineItemDisputeRequestReason.Unspecified
		};

	public static PhotoChronology ToProviderBillingPhotoChronology(string photoChronology) =>
		photoChronology switch
		{
			"OTHER" => PhotoChronology.OtherPhoto,
			"BEFORE_PHOTO" => PhotoChronology.BeforePhoto,
			"DURING_PHOTO" => PhotoChronology.DuringPhoto,
			"AFTER_PHOTO" => PhotoChronology.AfterPhoto,
			_ => PhotoChronology.OtherPhoto
		};

	public static EventGroupSource SourceToEntity(string source) =>
		source switch
		{
			"EVENT" => EventGroupSource.Event,
			"MANUAL_GROUPING" => EventGroupSource.Manual_Grouping,
			_ => EventGroupSource.Event
		};

	public static ModifyBy ModifyByToEntity(string type) =>
		type switch
		{
			"DM" => ModifyBy.DistrictManager,
			"PROVIDER" => ModifyBy.Provider,
			_ => ModifyBy.DistrictManager
		};

	public static ModifyAction ModifyActionToEntity(string type) =>
		type switch
		{
			"ADD" => ModifyAction.Add,
			"EDIT" => ModifyAction.Edit,
			_ => ModifyAction.Add
		};

	public static UnitOfMeasure UnitOfMeasureToEntity(string unitType) =>
		unitType switch
		{
			"UNSPECIFIED" => UnitOfMeasure.Unspecified,
			"ITEM" => UnitOfMeasure.Item,
			"CASE" => UnitOfMeasure.Case,
			"GALLON" => UnitOfMeasure.Gallon,
			"LITER" => UnitOfMeasure.Liter,
			"POUND" => UnitOfMeasure.Pound,
			"KILO" => UnitOfMeasure.Kilo,
			"TON" => UnitOfMeasure.Ton,
			"MINUTE" => UnitOfMeasure.Minute,
			"HOUR" => UnitOfMeasure.Hour,
			"TRIP" => UnitOfMeasure.Trip,
			"DAY" => UnitOfMeasure.Day,
			"WEEK" => UnitOfMeasure.Week,
			"YD3" => UnitOfMeasure.Yd3,
			"YD2" => UnitOfMeasure.Yd2,
			"MILE" => UnitOfMeasure.Mile,
			"METER" => UnitOfMeasure.Meter,
			"JOB" => UnitOfMeasure.Job,
			"FOOT" => UnitOfMeasure.Foot,
			"INCH" => UnitOfMeasure.Inch,
			"EVENT" => UnitOfMeasure.Event,
			"TRUCK" => UnitOfMeasure.Truck,
			"BAG" => UnitOfMeasure.Bag,
			"SERVICE" => UnitOfMeasure.Service,
			"SECOND" => UnitOfMeasure.Second,
			_ => UnitOfMeasure.Unspecified
		};

	public static ServiceItemSource ServiceItemSourceToEntity(string type) =>
		type switch
		{
			"LOOKUP_ITEM" => ServiceItemSource.LookupItem,
			"PROVIDER_AGREEMENT" => ServiceItemSource.ProviderAgreement,
			"COSTING_ENGINE" => ServiceItemSource.CostingEngine,
			_ => ServiceItemSource.Unspecified
		};

	public static ProviderBillingJobGroup ToProviderBillingJobGroup(OptionalTables optionalTables) =>
		new ProviderBillingJobGroup(optionalTables.Jobs.Map(e => BuildJob(optionalTables, e)));

	public static Option<JobBillingSubmissionDetail> ToSubmissionDetailLatest(OptionalTables optionalTables) =>
			Optional(
				optionalTables.JobbillingSubmissionSources.
				Map(e => new JobBillingSubmissionDetail(
					new DateTimeOffset(e.SubmitOnDate),
					JobBillingSubmittedBySourceToEntity(e.SubmissionSource)))
				.FirstOrDefault());

	public static JobBillingPayment ToPayment(OptionalTables optionalTables)
	{
		Option<JobBillingPayment> optionalJobBillingPayment =
			Optional(
			optionalTables.Payments
			.Map(e => new JobBillingPayment(
				new PaymentAmount(e.TotalAmountPaid),
				NonEmptyText.NewOptionUnvalidated(e.PaymentTerms),
				ToPayments(optionalTables)))
			.FirstOrDefault());                                                 //TODO there should never be more than 1 but without limiting the db table to 1 one we can't guarantee it

		return optionalJobBillingPayment.IfNone(() => new JobBillingPayment(new PaymentAmount(0), Option<NonEmptyText>.None, Lst<JobBillingPaymentTransaction>.Empty));
	}

	public static JobBillingProcessingFee ToProcessingFee(OptionalTables optionalTables) =>
		new JobBillingProcessingFee(
			optionalTables.ProcessingFees
				.Map(e => new JobBillingProcessingFeeLineItem(
					NonEmptyText.NewUnsafe(e.Description),
					new DT.Domain.ProcessingFee(e.ProcessingFeeValue)
				))
				.Freeze());

	public static JobBillingDecoratedEquipmentFlatRate ToEquipmentFlatRate(ProviderBillingId providerBillingId, OptionalTables optionalTables) =>
		new JobBillingDecoratedEquipmentFlatRate(
	0,
			Lst<JobBillingDecoratedEquipmentFlatRateLineItem>.Empty,
			ToJobBillingCostDiscountLineItems(providerBillingId, EntityType.FLAT_RATE_EQUIPMENT, optionalTables),
			ToJobBillingRuleMessage(providerBillingId.Value, EntityType.FLAT_RATE_EQUIPMENT_HEADER, optionalTables)
		);                                                                      //TODO don't see in database

	public static JobBillingDecoratedMaterialPartFlatRate ToMaterialPartFlatRate(ProviderBillingId providerBillingId, OptionalTables optionalTables) =>
		new JobBillingDecoratedMaterialPartFlatRate(
	0,
			Lst<JobBillingDecoratedMaterialPartFlatRateLineItem>.Empty,
			ToJobBillingCostDiscountLineItems(providerBillingId, EntityType.FLAT_RATE_MATERIAL, optionalTables),
			ToJobBillingRuleMessage(providerBillingId.Value, EntityType.FLAT_RATE_MATERIAL_HEADER, optionalTables)
		);                                                                      //TODO don't see in database

	public static JobBillingJobFlatRate ToJobFlatRate(ProviderBillingId providerBillingId, OptionalTables optionalTables) =>
	new JobBillingJobFlatRate(
	0,
			Lst<JobBillingJobFlatRateLineItem>.Empty,
			ToJobBillingCostDiscountLineItems(providerBillingId, EntityType.FLAT_RATE, optionalTables),
			ToJobBillingRuleMessage(providerBillingId.Value, EntityType.FLAT_RATE_HEADER, optionalTables)
		);                                                                      //TODO don't see in database

	public static JobBillingDecoratedLabor ToLabor(ProviderBillingId providerBillingId, OptionalTables optionalTables) =>
		new JobBillingDecoratedLabor(
			optionalTables.LaborLineItems.Sum(lineItem => lineItem.Cost),
			optionalTables.LaborLineItems.Map(lineItem => ToLaborLineItem(lineItem, optionalTables)),
			ToJobBillingCostDiscountLineItems(providerBillingId, EntityType.LABOR, optionalTables),
			ToJobBillingRuleMessage(providerBillingId.Value, EntityType.LABOR_HEADER, optionalTables)
		);

	public static JobBillingTripCharge ToTechnicianTripCharge(OptionalTables optionalTables) =>
	   new JobBillingTripCharge(optionalTables.TripChargeLineItems
		   .Map(e => new JobBillingTripChargeLineItem(
			   NonEmptyText.NewUnsafe(e.Description),
			   e.RequestedByDate,
			   e.ArrivalDate,
			   RateType.Regular,                                            //TODO rate type is not in the database for trip charge
			   new TripChargeRate(0),                                       //TODO no trip charge rate
			   new LineItemCost(e.TripCost),                                //TODO I think this is right
			   e.IsTripPayable,
			   e.IsRequiredByDateMissed,
			   ProviderBillingMapper.CreationSourceToEntity(e.CreationSource)
		   )).Freeze());

	public static JobBillingDecoratedEquipment ToEquipment(ProviderBillingId providerBillingId, OptionalTables optionalTables)
	{
		var catalogItems = optionalTables.CatalogEquipmentLineItems
			.Map(catalogItem =>
				new JobBillingDecoratedEquipmentLineItem(
					ToJobBillingEquipmentLineItem(catalogItem, optionalTables),
					ToJobBillingElementDispute(catalogItem.ItemId, EntityType.EQUIPMENT, optionalTables)));

		var nonCatalogItems = optionalTables.NonCatalogEquipmentLineItems
			.Map(nonCatalogItem =>
				new JobBillingDecoratedEquipmentLineItem(
					ToJobBillingEquipmentLineItem(nonCatalogItem, optionalTables),
					ToJobBillingElementDispute(nonCatalogItem.ItemId, EntityType.EQUIPMENT, optionalTables)));

		return new JobBillingDecoratedEquipment(
			catalogItems.Union(nonCatalogItems).Freeze(),
			ToJobBillingCostDiscountLineItems(providerBillingId, EntityType.EQUIPMENT, optionalTables),
			ToJobBillingRuleMessage(providerBillingId.Value, EntityType.EQUIPMENT_HEADER, optionalTables)
		);
	}

	public static JobBillingDecoratedMaterialPart ToMaterialPart(ProviderBillingId providerBillingId, OptionalTables optionalTables)
	{
		var catalogItems = optionalTables.CatalogMaterialPartLineItems
			.Map(catalogItem =>
				new JobBillingDecoratedMaterialPartLineItem(
					ToJobBillingMaterialPartLineItem(catalogItem, optionalTables),
					ToJobBillingElementDispute(catalogItem.ItemId, EntityType.MATERIAL, optionalTables)));

		var nonCatalogItems = optionalTables.NonCatalogMaterialPartLineItems
			.Map(nonCatalogItem =>
				new JobBillingDecoratedMaterialPartLineItem(
					ToJobBillingMaterialPartLineItem(nonCatalogItem, optionalTables),
					ToJobBillingElementDispute(nonCatalogItem.ItemId, EntityType.MATERIAL, optionalTables)));

		return new JobBillingDecoratedMaterialPart(
			catalogItems.Union(nonCatalogItems).Freeze(),
			ToJobBillingCostDiscountLineItems(providerBillingId, EntityType.MATERIAL, optionalTables),
			ToJobBillingRuleMessage(providerBillingId.Value, EntityType.MATERIAL_HEADER, optionalTables)
		);
	}

	public static ProviderBillingVisitDetail ToVisitDetail(OptionalTables optionalTables) =>
		 new ProviderBillingVisitDetail(
			optionalTables.Visits.Map(e => ToVisitDetail(e, optionalTables)).Freeze()
			);

	public static ProviderBillingBillingDetail ToBillingDetail(OptionalTables optionalTables) =>
		new ProviderBillingBillingDetail(
			optionalTables.BillingLevelLineItems.Map(ToBillingDetail).Freeze()
	);
	public static Option<TableModels.ProviderBilling> ToProviderBilling(Option<TableModels.NonRoutineProviderBilling> nonRoutineProviderBilling) =>
	nonRoutineProviderBilling.Map(e => new TableModels.ProviderBilling(
	e.ProviderBillingId,
			e.TicketId,
			e.TicketNumber,
			e.ProviderOrgId,
			e.CustomerId,
			e.PropertyId,
			e.ServiceLineId,
			e.ServiceTypeId,
			e.CostingScheme,
			e.Status,
			e.TotalCost,
			e.ProviderInvoiceNumber,
			e.Version,
			e.JobSummary,
			PsaId: null,
			Description: null,
			Notes: null,
			ProviderBillingNumber: null,
			BillingType.NewNonRoutine(NonRoutineBillingType.Standard),
	InvoiceCreatedAt: null
	));
	public static Option<TableModels.ProviderBilling> ToProviderBilling(Option<TableModels.RoutineProviderBilling> routineProviderBilling, BillingType billingType) =>
	routineProviderBilling.Map(e => new TableModels.ProviderBilling(
	e.ProviderBillingId,
	e.TicketId,
			e.TicketNumber,
			e.ProviderOrgId,
			e.CustomerId,
			e.PropertyId,
			e.ServiceLineId,
			ServiceTypeId: null,
			e.CostingScheme,
			e.Status,
			e.TotalCost,
			e.ProviderInvoiceNumber,
			e.Version,
			JobSummary: null,
			e.PsaId,
			e.Description,
			e.Notes,
			e.ProviderBillingNumber,
			billingType,
			e.InvoiceCreatedAt
			));

	public static RecordMeta ToRecordMeta(Guid guid, EntityType entityType, OptionalTables optionalTables) =>
		ToRecordMeta(optionalTables.MetaDatas
			.Where(e => e.EntityId == guid
				&& e.EntityType == entityType.ToString())
	.FirstOrDefault());

	public static Lst<DT.Domain.JobBillingRuleMessage> ToJobBillingRuleMessage(Guid guid, EntityType entityType, OptionalTables optionalTables) =>
		ToJobBillingRuleMessage(optionalTables.JobbillingRuleMessages
			.Where(e => e.EntityId == guid
				&& e.EntityType == entityType.ToString())
			.Freeze(),
	optionalTables);

	public static Option<DT.Domain.Event> ToEvent(OptionalTables optionalTables) =>
		Optional<TableModels.Event>(optionalTables.Events.FirstOrDefault())
			.Map(e =>
			{
				Option<TableModels.WeatherWorks> weatherWorks = optionalTables.WeatherWorks.FirstOrDefault();
				Option<BillingLevelLineItem> billingLevelLineItemOption = optionalTables.BillingLevelLineItems.FirstOrDefault();
				return new DT.Domain.Event(
				new EventId(e.EventId),
				new ServiceItemId(e.ServiceItemId),
				NonEmptyText.NewUnsafe(e.Name),
				SourceToEntity(e.Source),
				e.Amount,
				e.EventStart,
				e.EventEnd,
				new SourceId(e.SourceId),
				//Optional Fields
				NonEmptyText.NewUnsafe(e.Description),
				new EventLineItemId(e.EventLineItemId),

				//TODO this is a bandaid on a bandaid and we shouldn't have to do this but for now its needed
				ModifyByToEntity(
					weatherWorks.Match(a => "DM",
						billingLevelLineItemOption.Match(b => b.ModifyBy,
							() => e.ModifyBy))),
				ModifyActionToEntity(billingLevelLineItemOption.Match(b => b.ModifyAction, () => e.ModifyAction)),
				NonEmptyText.NewOptionUnvalidated(e.Reason)
				);
			})
			//front end always needs an event even if its blank
			.IfNone(() => new DT.Domain.Event(
				new EventId(new Guid()),
				new ServiceItemId(new Guid()),
				Name: NonEmptyText.NewUnsafe(""),         //this is intentional and was requested by the front end
				SourceToEntity(""),                 //this is intentional and was requested by the front end
				Amount: 0,
				EventStart: new DateTimeOffset(new DateTime(DateTime.Now.Year, 1, 1)),  //this is intentional and was requested by the front end
				EventEnd: new DateTimeOffset(new DateTime(DateTime.Now.Year, 12, 31)),  //this is intentional and was requested by the front end
				new SourceId(new Guid()),
				//Optional Fields
				Description: NonEmptyText.NewOptionUnvalidated(""),         //this is intentional and was requested by the front end
				EventLineItemId: Option<EventLineItemId>.None,
				Option<ModifyBy>.None,
				Option<ModifyAction>.None,
				Reason: Option<NonEmptyText>.None
			));

	public static Option<DT.Domain.WeatherWorks> ToWeatherWorks(OptionalTables optionalTables) =>
		Optional<TableModels.WeatherWorks>(optionalTables.WeatherWorks.FirstOrDefault())
			.Map(e => new DT.Domain.WeatherWorks(
				e.Snow,
				e.Ice,
				e.TemperatureInFahrenheit.HasValue ? Some(e.TemperatureInFahrenheit.Value) : Option<decimal>.None,
				NonEmptyText.NewOptionUnvalidated(e.Description)
			));
	public static Lst<DT.Domain.MultiVisitJobRate> ToMultiVisitJob(Lst<TableModels.MultiVisitJob> multiVisitJobs) =>
	multiVisitJobs
	.Map(e => new DT.Domain.MultiVisitJobRate(
				new MultiJobVisitId(e.MultiVisitJobId),
				new ProviderBillingId(e.ProviderBillingId),
				new ServiceBasedCostingId(e.ServiceBasedCostingId),
				NonEmptyText.NewUnsafe(e.Name),
				e.Rate,
				UnitOfMeasureToEntity(e.UnitType)
			));

	public static DT.Domain.ProviderBillingTimeAndMaterialLineItem ToTimeAndMaterialLineItem(TimeAndMaterialLineItem e, OptionalTables optionalTables) =>
		new(
			new VisitId(e.VisitId),
			NonEmptyText.NewOptionUnvalidated(e.Reason),
			new ServiceItemId(e.ServiceItemId),
			new LineItemId(e.ItemId),
			new DateTimeOffset(e.ClientTime),
			e.ItemSequenceNumber,
			ProviderBillingSourceToEntity(e.Source),
			AdjustQuanityToMinutesIfNeeded(e.Quantity, UnitOfMeasureToEntity(e.UnitType)),
			e.Amount,
			ServiceItemSourceToEntity(e.ServiceItemSource),
			UnitOfMeasureToEntity(e.UnitType),
			e.Rate,
			e.RateUnitType,
			ModifyByToEntity(e.ModifyBy),
			ModifyActionToEntity(e.ModifyAction),
			ToAdjustment(e.ItemId, EntityType.TIME_AND_MATERIAL_LINE_ITEM, optionalTables, UnitOfMeasureToEntity(e.UnitType)));

	private static decimal AdjustQuanityToMinutesIfNeeded(decimal quantity, UnitOfMeasure unitType)
	{
		switch (unitType)
		{
			case UnitOfMeasure.Day:
				return quantity * 60 * 24;
			case UnitOfMeasure.Hour:
				return quantity * 60;
			case UnitOfMeasure.Second:
				return quantity / 60;
			default:
				return quantity;
		}
	}

	public static Option<DT.Domain.ProviderBillingTripChargeLineItem> ToProviderBillingTripChargeLineItem(OptionalTables optionalTables) =>
			Optional<TableModels.ProviderBillingTripChargeLineItem>(optionalTables.ProviderBillingTripChargeLineItems.FirstOrDefault())
			.Map(e => new DT.Domain.ProviderBillingTripChargeLineItem(
				new LineItemId(e.LineItemId),
				NonEmptyText.NewUnsafe(e.Name),
				e.Rate,
				e.Amount,
				e.Quantity,
				e.MaximumChargeableTrips,
				ProviderBillingSourceToEntity(e.Source)));

	private static ProviderBillingJob BuildJob(OptionalTables optionalTables, TableModels.Job job)
	{
		Option<DT.Domain.Job> jobOption = (SorConcentratorApi.GetById(new JobWorkId(job.JobWorkId))).ToOption();

		return new ProviderBillingJob(
							new JobWorkId(job.JobWorkId),
							new ServiceTypeId(job.ServiceTypeId),
							NonEmptyText.NewUnsafe(job.JobWorkNumber),
							ProviderBillingMapper.UrgencyToEntity(job.Urgency),
							Optional<DateTimeOffset>(job.JobCompleteDateTime != null ? new DateTimeOffset((DateTime)job.JobCompleteDateTime) : null),
							NonEmptyText.NewUnsafe(job.Scope),
							jobOption.Match(e => e.JobWorkState, Option<JobWorkState>.None),
							jobOption.Match(e => e.JobWorkStatus, Option<JobWorkStatus>.None),
							ToBillingJobAttachmentMeta(optionalTables, job),
							jobOption.Match(e => e.JobCondition, Option<JobCondition>.None),
		jobOption.Match(e => Option<Lst<Costing>>.Some(e.Costings), Option<Lst<Costing>>.None)
		);
	}

	public static ProviderBillingDiscount ToProviderBillingDiscount(OptionalTables optionalTables)
	{
		var providerDiscounts = optionalTables.Discounts
				.Where(e => ProviderBillingSourceToEntity(e.Source) == ProviderBillingSource.Provider);

		return new ProviderBillingDiscount(
			optionalTables.Discounts
				.Where(e => ProviderBillingSourceToEntity(e.Source) != ProviderBillingSource.Provider)
				.Sum(e => e.DiscountAmount),
			providerDiscounts
				.Sum(e => e.DiscountAmount),
			GetProviderDiscountId(providerDiscounts)
		);
	}

	private static Option<Guid> GetProviderDiscountId(IEnumerable<Discount> providerDiscounts)
	{
		if (providerDiscounts.Count() > 0)
		{
			return providerDiscounts
				.Select(e => e.DiscountId)
				.FirstOrDefault();
		}
		else
		{
			return Option<Guid>.None;
		}
	}

	private static Lst<JobBillingMessageVisibility> ToJobBillingRuleVisibility(Guid guid, OptionalTables optionalTables) =>
	   optionalTables.JobbillingRuleMessageVisibilities
		   .Where(e => e.MessageId == guid)
		   .Map(e => JobBillingMessageVisibilityToEntity(e.MessageVisibility))
	.Freeze();
	private static Lst<DT.Domain.JobBillingRuleMessage> ToJobBillingRuleMessage(Lst<TableModels.JobBillingRuleMessage> optionaJobbillingRuleMessage, OptionalTables optionalTables) =>
	optionaJobbillingRuleMessage.Map(e =>
	new DT.Domain.JobBillingRuleMessage(
	JobBillingMessageTypeToEntity(e.MessageType),
				JobBillingMessageRuleToEntity(e.MessageRule),
				NonEmptyText.NewOptionUnvalidated(e.MessageText),
				ToJobBillingRuleVisibility(e.MessageId, optionalTables)
		));

	private static RecordMeta ToRecordMeta(Option<MetaData> optionMetaData) =>
		optionMetaData.Match(
			e => new RecordMeta(
				new UserId(e.CreatedByUserId),
				new DateTimeOffset(e.CreatedOnDateTime),
				new UserId(e.ModifiedByUserId),
				new DateTimeOffset(e.ModifiedOnDateTime)),
			() => new RecordMeta(
				Option<UserId>.None,
				Option<DateTimeOffset>.None,
				Option<UserId>.None,
				Option<DateTimeOffset>.None
				));

	private static BillingJobAttachmentMeta ToBillingJobAttachmentMeta(OptionalTables optionalTables, TableModels.Job job)
	{
		var before = optionalTables.Photos.Where(e => ToProviderBillingPhotoChronology(e.PhotoChronology ?? MissingValue) == PhotoChronology.BeforePhoto);
		var after = optionalTables.Photos.Where(e => ToProviderBillingPhotoChronology(e.PhotoChronology ?? MissingValue) == PhotoChronology.AfterPhoto);

		var visitIds = optionalTables.Visits.Where(e => e.JobWorkId == job.JobWorkId).Select(e => e.VisitId).ToHashSet();
		var serviceLineIds = optionalTables.ServiceLineItems.Where(e => visitIds.Contains(e.VisitId)).Select(e => e.ItemId).ToHashSet();

		int beforeCount = CountPhotos(before, visitIds) + CountPhotos(before, serviceLineIds);
		int afterCount = CountPhotos(after, visitIds) + CountPhotos(after, serviceLineIds);

		return new BillingJobAttachmentMeta(
			new PhotoBeforeCount(beforeCount),
			new PhotoAfterCount(afterCount));
	}

	private static int CountPhotos(IEnumerable<TableModels.Photo> before, System.Collections.Generic.HashSet<Guid> visitIds) =>
		before.Where(e => visitIds.Contains(e.EntityId)).Count();

	private static Lst<JobBillingPaymentTransaction> ToPayments(OptionalTables optionalTables)
	{
		var creditCards = optionalTables.PaymentCreditCards
			.Map(e => new JobBillingPaymentTransaction(
				new PaymentAmount(0),
				new JobBillingPaymentMethodCreditCard(
					ProviderBillingMapper.CreditCardProviderToEntity(e.CreditCardProvider),
					e.PaidAtDateTime,
					NonEmptyText.NewUnsafe(e.Last4Digits),                              //TODO db has not null, c# has option
					NonEmptyText.NewOptionUnvalidated(e.TransactionReferenceCode))))
			.Freeze();

		var electronicFundTransfers = optionalTables.PaymentElectronicFundTransfers
			.Map(e => new JobBillingPaymentTransaction(
				new PaymentAmount(0),
				new JobBillingPaymentMethodElectronicFundTransfer(
					e.PaidAtDateTime,
					NonEmptyText.NewOptionUnvalidated(e.TransactionReferenceCode))))
			.Freeze();

		return creditCards.Union(electronicFundTransfers).Freeze();
	}

	private static IJobBillingDecoratedLaborLineItem ToLaborLineItem(LaborLineItem laborLineItem, OptionalTables optionalTables) =>
		new JobBillingDecoratedLaborLineItem(
			new LineItemId(laborLineItem.ItemId),
			new JobWorkId(FindJobWorkIdForLaborLineItem(laborLineItem, optionalTables)),
			ProviderBillingMapper.TechnicianTypeToEntity(laborLineItem.TechnicianType),
			laborLineItem.Hours,
			0,                                                                          //TODO either get from db or calculate like this? (long)TimeSpan.FromHours((double)laborLineItem.Hours).TotalSeconds,
			0,                                                                          //TODO either get from db or calculate
			new LaborRate(laborLineItem.LaborRate),
			laborLineItem.Cost,
			ProviderBillingMapper.RateTypeToEntity(laborLineItem.RateType),
			ToJobBillingElementDispute(laborLineItem.ItemId, EntityType.LABOR, optionalTables),
			ToTimeAdjustments(laborLineItem, optionalTables),
			ToTechnicianTrip(laborLineItem, optionalTables),
			ToJobBillingRuleMessage(laborLineItem.ItemId, EntityType.LABOR, optionalTables)
		);

	private static Guid FindJobWorkIdForLaborLineItem(LaborLineItem laborLineItem, OptionalTables optionalTables) =>
		Optional<Guid>(optionalTables.LaborJobAssignments
						.Where(e => e.ItemId == laborLineItem.ItemId)
						.Select(e => e.JobWorkId)
						.FirstOrDefault())
			.IfNone(new Guid());


	private static Lst<JobBillingDecoratedLaborLineItemTechnicianTrip> ToTechnicianTrip(LaborLineItem laborLineItem, OptionalTables optionalTables) =>
	  optionalTables.TechnicianLabors.Where(e => e.LaborItemId == laborLineItem.ItemId)
		  .Map(e => new JobBillingDecoratedLaborLineItemTechnicianTrip(
			  new TechnicianTripId(e.TechnicianLaborLineId),                            //?
			  new UserId(e.TechnicianUserId),
			  CheckInDateTime: MissingDate,                                             //TODO not sure where this is stored
			  CheckOutDateTime: MissingDate,                                            //TODO not sure where this is stored
			  e.TotalBillableTimeSeconds,
			  e.IsPayable,
			  ProviderBillingMapper.CreationSourceToEntity(e.LineItemSource)
		  ))
		  .Freeze();

	private static Lst<IJobBillingLaborLineItemTimeAdjustment> ToTimeAdjustments(LaborLineItem laborLineItem, OptionalTables optionalTables) =>
		optionalTables.LaborTimeAdjustments.Where(e => e.ItemId == laborLineItem.ItemId)
			.Map(e => new JobBillingLaborLineItemTimeAdjustment(
				new AdjustmentMinutes(e.TimeAdjustmentInMinutes),
				new TimeAdjustmentCost(e.TimeAdjustmentInMinutes * laborLineItem.LaborRate),
				e.IsProviderConfirmed,
				ProviderBillingMapper.CreationSourceToEntity(e.CreationSource),
				NonEmptyText.NewOptionUnvalidated(e.Reason),
				ToRecordMeta(laborLineItem.ItemId, EntityType.TIME_ADJUSTMENT, optionalTables),
				ToJobBillingRuleMessage(laborLineItem.ItemId, EntityType.TIME_ADJUSTMENT, optionalTables)
			))
			.Cast<IJobBillingLaborLineItemTimeAdjustment>()
			.Freeze();

	private static JobBillingEquipmentLineItem ToJobBillingEquipmentLineItem(NonCatalogedEquipmentLineItem nonCatalogItem, OptionalTables optionalTables) =>
			new JobBillingEquipmentLineItem(
				nonCatalogItem.ProviderBillingId,
				ProviderBillingMapper.EquipmentCatalogItemTypeEntity(nonCatalogItem.EquipmentKind),
				new JobBillingMaterialPartEquipmentNonCatalogItem(NonEmptyText.NewUnsafe(nonCatalogItem.Name)),
				nonCatalogItem.Quantity,
				UnitOfMeasure.Unspecified,                                              //TODO not in database
				new ItemCost(nonCatalogItem.ItemCost),
				new LineItemCost(nonCatalogItem.Quantity * nonCatalogItem.ItemCost),    //this is planed to be calculated in a view if its needed to be presented
				ProviderBillingMapper.CreationSourceToEntity(nonCatalogItem.CreationSource),
				nonCatalogItem.JobWorkId.Match(
					e => Some(new JobWorkId(e)),
					() => Option<JobWorkId>.None),
				NonEmptyText.NewOptionUnvalidated(nonCatalogItem.Reason),
				ToRecordMeta(nonCatalogItem.ItemId, EntityType.EQUIPMENT, optionalTables),
				ToJobBillingRuleMessage(nonCatalogItem.ItemId, EntityType.EQUIPMENT, optionalTables)
			);

	private static JobBillingEquipmentLineItem ToJobBillingEquipmentLineItem(CatalogedEquipmentLineItem catalogItem, OptionalTables optionalTables) =>
		new JobBillingEquipmentLineItem(
			catalogItem.ProviderBillingId,
			ProviderBillingMapper.EquipmentCatalogItemTypeEntity(catalogItem.EquipmentKind),
			new JobBillingMaterialPartEquipmentCatalogItem(
				new CatalogItemId(catalogItem.CatalogItemReference),
				ToEquipmentRules(catalogItem, optionalTables)),
			catalogItem.Quantity,
			UnitOfMeasure.Unspecified,                                                  //TODO not in database
			new ItemCost(catalogItem.ItemCost),
			new LineItemCost(catalogItem.Quantity * catalogItem.ItemCost),              //this is planed to be calculated in a view if its needed to be presented
			ProviderBillingMapper.CreationSourceToEntity(catalogItem.CreationSource),
			catalogItem.JobWorkId.Match(
				e => Some(new JobWorkId(e)),
				() => Option<JobWorkId>.None),
			NonEmptyText.NewOptionUnvalidated(catalogItem.Reason),
			ToRecordMeta(catalogItem.ItemId, EntityType.EQUIPMENT, optionalTables),
			ToJobBillingRuleMessage(catalogItem.ItemId, EntityType.EQUIPMENT, optionalTables)
	);

	private static Lst<JobBillingCatalogItemRuleValue> ToEquipmentRules(CatalogedEquipmentLineItem catalogItem, OptionalTables optionalTables) =>
		optionalTables.EquipmentLineItemRules.Where(e => catalogItem.ItemId == e.EquipmentLineItemId)
			.Map(e => new JobBillingCatalogItemRuleValue(
				NonEmptyText.NewUnsafe(e.Name),
				NonEmptyText.NewOptionUnvalidated(e.Value)
			))
			.Freeze();

	private static JobBillingMaterialPartLineItem ToJobBillingMaterialPartLineItem(NonCatalogedMaterialPartLineItem nonCatalogItem, OptionalTables optionalTables) =>
		new JobBillingMaterialPartLineItem(
			nonCatalogItem.ProviderBillingId,
			ProviderBillingMapper.MaterialPartCatalogItemTypeEntity(nonCatalogItem.MaterialPartKind),
			new JobBillingMaterialPartEquipmentNonCatalogItem(NonEmptyText.NewUnsafe(nonCatalogItem.Name)),
			nonCatalogItem.Quantity,
			UnitOfMeasure.Unspecified,                                                  //TODO not specified in the database
			new ItemCost(nonCatalogItem.ItemCost),
			new LineItemCost(nonCatalogItem.Quantity * nonCatalogItem.ItemCost),        //this is planed to be calculated in a view if its needed to be presented
			ProviderBillingMapper.CreationSourceToEntity(nonCatalogItem.CreationSource),
			nonCatalogItem.JobWorkId.Match(
				e => Some(new JobWorkId(e)),
				() => Option<JobWorkId>.None),
			NonEmptyText.NewOptionUnvalidated(nonCatalogItem.Reason),
			ToRecordMeta(nonCatalogItem.ItemId, EntityType.MATERIAL, optionalTables),
			ToJobBillingRuleMessage(nonCatalogItem.ItemId, EntityType.MATERIAL, optionalTables)
	  );

	private static JobBillingMaterialPartLineItem ToJobBillingMaterialPartLineItem(CatalogedMaterialPartLineItem catalogItem, OptionalTables optionalTables) =>
		new JobBillingMaterialPartLineItem(
			catalogItem.ProviderBillingId,
			ProviderBillingMapper.MaterialPartCatalogItemTypeEntity(catalogItem.MaterialPartKind),
			new JobBillingMaterialPartEquipmentCatalogItem(
				new CatalogItemId(catalogItem.CatalogItemReference),
				ToMaterialRules(catalogItem, optionalTables)),
			catalogItem.Quantity,
			UnitOfMeasure.Unspecified,                                                  //TODO not specified in the database
			new ItemCost(catalogItem.ItemCost),
			new LineItemCost(catalogItem.Quantity * catalogItem.ItemCost),              //this is planed to be calculated in a view if its needed to be presented
			ProviderBillingMapper.CreationSourceToEntity(catalogItem.CreationSource),
			catalogItem.JobWorkId.Match(
				e => Some(new JobWorkId(e)),
				() => Option<JobWorkId>.None),
			NonEmptyText.NewOptionUnvalidated(catalogItem.Reason),
			ToRecordMeta(catalogItem.ItemId, EntityType.MATERIAL, optionalTables),
			ToJobBillingRuleMessage(catalogItem.ItemId, EntityType.MATERIAL, optionalTables)
		);

	private static Lst<JobBillingCatalogItemRuleValue> ToMaterialRules(CatalogedMaterialPartLineItem catalogItem, OptionalTables optionalTables) =>
		optionalTables.MaterialPartLineItemRules.Where(e => catalogItem.ItemId == e.MaterialPartLineItemId)
			.Map(e => new JobBillingCatalogItemRuleValue(
				NonEmptyText.NewUnsafe(e.Name),
				NonEmptyText.NewOptionUnvalidated(e.Value)
			))
			.Freeze();

	private static bool CheckInOutWithinEvent(DateTime? weatherDateStart, DateTime? weatherDateEnd, DateTime checkInDate, DateTime checkOutDate)
	{
		if (weatherDateStart == null || weatherDateEnd == null)
		{
			return false;
		}
		return checkInDate <= weatherDateEnd && checkOutDate >= weatherDateStart;
	}

	private static ProviderBillingVisit ToVisitDetail(Visit visit, OptionalTables optionalTables) =>
		new ProviderBillingVisit(
			new VisitId(visit.VisitId),
			new DateTimeOffset(visit.CheckIn),
			new DateTimeOffset(visit.CheckOut),
			VisitSourceToEntity(visit.SourceTicketStage),
			visit.MissedCheckIn,
			visit.Addendum,
			optionalTables.WeatherWorks
				.Where(w => w.ProviderBillingId == visit.ProviderBillingId && CheckInOutWithinEvent(w.EventStart, w.EventEnd, visit.CheckIn, visit.CheckOut))
				.Map(w => NonEmptyText.NewOptionUnvalidated(w.Description))
				.FirstOrDefault(),
			visit.JobWorkId == null ? new JobWorkId(Guid.Empty) : new JobWorkId(visit.JobWorkId.Value),
			optionalTables.ServiceLineItems
				.Where(e => e.VisitId == visit.VisitId)
				.Map(e => ToProviderBillingServiceLineItem(e, optionalTables))
				.Freeze(),
			optionalTables.TimeAndMaterialLineItems
				.Where(e => e.VisitId == visit.VisitId)
				.Map(e => ToTimeAndMaterialLineItem(e, optionalTables))
				.Freeze()
			);

	private static ProviderBillingServiceLineItem ToProviderBillingServiceLineItem(ServiceLineItem serviceLineItem, OptionalTables optionalTables) =>
		new ProviderBillingServiceLineItem(
			new LineItemId(serviceLineItem.ItemId),
			NonEmptyText.NewOptionUnvalidated(serviceLineItem.ItemName),
			new DateTimeOffset(serviceLineItem.DateTime),
			new ServiceRate(serviceLineItem.ServiceRate),
			1,                                                                          //TODO this is always 1?
			new LineItemCost(serviceLineItem.Cost),
			ServiceLineItemToEntity(serviceLineItem.Type),
			new ServiceTypeId(serviceLineItem.ServiceTypeId),
			new PerOccurrenceItemId(serviceLineItem.PerOccurrenceItemId),
			ModifyByToEntity(serviceLineItem.ModifyBy),
			ModifyActionToEntity(serviceLineItem.ModifyAction),
			NonEmptyText.NewOptionUnvalidated(serviceLineItem.Reason),
			ToJobBillingElementDispute(serviceLineItem.ItemId, EntityType.SERVICE_LINE_ITEM, optionalTables),
			ToAdjustment(serviceLineItem.ItemId, EntityType.SERVICE_LINE_ITEM, optionalTables, serviceLineItem)
			);

	private static Option<VisitServiceLineItemAdjustment> ToAdjustment(Guid itemId, EntityType entityType, OptionalTables optionalTables, ServiceLineItem serviceLineItem) =>
		 Optional<Adjustment>(
			optionalTables.Adjustments
				.Where(e => e.EntityId == itemId
					&& e.EntityType == entityType.ToString())
				.FirstOrDefault()
			)
			.Map(e => new VisitServiceLineItemAdjustment(
					NonEmptyText.NewOptionUnvalidated(serviceLineItem.ItemName),
					new LineItemId(serviceLineItem.ItemId),
					e.Amount
				)
			);

	private static Option<TimeAndMaterialLineItemAdjustment> ToAdjustment(Guid itemId, EntityType entityType, OptionalTables optionalTables, UnitOfMeasure unitType) =>
		 Optional<Adjustment>(
			optionalTables.Adjustments
				.Where(e => e.EntityId == itemId
					&& e.EntityType == entityType.ToString())
				.FirstOrDefault()
			)
			.Map(e => new TimeAndMaterialLineItemAdjustment(
					LineItemId: new LineItemId(itemId),
					Quantity: AdjustQuanityToMinutesIfNeeded(e.Quantity, unitType),
					Amount: e.Amount
				)
			);

	private static ProviderBillingBillingLineItem ToBillingDetail(BillingLevelLineItem billingLevelLineItem) =>
		new ProviderBillingBillingLineItem(
			new LineItemId(billingLevelLineItem.ItemId),
			NonEmptyText.NewUnsafe(billingLevelLineItem.ItemName),
			BillingLineItemTypeToEntity(billingLevelLineItem.Type),
			billingLevelLineItem.Price,
			billingLevelLineItem.Quantity
			);

	private static Option<JobBillingElementDispute> ToJobBillingElementDispute(Guid itemId, EntityType entityType, OptionalTables optionalTables) =>
		Optional<Dispute>(
			optionalTables.JobBillingDispute
				.Where(e => e.EntityId == itemId
					&& e.EntityType == entityType.ToString())
				.FirstOrDefault()
			)
			.Map(e => new JobBillingElementDispute(
					new JobBillingDisputeId(e.DisputeId),
					ToJobBillingElementDisputeConversation(e.DisputeId, optionalTables)
				)
			);

	private static Lst<JobBillingElementDisputeConversation> ToJobBillingElementDisputeConversation(Guid disputeId, OptionalTables optionalTables) =>
		optionalTables.JobBillingDisputeConversations
			.Where(e => e.DisputeId == disputeId)
			.Map(e => new JobBillingElementDisputeConversation(
				new DisputeConversationId(e.DisputeConversationId),
				e.DisputeConversationOrder,
				ToJobBillingElementDisputeRequest(e.DisputeConversationId, optionalTables),
				ToRecordMeta(e.DisputeConversationId, EntityType.JOB_BILLING_DISPUTE_CONVERSATION, optionalTables),
				ToJobBillingElementDisputeResponse(e.DisputeConversationId, optionalTables)))
			.Freeze();

	private static Option<JobBillingElementDisputeResponse> ToJobBillingElementDisputeResponse(Guid disputeConversationId, OptionalTables optionalTables) =>
		Optional<JobBillingElementDisputeResponse>(
			optionalTables.JobBillingDisputeResponses
				.Where(e => e.DisputeConversationId == disputeConversationId)
				.Map(e => new JobBillingElementDisputeResponse(
					NonEmptyText.NewUnsafe(e.Message),
					ToRecordMeta(e.DisputeResponseId, EntityType.JOB_BILLING_DISPUTE_RESPONSE, optionalTables)
					))
			.FirstOrDefault());

	private static JobBillingElementDisputeRequest ToJobBillingElementDisputeRequest(Guid disputeConversationId, OptionalTables optionalTables) =>
		Optional<JobBillingElementDisputeRequest>(
			optionalTables.JobBillingDisputeRequests
				.Where(e => e.DisputeConversationId == disputeConversationId)
				.Map(e => new JobBillingElementDisputeRequest(
					ToJobBillingLineItemDisputeRequestReasonEntity(e.DisputeRequestReason),
					ToRecordMeta(e.DisputeRequestId, EntityType.JOB_BILLING_DISPUTE_REQUEST, optionalTables),
					NonEmptyText.NewOptionUnvalidated(e.ReasonText),
					NonEmptyText.NewOptionUnvalidated(e.AdditionalText)
					))
				.FirstOrDefault())
			.IfNone(() => new JobBillingElementDisputeRequest(
				JobBillingLineItemDisputeRequestReason.Other,
				ToRecordMeta(new Guid(), EntityType.JOB_BILLING_DISPUTE_REQUEST, optionalTables),  //this will always return a blank meta data since the guid won't match
				Option<NonEmptyText>.None,
				Option<NonEmptyText>.None));

	private static Lst<IJobBillingCostDiscountLineItem> ToJobBillingCostDiscountLineItems(ProviderBillingId providerBillingId, EntityType entityType, OptionalTables optionalTables) =>
		optionalTables.Adjustments
			.Where(e => EntityTypeToEntity(e.EntityType) == entityType)
			.Map(e => new JobBillingCostDiscountLineItem(
				e.Amount,
				CreationSourceToEntity(e.CreationSource),
				ToRecordMeta(e.AdjustmentId, EntityType.DISCOUNT, optionalTables)
				))
			.Select(e => (IJobBillingCostDiscountLineItem)e)
			.Freeze();


}