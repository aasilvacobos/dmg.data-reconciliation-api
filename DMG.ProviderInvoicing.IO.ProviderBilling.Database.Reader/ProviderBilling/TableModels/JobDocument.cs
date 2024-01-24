using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record JobDocument(
                    Guid DocumentId,
                    Guid ProviderBillingId,
                    string MimeType,
                    string? FileName,
                    string? Description,
                    string AttachmentType)
{
    internal static string Sql { get; } = @"SELECT document_id, provider_billing_id, mime_type, file_name, description, attachment_type
	FROM provider_billing.job_document 
    where provider_billing_id = @id;";

    internal static async Task<Lst<JobDocument>> ReadAsync(NpgsqlDataReader reader)
    {
        List<JobDocument> items = new List<JobDocument>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.JobDocument(
                reader.GetGuid("document_id"),
                reader.GetGuid("provider_billing_id"),
                reader.GetString("mime_type"),
                reader.SafeGetString("file_name"),
                reader.SafeGetString("description"),
                reader.GetString("attachment_type")
                ));
        }

        return items.Freeze();
    }
}
