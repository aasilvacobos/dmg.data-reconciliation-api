namespace DMG.ProviderInvoicing.DT.Domain.Map;

public static class ProviderBillingMapper
{
    public static JobBillingId ToJobBillingId(ProviderBillingId providerBillingId) =>
        new JobBillingId(providerBillingId.Value);
}