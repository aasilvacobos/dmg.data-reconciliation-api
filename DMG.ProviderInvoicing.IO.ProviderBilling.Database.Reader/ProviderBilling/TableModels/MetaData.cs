using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record MetaData(
                    Guid ProviderBillingId,
                    Guid MetaDataId,
                    Guid EntityId,
                    string EntityType,
                    Guid CreatedByUserId,
                    DateTime CreatedOnDateTime,
                    Guid ModifiedByUserId,
                    DateTime ModifiedOnDateTime)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, meta_data_id, entity_id, entity_type, created_by_user_id, created_on_date_time, modified_by_user_id, modified_on_date_time
	FROM provider_billing.meta_data 
    where provider_billing_id = @id;";

    internal static async Task<Lst<MetaData>> ReadAsync(NpgsqlDataReader reader)
    {
        List<MetaData> items = new List<MetaData>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.MetaData(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("meta_data_id"),
                reader.GetGuid("entity_id"),
                reader.GetString("entity_type"),
                reader.GetGuid("created_by_user_id"),
                reader.GetDateTime("created_on_date_time"),
                reader.GetGuid("modified_by_user_id"),
                reader.GetDateTime("modified_on_date_time")));
        }

        return items.Freeze();
    }
}
