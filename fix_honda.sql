-- ============================================================
-- HONDA: Fix motorcycles, ATVs, scooters
-- ============================================================

-- Honda Motorcycles (CB, CBR, CRF, CMX, CB etc.)
UPDATE VehicleCatalog SET Category = 'Motorcycle', LengthFeet = 7.0, WidthFeet = 3.0, HeightFeet = 4.2
WHERE Brand = 'HONDA' AND (
    Model LIKE 'CB%' OR Model LIKE 'CBR%' OR Model LIKE 'CRF%' OR Model LIKE 'CMX%'
    OR Model LIKE 'CTX%' OR Model LIKE 'GL%' OR Model LIKE 'NC%' OR Model LIKE 'NM%'
    OR Model LIKE 'PCX%' OR Model LIKE 'SH%' OR Model LIKE 'SHi%' OR Model LIKE 'VFR%'
    OR Model LIKE 'VT%' OR Model LIKE 'XL%' OR Model LIKE 'XRV%' OR Model LIKE 'ADV%'
    OR Model LIKE 'Africa Twin%' OR Model LIKE 'Rebel%' OR Model LIKE 'Shadow%'
    OR Model LIKE 'Goldwing%' OR Model LIKE 'Gold Wing%' OR Model LIKE 'Fury%'
    OR Model LIKE 'Grom%' OR Model LIKE 'Monkey%' OR Model LIKE 'Ruckus%'
    OR Model LIKE 'Navi%' OR Model LIKE 'Metropolitan%' OR Model LIKE 'Elite%'
    OR Model LIKE 'Forza%' OR Model LIKE 'Dio%' OR Model LIKE 'DAX%' OR Model LIKE 'Trail 125%'
    OR Model LIKE 'Transalp%' OR Model LIKE 'Hornet%' OR Model LIKE 'NT%' OR Model LIKE 'NX%'
    OR Model LIKE 'RVF%' OR Model LIKE 'ST%' OR Model LIKE 'Hawk%' OR Model LIKE 'Nighthawk%'
    OR Model IN ('599', '919', 'Big Ruckus', 'A1', 'Valkyrie', 'Rune')
    OR Model LIKE 'Valkyrie%' OR Model LIKE 'Rune%' OR Model LIKE 'Helix%' OR Model LIKE 'Joying%'
    OR Model LIKE 'Magna%' OR Model LIKE 'Sabre%' OR Model LIKE 'Interstate%' OR Model LIKE 'Silverwing%'
    OR Model LIKE 'PS%' OR Model LIKE 'X-ADV%' OR Model LIKE 'Integra%' OR Model LIKE 'Big Red%'
    OR Model LIKE 'Deauville%' OR Model LIKE 'Varadero%' OR Model LIKE 'Crossrunner%'
    OR Model LIKE 'Crosstourer%' OR Model LIKE 'CG%' OR Model LIKE 'XB%'
    OR Model LIKE 'Benly%' OR Model LIKE 'Dream%'
);

-- Honda ATVs / Side by Sides
UPDATE VehicleCatalog SET Category = 'ATV', LengthFeet = 7.5, WidthFeet = 4.5, HeightFeet = 4.5
WHERE Brand = 'HONDA' AND (
    Model LIKE 'TRX%' OR Model LIKE 'ATC%' OR Model LIKE 'FourTrax%' OR Model LIKE 'Pioneer%'
    OR Model LIKE 'Talon%' OR Model LIKE 'Big Red%' OR Model LIKE 'ATC%' OR Model LIKE 'RD%'
    OR Model LIKE 'SXS%'
);

-- Honda cars (Accord, Civic, CR-V etc.) stay as-is or fix
UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 15.0, WidthFeet = 5.9, HeightFeet = 4.7
WHERE Brand = 'HONDA' AND Model IN ('Accord','Civic','Clarity','Insight') AND Category != 'Sedan';

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 15.5, WidthFeet = 6.1, HeightFeet = 5.5
WHERE Brand = 'HONDA' AND (Model LIKE 'CR-V%' OR Model LIKE 'HR-V%' OR Model LIKE 'Pilot%' OR Model LIKE 'Passport%' OR Model LIKE 'Ridgeline%' OR Model = 'Element') AND Category != 'SUV';

UPDATE VehicleCatalog SET Category = 'Hatchback', LengthFeet = 13.8, WidthFeet = 5.8, HeightFeet = 4.9
WHERE Brand = 'HONDA' AND (Model LIKE 'Civic%Hatch%' OR Model IN ('Fit','Jazz','Brio','Amaze') OR Model LIKE 'e%') AND Category NOT IN ('Sedan','SUV','Motorcycle','ATV');
