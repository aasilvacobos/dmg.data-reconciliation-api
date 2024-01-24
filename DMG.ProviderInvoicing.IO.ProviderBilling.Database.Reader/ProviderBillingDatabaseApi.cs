// Ignore Spelling: Api

using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Domain.Map;
using DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling;
using LanguageExt;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader;

public class ProviderBillingDatabaseApi
{
    private readonly ProviderBillingOperation _op;

    public ProviderBillingDatabaseApi(string connectionString)
    {
        _op = new ProviderBillingOperation(connectionString);
    }
    public async Task<Either<ErrorMessage, DT.Domain.ProviderBilling>> RetrieveByIdAsync(ProviderBillingId providerBillingId) =>
        await _op.RetrieveByIdAsync(providerBillingId);

    public async Task<bool> ContainsAsync(ProviderBillingId providerBillingId) =>
       await _op.ContainsAsync(providerBillingId);

    public async Task<bool> ExistForJobAsync(JobWorkId jobWorkId) =>
        await _op.ExistForJobAsync(jobWorkId);

    public async Task<Either<ErrorMessage, Lst<DT.Domain.JobPhoto>>> GetProviderBillingPhotosByProviderBillingIdAsync(ProviderBillingId providerBillingId) =>
         await _op.GetPhotosByProviderBillingIdAsync(providerBillingId);

    public async Task<Either<ErrorMessage, Lst<ProviderBillingId>>> GetPendingInvoicesForProviderId(ProviderOrgId providerOrgId) =>
        await _op.GetPendingInvoicesForProviderId(providerOrgId);
}
