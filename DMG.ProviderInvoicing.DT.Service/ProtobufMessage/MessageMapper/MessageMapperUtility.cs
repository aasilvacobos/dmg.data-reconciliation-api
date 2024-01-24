using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;
using Google.Protobuf.WellKnownTypes;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper; 

/// Utility methods for mapping protobuf messages and domain entities. 
public static class MessageMapperUtility 
{
    /// <summary>
    /// If a message string property that is considered required contains a null, empty string, or white space,
    /// then it should be replaced with this value. This scenario should never happen. But when it does, we don't want
    /// a null, empty string, or white space value sneaking in. This makes it obvious to whomever is consuming the data. 
    /// </summary>
    public const string DefaultRequiredStringValueIfMissing = @"DATA_NOT_FOUND";

    /// <summary>
    /// If a message Timestamp property that is considered required is null,
    /// then it should be replaced with this value. This scenario should never happen.
    /// But when it does, we need some value to make it obvious to whomever is consuming the data. 
    /// </summary>
    public static readonly DateTimeOffset DefaultRequiredDateTimeOffsetValueIfMissing = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan(0));

    /// Parse string to Guid if string contains valid Guid. Otherwise, return an empty Guid. 
    public static Guid ParseGuidStringDefaultToEmptyGuid(string guidString) 
    {
        var rtn = Guid.TryParse(guidString, out var guidOut);
        return rtn ? guidOut : Guid.Empty;
    }

    /// Try to parse a string to a required (non-empty) Guid. If string contains an invalid or empty Guid, then None is returned. 
    public static Option<Guid> TryParseGuidString(string guidString) 
    {
        var rtn = Guid.TryParse(guidString, out var guidOut);
        return rtn && guidOut != Guid.Empty ? Some(guidOut) : None;
    }
    
    /// Parse a string to an optional Guid. If string contains an invalid or empty Guid, then None is returned.
    public static Option<Guid> ParseGuidOptionString(string guidString) 
    {
        var rtn = Guid.TryParse(guidString, out var guidOut);
        return rtn && guidOut != Guid.Empty ? Some(guidOut) : None;
    }
    
    /// Try to convert Protobuf Timestamp to DateTimeOffset. Otherwise, return None.
    public static Option<DateTimeOffset> TryToDateTimeOffset(Timestamp timestamp) =>
        timestamp == default! ? None : Some(timestamp.ToDateTimeOffset());

    /// Try to convert Timestamp to DateTimeOffset. Otherwise, set to minimum date.
    public static DateTimeOffset ToDateTimeOffsetDefaultToMinimumDate(Timestamp timestamp) =>
        timestamp == default! ? DefaultRequiredDateTimeOffsetValueIfMissing : timestamp.ToDateTimeOffset();
    
    /// Try to convert Timestamp to DateOnly. Otherwise, return None
    public static Option<DateOnly> TryToDateOnly(Timestamp timestamp) =>
        timestamp == default! ? None : Some(DateOnly.FromDateTime(timestamp.ToDateTime()));
    
    /// Convert protobuf message MapField of string/string KV to immutable Map of string/OptionNonEmptyText>
    public static Map<string, Option<NonEmptyText>> ToMapImmutable(Google.Protobuf.Collections.MapField<string, string> mapMessage) =>
        mapMessage
            .ToMap()
            // remove entries with empty keys to be safe
            .Filter((k, _) => !string.IsNullOrWhiteSpace(k))
            .Map<(string, string), (string, Option<NonEmptyText>)>(keyValuePair => (
                keyValuePair.Item1,   
                NonEmptyText.NewOptionUnvalidated(keyValuePair.Item2)))
            .ToMap();
}