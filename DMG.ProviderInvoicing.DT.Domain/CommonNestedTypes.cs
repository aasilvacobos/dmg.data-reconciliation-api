using System;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain;

/// Money type    
public record Currency (
    string CurrencyCode, // 3 characters
    // total amount, shifted by three decimal places:
    // 123456 => 123.456
    // 2123456000 => 2123456.000    
    long Amount);

/// User history data for a record    
public record RecordMeta(
    Option<UserId>          CreatedByUserId, 
    Option<DateTimeOffset>  CreatedOnDateTime,
    Option<UserId>          ModifiedByUserId,
    Option<DateTimeOffset>  ModifiedOnDateTime);