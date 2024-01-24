using DMG.ProviderInvoicing.DT.Domain.Rule;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Domain;

///  Urgency level of a job
public enum JobUrgency 
{
    Unknown,
    Emergency,
    High,
    Normal
}

/// Work status of a job 
public enum JobWorkStatus 
{
    Unknown,
    Draft,
    Open,
    InProgress,
    Completed,
    Verified,
    Closed,
    Cancelled,
    NoPay,
    OnHold
}

/// Work state         
public enum JobWorkState 
{
    Unknown,
    JobInDraft,
    JobOpen,
    JobPosted,
    JobFirstApplicationReceived,
    JobPendingReposting,
    JobReposted,
    JobAutoAssigned,
    JobPendingDirectAssign,
    JobDirectAssigned,
    JobDetailsModified,
    JobRatesModified,
    JobRescheduled,
    JobReassigned,
    JobTechnicianCheckedIn,
    JobTechnicianCheckedOut,
    JobManuallyCheckedIn,
    JobManuallyCheckedOut,
    JobTechnicianReassigned,
    JobCompleted,
    JobBillingTechnicianSubmitted,
    JobBillingProviderSubmitted,
    JobBillingProviderDraft,
    JobBillingApproved,
    JobBillingDisputed,
    JobReturnTripNeeded,
    JobNteIncreaseRequested,
    JobNteModified,
    JobCancelled,
    JobInvoiced,
    JobInvoiceEarlyPaid,
    JobInvoicePaid,
    JobBillingCreated,
    JobBillingSentForCorrection,
    JobBillingDmgModified,
    JobTechnicianOnTheWay,
    JobTechnicianMissedCheckin,
    JobCompletionPending,
    JobUnsuccessfullyCompleted,
    JobProviderNotResponding,
    JobInvoiceCreated,
    JobMarkedNoPay,
    JobPreAssigned,
    JobPostedAgainstEstimate, 
    JobAssignedAgainstEstimate,
    JobBillingPending
}

/// Transactional Data **AGGREGATE**: The work job entity.
public record Job(
    JobWorkId                                   JobWorkId,  // work id
    string                                      JobWorkNumber,
    TicketId                                    TicketId,
    string                                      TicketNumber,
    CustomerId                                  CustomerId,
    PropertyId                                  PropertyId,
    ProviderOrgId                               ProviderOrgId,  
    JobUrgency                                  Urgency,
    DateTimeOffset                              JobCompleteDate,
    ServiceTypeId                               ServiceTypeId,
    ServiceLineId                               ServiceLineId,
    string                                      Scope,
    JobWorkStatus                               JobWorkStatus,
    JobWorkState                                JobWorkState,
    NonEmptyText                                JobWorkStateText,
    // optional scalars
    Option<JobBillingId>                        JobBillingId,
    // required sections
    // optional sections
    Option<JobCondition>                        JobCondition,
    // collections
    Lst<Costing>                                Costings)
{
    // Computed fields
    public bool IsProviderNotResponding => JobRule.IsProviderNotResponding(this);
}