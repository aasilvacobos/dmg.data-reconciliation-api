using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record PreProviderBilling(
                    Guid ProviderBillingId,
                    bool IsCIWO,
                    string BillingType,
                    string BillingSubType)
{
    internal static string Sql { get; } = @"select provider_billing_id, is_ciwo,billing_type,billing_subtype from provider_billing.pre_provider_billing where provider_billing_id = @id;";

    internal static async Task<Option<PreProviderBilling>> ReadAsync(NpgsqlDataReader reader)
    {
        while (await reader.ReadAsync())
        {
             return (new TableModels.PreProviderBilling(
                reader.GetGuid("provider_billing_id"),
                reader.GetBoolean("is_ciwo"),
                reader.GetString("billing_type"),
                reader.GetString("billing_subtype")));
        }
        return Option<PreProviderBilling>.None;
    }
}
