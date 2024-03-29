﻿using DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.Map;

public record OptionalTables(
    Lst<Job> Jobs,
    Option<TableModels.NonRoutineProviderBilling> NonRoutineProviderBillingOption,
    Option<TableModels.RoutineProviderBilling> RoutineProviderBillingOption,
    Option<TableModels.ProviderBilling> ProviderBillingOption,
    Option<ProviderBillingAssignment> ProviderInvoiceAssignmentOption,
    Lst<CatalogedMaterialPartLineItem> CatalogMaterialPartLineItems,
    Lst<MaterialPartLineItemRule> MaterialPartLineItemRules,
    Lst<NonCatalogedMaterialPartLineItem> NonCatalogMaterialPartLineItems,
    Lst<CatalogedEquipmentLineItem> CatalogEquipmentLineItems,
    Lst<EquipmentLineItemRule> EquipmentLineItemRules,
    Lst<NonCatalogedEquipmentLineItem> NonCatalogEquipmentLineItems,
    Lst<LaborLineItem> LaborLineItems,
    Lst<LaborJobAssignment> LaborJobAssignments,
    Lst<LaborTimeAdjustment> LaborTimeAdjustments,
    Lst<TechnicianLabor> TechnicianLabors,
    Lst<TripChargeLineItem> TripChargeLineItems,
    Lst<ProviderBillingTripChargeLineItem> ProviderBillingTripChargeLineItems,
    Lst<ProcessingFee> ProcessingFees,
    Lst<Payment> Payments,
    Lst<PaymentCreditCard> PaymentCreditCards,
    Lst<PaymentElectronicFundTransfer> PaymentElectronicFundTransfers,
    Lst<TableModels.Photo> Photos,
    Lst<TableModels.JobDocument> JobDocuments,
    Lst<Visit> Visits,
    Lst<ServiceLineItem> ServiceLineItems,
    Lst<BillingLevelLineItem> BillingLevelLineItems,
    Lst<MetaData> MetaDatas,
    Lst<JobBillingRuleMessage> JobbillingRuleMessages,
    Lst<JobBillingRuleMessageVisibility> JobbillingRuleMessageVisibilities,
    Lst<JobBillingSubmissionSource> JobbillingSubmissionSources,
    Lst<Discount> Discounts,
    Lst<Dispute> JobBillingDispute,
    Lst<JobBillingDisputeConversation> JobBillingDisputeConversations,
    Lst<JobBillingDisputeRequest> JobBillingDisputeRequests,
    Lst<JobBillingDisputeResponse> JobBillingDisputeResponses,
    Lst<Event> Events,
    Lst<TimeAndMaterialLineItem> TimeAndMaterialLineItems,
    Lst<WeatherWorks> WeatherWorks,
    Lst<Adjustment> Adjustments,
    Option<PreProviderBilling> PreProviderBillings,
    Lst<MultiVisitJob> MultiVisitJobs
);