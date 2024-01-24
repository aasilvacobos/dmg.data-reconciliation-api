using DMG.Common;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using Dmg.Work.Billing.V1;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

public static class JobBillingGrossDisputeMessageMapper
{
    public static JobBillingLineItemDisputeRequestReason ToJobBillingLineItemDisputeRequestReason(Dmg.Work.Billing.V1.DisputeReason disputeReasonMessage) =>
        disputeReasonMessage
            switch
            {
                /* Let the front end know if we have to change this */
                Dmg.Work.Billing.V1.DisputeReason.PleaseShareTheReasonWhyThisMuchQuantityWasUsed => JobBillingLineItemDisputeRequestReason.PleaseShareTheReasonWhyThisMuchQuantityWasUsed,
                Dmg.Work.Billing.V1.DisputeReason.WhyWereSoManyHoursUsedToDoTheJob => JobBillingLineItemDisputeRequestReason.WhyWereSoManyHoursUsedToDoTheJob,
                Dmg.Work.Billing.V1.DisputeReason.TheRateIsHighCanYouExplainWhyAndShareProof => JobBillingLineItemDisputeRequestReason.TheRateIsHighCanYouExplainWhyAndShareProof,
                Dmg.Work.Billing.V1.DisputeReason.TheRateIsHighCanYouShareTheProofOfPurchase => JobBillingLineItemDisputeRequestReason.TheRateIsHighCanYouShareTheProofOfPurchase,
                Dmg.Work.Billing.V1.DisputeReason.Other => JobBillingLineItemDisputeRequestReason.Other,
                _ => JobBillingLineItemDisputeRequestReason.Unspecified
            };
    
    public static Option<JobBillingElementDispute> ToEntityOption(Dispute? dispute)
    {
        return Optional(dispute)
            .Filter(message => TryParseGuidString(message.DisputeId).IsSome || message.DisputeConversations.Count > 0)
            .Match(
                d =>
                {
                    Guid jobBillingDisputeIdGuid = ParseGuidStringDefaultToEmptyGuid(d.DisputeId);
                    Lst<JobBillingElementDisputeConversation> jobBillingGrossDisputeConversations = d.DisputeConversations.Freeze().Map(ToJobBillingElementDisputeConversation);
                    var optionJobBillingGrossDispute = Option<JobBillingElementDispute>.Some(new JobBillingElementDispute(new JobBillingDisputeId(jobBillingDisputeIdGuid), jobBillingGrossDisputeConversations));
                    return optionJobBillingGrossDispute;
                },
                () => Option<JobBillingElementDispute>.None);
    }

    private static JobBillingElementDisputeConversation ToJobBillingElementDisputeConversation(DisputeConversation disputeConversation)
    {
        var disputeConversationId = new DisputeConversationId(ParseGuidStringDefaultToEmptyGuid(disputeConversation.DisputeConversationId));
        long order = disputeConversation.Order;
        JobBillingElementDisputeRequest disputeRequest = ToEntityJobBillingElementDisputeRequest(disputeConversation.DisputeRequest);
        var recordMetaData = RecordMetaMessageMapper.ToEntity(disputeConversation.MetaData);
        Option<JobBillingElementDisputeResponse> jobBillingGrossDisputeResponse = Optional(disputeConversation.DisputeResponse).Map(ToEntityJobBillingElementDisputeResponse);

        return new JobBillingElementDisputeConversation(disputeConversationId, order, disputeRequest, recordMetaData, jobBillingGrossDisputeResponse);
    }

    private static JobBillingElementDisputeRequest ToEntityJobBillingElementDisputeRequest(Option<DisputeRequest> disputeRequest)
    {
        return disputeRequest.Match(
            d=>
            {
                var reasonText = NonEmptyText.NewOptionUnvalidated(d.DisputeReason);
                RecordMeta disputeRequestRecordMetaData = RecordMetaMessageMapper.ToEntity(d.MetaData);
                var additionalText = NonEmptyText.NewOptionUnvalidated(d.DisputeAdditionalText);
                return new JobBillingElementDisputeRequest(ToJobBillingLineItemDisputeRequestReason(d.DisputeReasonEnum), disputeRequestRecordMetaData, reasonText, additionalText);
            },
            () =>
            {
                var reasonText = NonEmptyText.NewOptionUnvalidated(DefaultRequiredStringValueIfMissing);
                var disputeRequestRecordMetaData = RecordMetaMessageMapper.BuildRecordMetaEmpty();
                var additionalText = NonEmptyText.NewOptionUnvalidated(DefaultRequiredStringValueIfMissing);
                return new JobBillingElementDisputeRequest(JobBillingLineItemDisputeRequestReason.Other, disputeRequestRecordMetaData, reasonText, additionalText);
            }
        );
    }

    public static JobBillingElementDisputeResponse ToEntityJobBillingElementDisputeResponse(DisputeResponse disputeResponse) =>
        new JobBillingElementDisputeResponse(
            NonEmptyText.NewUnsafe(disputeResponse.DisputeResponseMessage.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)), 
            RecordMetaMessageMapper.ToEntity(disputeResponse.MetaData));
}