using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.IO.SorConcentrator.Common;
using LanguageExt;
using static DMG.ProviderInvoicing.IO.SorConcentrator.Common.MemoryCacheStatistics;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.IO.SorConcentrator;

/// API consumed by other I/O adapters to retrieve an entity from the SOR Concentrator.
public static class SorConcentratorApi
{
    public static void TestTopicConnections() =>
        SorConcentratorClient.TestConnections();
    
    public static Task<Either<ErrorMessage, CatalogItem>> GetByIdAsync(CatalogItemId catalogItemId) =>
        SystemOfRecordCatalogItem.GetByIdAsync(catalogItemId);

    public static Task<Lst<Either<ErrorMessage, CatalogItem>>> GetByIdsAsync(Lst<CatalogItemId> catalogItemIds) =>
        SystemOfRecordCatalogItem.GetByIdsAsync(catalogItemIds);
    
    public static Task<Either<ErrorMessage, Customer>> GetByIdAsync(CustomerId customerId) =>
        SystemOfRecordCustomer.GetByIdAsync(customerId);

    public static Task<Either<ErrorMessage, Job>> GetByIdAsync(JobWorkId jobWorkId) =>
        SystemOfRecordJob.GetJobByIdAsync(jobWorkId);

    public static Either<ErrorMessage, Job> GetById(JobWorkId jobWorkId) =>
        SystemOfRecordJob.GetJobById(jobWorkId);

    public static Task<Either<ErrorMessage, JobBillingGross>> GetByIdAsync(JobBillingId jobBillingId) =>
        SystemOfRecordJobBilling.GetByIdAsync(jobBillingId);

    public static Task<Either<ErrorMessage, JobBillingDecorated>> GetJobBillingDecoratedByIdAsync(JobBillingId jobBillingId) =>
        SystemOfRecordJobBilling.GetJobBillingDecoratedByIdAsync(jobBillingId);
    
    public static Task<Either<ErrorMessage, Property>> GetByIdAsync(PropertyId propertyId) =>
        SystemOfRecordProperty.GetByIdAsync(propertyId);

    public static Task<Either<ErrorMessage, ProviderOrg>> GetByIdAsync(ProviderOrgId providerOrgId) =>
        SystemOfRecordProviderOrg.GetByIdAsync(providerOrgId);
    
    public static Task<Either<ErrorMessage, ServiceLine>> GetByIdAsync(ServiceLineId serviceLineId) =>
        SystemOfRecordServiceLine.GetByIdAsync(serviceLineId);

    public static Task<Either<ErrorMessage, ServiceType>> GetByIdAsync(ServiceTypeId serviceTypeId) =>
        SystemOfRecordServiceType.GetByIdAsync(serviceTypeId);

    public static Task<Either<ErrorMessage, Ticket>> GetByIdAsync(TicketId ticketId) =>
        SystemOfRecordTicket.GetByIdAsync(ticketId);

    public static Task<Either<ErrorMessage, DMG.TicketBilling.TicketBilling>> GetByIdAsync(TicketBillingId ticketId) =>
        SystemOfRecordTicketBilling.GetByIdAsync(ticketId);

    public static Task<Either<ErrorMessage, User>> GetByIdAsync(UserId userId) =>
        SystemOfRecordUser.GetByIdAsync(userId);
    public static Task<Either<ErrorMessage, DMG.Proto.ProviderAgreements.V2.Agreement>> GetByIdAsync(PsaId psaId) =>
        SystemOfRecordPsa.GetByIdAsync(psaId);
    
    public static Task<Either<ErrorMessage, DMG.Proto.CustomerContracts.ContractTermsSheet>> GetByIdAsync(ContractTermSheetId contractTermSheetId) =>
        SystemOfRecordCustomerTermSheet.GetByIdAsync(contractTermSheetId);

    public static Lst<MemoryCacheCounts> GetAllMemoryCacheCounts() =>
        MemoryCache.GetAllCounts();
}