using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using Dmg.Work.Commons.V1;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between work/job protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class WorkMessageMapper 
{
    public static DT.Domain.JobUrgency ToEntityJobUrgency(Dmg.Work.V1.WorkUrgency workUrgencyMessage) =>
        workUrgencyMessage
            switch 
            {
                Dmg.Work.V1.WorkUrgency.Emergency => DT.Domain.JobUrgency.Emergency,
                Dmg.Work.V1.WorkUrgency.High => DT.Domain.JobUrgency.High,
                Dmg.Work.V1.WorkUrgency.Normal => DT.Domain.JobUrgency.Normal,
                Dmg.Work.V1.WorkUrgency.Unknown => DT.Domain.JobUrgency.Unknown,
                _ => DT.Domain.JobUrgency.Unknown
            };

    public static DT.Domain.JobConditionAdditionalRule ToEntityJobConditionAdditionalRule(Dmg.Work.Commons.V1.AdditionalRule additionalRuleMessage) =>
        additionalRuleMessage
            switch 
            {
                AdditionalRule.NoBeforePhotosAllowed => DT.Domain.JobConditionAdditionalRule.NoBeforePhotosAllowed,
                AdditionalRule.NoAfterPhotosAllowed => DT.Domain.JobConditionAdditionalRule.NoAfterPhotosAllowed,
                AdditionalRule.NoDuringPhotosAllowed => DT.Domain.JobConditionAdditionalRule.NoDuringPhotosAllowed,
                AdditionalRule.NteIncreaseNotAllowed => DT.Domain.JobConditionAdditionalRule.NotToExceedAmountIncreaseNotAllowed,
                AdditionalRule.CheckoutQuestionnaireApplicable => DT.Domain.JobConditionAdditionalRule.CheckOutQuestionnaireApplicable,
                AdditionalRule.InspectionApplicable => DT.Domain.JobConditionAdditionalRule.InspectionApplicable,
                AdditionalRule.SameCheckoutExperience => DT.Domain.JobConditionAdditionalRule.SameCheckoutExperience,
                AdditionalRule.EventGroupingDisabled => DT.Domain.JobConditionAdditionalRule.EventGroupingDisabled,
                AdditionalRule.DiwoEnabled => DT.Domain.JobConditionAdditionalRule.DiwoEnabled,
                AdditionalRule.BillingAssignmentPropertyDm => DT.Domain.JobConditionAdditionalRule.BillingAssignmentPropertyDm,
                AdditionalRule.CiwoEnabled => DT.Domain.JobConditionAdditionalRule.CiwoEnabled,
                AdditionalRule.DoesNotOverrideExistingMvj => DT.Domain.JobConditionAdditionalRule.DoesNotOverrideExistingMvj,
                AdditionalRule.ExternalCheckInCheckOutNotAllowed => DT.Domain.JobConditionAdditionalRule.ExternalCheckInCheckOutNotAllowed,
                _ => DT.Domain.JobConditionAdditionalRule.Undefined
            };
    
    public static DT.Domain.JobWorkStatus ToEntityJobWorkStatus(Dmg.Work.V1.WorkStatus workStatusMessage) =>
        workStatusMessage
            switch 
            {
                Dmg.Work.V1.WorkStatus.Draft => DT.Domain.JobWorkStatus.Draft,
                Dmg.Work.V1.WorkStatus.Open => DT.Domain.JobWorkStatus.Open,
                Dmg.Work.V1.WorkStatus.InProgress => DT.Domain.JobWorkStatus.InProgress,
                Dmg.Work.V1.WorkStatus.Completed => DT.Domain.JobWorkStatus.Completed,
                Dmg.Work.V1.WorkStatus.Verified => DT.Domain.JobWorkStatus.Verified,
                Dmg.Work.V1.WorkStatus.Closed => DT.Domain.JobWorkStatus.Closed,
                Dmg.Work.V1.WorkStatus.Cancelled => DT.Domain.JobWorkStatus.Cancelled,
                Dmg.Work.V1.WorkStatus.NoPay => DT.Domain.JobWorkStatus.NoPay,
                Dmg.Work.V1.WorkStatus.OnHold => DT.Domain.JobWorkStatus.OnHold,
                Dmg.Work.V1.WorkStatus.Unknown => DT.Domain.JobWorkStatus.Unknown,
                _ => DT.Domain.JobWorkStatus.Unknown
            };

    public static DT.Domain.JobWorkState ToEntityWorkState(Dmg.Work.V1.WorkState workStateMessage) =>
        workStateMessage switch 
            {
                Dmg.Work.V1.WorkState.Unknown => DT.Domain.JobWorkState.Unknown,
                Dmg.Work.V1.WorkState.JobInDraft => DT.Domain.JobWorkState.JobInDraft,
                Dmg.Work.V1.WorkState.JobOpen => DT.Domain.JobWorkState.JobOpen,
                Dmg.Work.V1.WorkState.JobPosted => DT.Domain.JobWorkState.JobPosted,

                Dmg.Work.V1.WorkState.JobFirstApplicationReceived => DT.Domain.JobWorkState.JobFirstApplicationReceived,
                Dmg.Work.V1.WorkState.JobPendingReposting => DT.Domain.JobWorkState.JobPendingReposting,
                Dmg.Work.V1.WorkState.JobReposted => DT.Domain.JobWorkState.JobReposted,
                Dmg.Work.V1.WorkState.JobAutoAssigned => DT.Domain.JobWorkState.JobAutoAssigned,
                Dmg.Work.V1.WorkState.JobPendingDirectAssign => DT.Domain.JobWorkState.JobPendingDirectAssign,
                Dmg.Work.V1.WorkState.JobDirectAssigned => DT.Domain.JobWorkState.JobDirectAssigned,

                Dmg.Work.V1.WorkState.JobDetailsModified => DT.Domain.JobWorkState.JobDetailsModified,
                Dmg.Work.V1.WorkState.JobRatesModified => DT.Domain.JobWorkState.JobRatesModified,
                Dmg.Work.V1.WorkState.JobRescheduled => DT.Domain.JobWorkState.JobRescheduled,
                Dmg.Work.V1.WorkState.JobReassigned => DT.Domain.JobWorkState.JobReassigned,

                Dmg.Work.V1.WorkState.JobTechnicianCheckedIn => DT.Domain.JobWorkState.JobTechnicianCheckedIn,
                Dmg.Work.V1.WorkState.JobTechnicianCheckedOut => DT.Domain.JobWorkState.JobTechnicianCheckedOut,

                Dmg.Work.V1.WorkState.JobManuallyCheckedIn => DT.Domain.JobWorkState.JobManuallyCheckedIn,
                Dmg.Work.V1.WorkState.JobManuallyCheckedOut => DT.Domain.JobWorkState.JobManuallyCheckedOut,
                Dmg.Work.V1.WorkState.JobTechnicianReassigned => DT.Domain.JobWorkState.JobTechnicianReassigned,
                Dmg.Work.V1.WorkState.JobCompleted => DT.Domain.JobWorkState.JobCompleted,

                Dmg.Work.V1.WorkState.JobBillingTechnicianSubmitted => DT.Domain.JobWorkState.JobBillingTechnicianSubmitted,
                Dmg.Work.V1.WorkState.JobBillingProviderSubmitted => DT.Domain.JobWorkState.JobBillingProviderSubmitted,
                Dmg.Work.V1.WorkState.JobBillingProviderDraft => DT.Domain.JobWorkState.JobBillingProviderDraft,
                Dmg.Work.V1.WorkState.JobBillingApproved => DT.Domain.JobWorkState.JobBillingApproved,
                Dmg.Work.V1.WorkState.JobBillingDisputed => DT.Domain.JobWorkState.JobBillingDisputed,
                Dmg.Work.V1.WorkState.JobReturnTripNeeded => DT.Domain.JobWorkState.JobReturnTripNeeded,
                Dmg.Work.V1.WorkState.JobNteIncreaseRequested => DT.Domain.JobWorkState.JobNteIncreaseRequested,
                Dmg.Work.V1.WorkState.JobNteModified => DT.Domain.JobWorkState.JobNteModified,
                Dmg.Work.V1.WorkState.JobCancelled => DT.Domain.JobWorkState.JobCancelled,
                Dmg.Work.V1.WorkState.JobInvoiced => DT.Domain.JobWorkState.JobInvoiced,

                Dmg.Work.V1.WorkState.JobInvoiceEarlyPaid => DT.Domain.JobWorkState.JobInvoiceEarlyPaid,
                Dmg.Work.V1.WorkState.JobInvoicePaid => DT.Domain.JobWorkState.JobInvoicePaid,
                Dmg.Work.V1.WorkState.JobBillingCreated => DT.Domain.JobWorkState.JobBillingCreated,
                Dmg.Work.V1.WorkState.JobBillingSentForCorrection => DT.Domain.JobWorkState.JobBillingSentForCorrection,
                Dmg.Work.V1.WorkState.JobBillingDmgModified => DT.Domain.JobWorkState.JobBillingDmgModified,
                Dmg.Work.V1.WorkState.JobTechnicianOnTheWay => DT.Domain.JobWorkState.JobTechnicianOnTheWay,
                Dmg.Work.V1.WorkState.JobTechnicianMissedCheckin => DT.Domain.JobWorkState.JobTechnicianMissedCheckin,
                Dmg.Work.V1.WorkState.JobCompletionPending => DT.Domain.JobWorkState.JobCompletionPending,
                Dmg.Work.V1.WorkState.JobUnsuccessfullyCompleted => DT.Domain.JobWorkState.JobUnsuccessfullyCompleted,     

                Dmg.Work.V1.WorkState.JobProviderNotResponding => DT.Domain.JobWorkState.JobProviderNotResponding,     
                Dmg.Work.V1.WorkState.JobInvoiceCreated => DT.Domain.JobWorkState.JobInvoiceCreated,     
                Dmg.Work.V1.WorkState.JobMarkedNoPay => DT.Domain.JobWorkState.JobMarkedNoPay,     
                Dmg.Work.V1.WorkState.JobPreAssigned => DT.Domain.JobWorkState.JobPreAssigned,     
                Dmg.Work.V1.WorkState.JobPostedAgainstEstimate => DT.Domain.JobWorkState.JobPostedAgainstEstimate,     
                Dmg.Work.V1.WorkState.JobAssignedAgainstEstimate => DT.Domain.JobWorkState.JobAssignedAgainstEstimate,
                Dmg.Work.V1.WorkState.JobBillingPending => DT.Domain.JobWorkState.JobBillingPending,
                _ => DT.Domain.JobWorkState.Unknown
            };

    public static Option<DT.Domain.JobCondition> ToEntityJobCondition(PropertyId propertyId, ServiceTypeId serviceTypeId, Dmg.Work.V1.JobRequirements? messageNullable) =>
        Optional(messageNullable)
            .Map (message => 
                new JobCondition(
                    propertyId,
                    serviceTypeId,
                    message.RequiresCheckIn,
                    message.SameDaySameProperty,
                    MinimumBeforePhotos: Convert.ToUInt32(Math.Abs(Optional(message.MinBeforePhotos).Match(x => x, () => 0))),
                    MaximumBeforePhotos: Convert.ToUInt32(Math.Abs(Optional(message.MaxBeforePhotos).Match(x => x, () => 0))),
                    MinimumAfterPhotos: Convert.ToUInt32(Math.Abs(Optional(message.MinAfterPhotos).Match(x => x, () => 0))),
                    MaximumAfterPhotos: Convert.ToUInt32(Math.Abs(Optional(message.MaxAfterPhotos).Match(x => x, () => 0))),
                    Convert.ToUInt32(Math.Abs(Optional(message.MinimumMinutesForCheckin).Match(x => x, () => 0))),
                    Convert.ToUInt32(Math.Abs(message.AllowedRegularTechnicians)),
                    Convert.ToUInt32(Math.Abs(message.AllowedHelperTechnicians)),
                    Convert.ToUInt32(Math.Abs(Optional(message.MaximumChargeableTrips).Match(x => x, () => 0))),
                    message.UnpayableCatalogItemIds.Map(x => new CatalogItemId(ParseGuidStringDefaultToEmptyGuid(x))).Freeze(),
                    message.AdditionalRules.Freeze()
                        .Filter(x => x != AdditionalRule.Unspecified)
                        .Map(ToEntityJobConditionAdditionalRule)));
    
    /// Map job work message to job entity
    public static DT.Domain.Job ToEntity(Dmg.Work.V1.Work work) 
    {
        var defaultStringValue = DefaultRequiredStringValueIfMissing; 

        return new(new JobWorkId(ParseGuidStringDefaultToEmptyGuid(work.WorkId)),
            work.WorkNumber.DefaultIfNullOrWhiteSpace(defaultStringValue),
            new(ParseGuidStringDefaultToEmptyGuid(work.TicketId)),
            work.TicketNumber.DefaultIfNullOrWhiteSpace(defaultStringValue),
            new(ParseGuidStringDefaultToEmptyGuid(work.CustomerId)),
            new(ParseGuidStringDefaultToEmptyGuid(work.PropertyId)),
            new(Optional(work.JobDetails)
                .Bind(jd => Optional(jd.AssignmentDetails))
                .Map(ad => ad.ProviderId)// Fulfillment used ProviderId to store ProviderOrgId
                .Match(ParseGuidStringDefaultToEmptyGuid,() => Guid.Empty) ),
            ToEntityJobUrgency(work.WorkUrgency),
            ToDateTimeOffsetDefaultToMinimumDate(work.WorkCompletionTimeUtc),
            new(ParseGuidStringDefaultToEmptyGuid(work.ServiceTypeId)),
            new(ParseGuidStringDefaultToEmptyGuid(work.ServiceLineId)),
            work.Description.DefaultIfNullOrWhiteSpace(defaultStringValue), // yes, this is the scope 
            ToEntityJobWorkStatus(work.WorkStatus),
            ToEntityWorkState(work.WorkState),
            NonEmptyText.NewUnsafe(work.WorkState.ToString()),
            // optional scalars
            Optional(work.JobDetails)
                .Bind(jobDetailsMessage => ParseGuidOptionString(jobDetailsMessage.JobBillingId))
                .Map(jobBillingIdGuid => new JobBillingId(jobBillingIdGuid)),
            // required sections
            Optional(work.JobDetails)
                .Bind(jobDetails => Optional(jobDetails.Requirements))
                .Bind(jobRequirements => ToEntityJobCondition(
                    new PropertyId(ParseGuidStringDefaultToEmptyGuid(work.PropertyId)),
                    new ServiceTypeId(ParseGuidStringDefaultToEmptyGuid(work.ServiceTypeId)),
                    jobRequirements)),
            // collections
            Optional(work.JobDetails)
                .Bind(jobDetails => Optional(jobDetails.Rates))
                .Bind(x => x.ToList())
                .Select(CostingMessageMapper.ToEntity)
                .Freeze()); 
    }
}