using System;

namespace DMG.ProviderInvoicing.DT.Domain; 

/// Unit of measure
public enum UnitOfMeasure 
{
    Unspecified,
    Item,
    Case,
    Gallon,
    Liter,
    Pound,
    Kilo,
    Ton,
    Minute,
    Hour,
    Trip,
    Day,
    Week,
    Yd3,
    Yd2,
    Mile,
    Meter,
    Job,
    Foot,
    Inch,
    Event,
    Truck,
    Bag,
    Service,
    Second
}

/// Types of material/part/equipment catalog items. Subset of CatalogItemType.
public enum MaterialPartEquipmentCatalogItemType 
{
    Material,
    Part,
    EquipmentOwned,
    EquipmentRental
}

/// Types of material/part catalog items. Subset of CatalogItemType.
public enum MaterialPartCatalogItemType 
{
    Material,
    Part
}

/// Types of equipment catalog items. Subset of CatalogItemType.
public enum EquipmentCatalogItemType 
{
    EquipmentOwned,
    EquipmentRental
}

// Type of technician
public enum TechnicianType 
{
    TechnicianTypeRegular,
    TechnicianTypeHelper
}

// Rate type
public enum RateType
{
    Regular,
    Emergency,
    Holiday
}