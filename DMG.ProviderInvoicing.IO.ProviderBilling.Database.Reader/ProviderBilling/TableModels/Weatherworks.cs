using LanguageExt;
using Npgsql;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using System.Data;
using System.Text;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;

public record WeatherWorks(
                    Guid ProviderBillingId,
                    Guid RowId,
                    DateTime? EventStart,
                    DateTime? EventEnd,
                    decimal Snow,
                    decimal? TemperatureInFahrenheit,
                    string? Description,
                    decimal Ice)
{
    internal static string Sql { get; } = @"SELECT provider_billing_id, row_id, event_start, event_end, snow, temperature_in_fahrenheit, description, ice
	FROM provider_billing.weatherworks 
    where provider_billing_id = @id;";

    internal static async Task<Lst<WeatherWorks>> ReadAsync(NpgsqlDataReader reader)
    {
        List<WeatherWorks> items = new List<WeatherWorks>();

        while (await reader.ReadAsync())
        {
            items.Add(new TableModels.WeatherWorks(
                reader.GetGuid("provider_billing_id"),
                reader.GetGuid("row_id"),
                reader.SafeGetDateTime("event_start"),
                reader.SafeGetDateTime("event_end"),
                reader.GetDecimal("snow"),
                reader.SafeGetDecimal("temperature_in_fahrenheit"),
                reader.SafeGetString("description"),
                reader.GetDecimal("ice")));
        }

        return items.Freeze();
    }
}
