using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record Photo(
                    Guid PhotoId,
                    Guid ProviderBillingId,
                    string? PhotoChronology,
                    string MimeType,
                    string? FileName,
                    string? Description,
                    Guid EntityId,
                    string EntityType)
{
    internal static string Sql { get; } = @"SELECT photo_id, p.provider_billing_id, photo_chronology, mime_type, file_name, description, e.entity_reference_id, e.entity_reference_type
	FROM provider_billing.photo p
    left join provider_billing.entity_relation e on p.photo_id = e.entity_id
    where p.provider_billing_id = @id;";

    internal static async Task<Lst<Photo>> ReadAsync(NpgsqlDataReader reader)
    {
        List<Photo> items = new List<Photo>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.Photo(
                reader.GetGuid("photo_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetString("photo_chronology"),
                reader.GetString("mime_type"),
                reader.SafeGetString("file_name"),
                reader.SafeGetString("description"),
                reader.SafeGetGuid("entity_reference_id") ?? reader.GetGuid("provider_billing_id"),
                reader.SafeGetString("entity_reference_type") ?? "PROVIDER_BILLING"
                ));
        }

        return items.Freeze();
    }
}
