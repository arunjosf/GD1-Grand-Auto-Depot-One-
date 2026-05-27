-- ============================================================
-- TOYOTA: Precise category fixes
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Truck', LengthFeet = 18.0, WidthFeet = 6.5, HeightFeet = 6.0
WHERE Brand = 'TOYOTA' AND (
    Model LIKE 'Tundra%' OR Model LIKE 'Tacoma%' OR Model LIKE 'Hilux%'
    OR Model LIKE 'T100%' OR Model LIKE 'Pickup%'
);

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 15.5, WidthFeet = 6.2, HeightFeet = 5.5
WHERE Brand = 'TOYOTA' AND (
    Model LIKE '4Runner%' OR Model LIKE 'RAV4%' OR Model LIKE 'Land Cruiser%'
    OR Model LIKE 'Highlander%' OR Model LIKE 'Sequoia%' OR Model LIKE 'FJ%'
    OR Model LIKE 'Fortuner%' OR Model LIKE 'Kluger%' OR Model LIKE 'Prado%'
    OR Model LIKE 'C-HR%' OR Model LIKE 'Venza%' OR Model LIKE 'bZ4X%'
) AND Category != 'Truck';

UPDATE VehicleCatalog SET Category = 'Van', LengthFeet = 16.5, WidthFeet = 6.0, HeightFeet = 6.5
WHERE Brand = 'TOYOTA' AND (
    Model LIKE 'Sienna%' OR Model LIKE 'Previa%' OR Model LIKE 'Hiace%'
    OR Model LIKE 'Innova%' OR Model LIKE 'Alphard%' OR Model LIKE 'Vellfire%'
    OR Model LIKE 'Estima%'
) AND Category NOT IN ('Truck','SUV');

UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.5, WidthFeet = 6.1, HeightFeet = 4.4
WHERE Brand = 'TOYOTA' AND (
    Model LIKE 'Supra%' OR Model LIKE 'GT86%' OR Model LIKE 'GR86%'
    OR Model LIKE '86%' OR Model LIKE 'Celica%' OR Model LIKE 'MR2%'
    OR Model LIKE 'GR Yaris%'
) AND Category NOT IN ('Truck','SUV','Van');

UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 15.0, WidthFeet = 6.0, HeightFeet = 4.7
WHERE Brand = 'TOYOTA' AND (
    Model LIKE 'Camry%' OR Model LIKE 'Corolla%' OR Model LIKE 'Avalon%'
    OR Model LIKE 'Prius%' OR Model LIKE 'Yaris%' OR Model LIKE 'Etios%'
    OR Model LIKE 'Crown%' OR Model LIKE 'Cressida%' OR Model LIKE 'Mark%'
) AND Category NOT IN ('Truck','SUV','Van','Coupe');

-- ============================================================
-- ISUZU: Trucks 
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Truck', LengthFeet = 22.0, WidthFeet = 7.0, HeightFeet = 8.0
WHERE Brand = 'ISUZU' AND (
    Model LIKE 'F-Series%' OR Model LIKE 'N-Series%' OR Model LIKE 'NPR%'
    OR Model LIKE 'NRR%' OR Model LIKE 'FTR%' OR Model LIKE 'FVR%' OR Model LIKE 'FXR%'
    OR Model LIKE 'GMC%' OR Model LIKE 'Elf%' OR Model LIKE 'Forward%'
);

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 15.0, WidthFeet = 6.2, HeightFeet = 5.7
WHERE Brand = 'ISUZU' AND (
    Model LIKE 'Trooper%' OR Model LIKE 'Axiom%' OR Model LIKE 'Rodeo%'
    OR Model LIKE 'Vehicross%' OR Model LIKE 'Amigo%' OR Model LIKE 'MU-X%'
    OR Model LIKE 'D-Max%'
) AND Category != 'Truck';

-- ============================================================
-- AUDI: Proper category fixes
-- ============================================================
UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 15.5, WidthFeet = 6.2, HeightFeet = 5.5
WHERE Brand = 'AUDI' AND (
    Model LIKE 'Q3%' OR Model LIKE 'Q4%' OR Model LIKE 'Q5%'
    OR Model LIKE 'Q7%' OR Model LIKE 'Q8%' OR Model LIKE 'e-tron%' AND Model NOT LIKE '%GT%'
);

UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.8, WidthFeet = 6.2, HeightFeet = 4.5
WHERE Brand = 'AUDI' AND (
    Model LIKE 'TT%' OR Model LIKE 'R8%' OR Model LIKE 'A5%' AND Model LIKE '%Coupe%'
    OR Model LIKE 'RS5%' AND Model LIKE '%Coupe%' OR Model LIKE 'e-tron GT%'
    OR Model LIKE 'RS e-tron GT%'
) AND Category != 'SUV';

UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 15.5, WidthFeet = 6.1, HeightFeet = 4.8
WHERE Brand = 'AUDI' AND (
    Model LIKE 'A3%' OR Model LIKE 'A4%' OR Model LIKE 'A6%'
    OR Model LIKE 'A7%' OR Model LIKE 'A8%' OR Model LIKE 'S3%'
    OR Model LIKE 'S4%' OR Model LIKE 'S6%' OR Model LIKE 'S7%' OR Model LIKE 'S8%'
    OR Model LIKE 'RS3%' OR Model LIKE 'RS4%' OR Model LIKE 'RS6%' OR Model LIKE 'RS7%'
    OR Model LIKE '100%' OR Model LIKE '80%' OR Model LIKE '90%' OR Model LIKE '200%' OR Model LIKE '4000%' OR Model LIKE '5000%'
) AND Category NOT IN ('SUV','Coupe');

UPDATE VehicleCatalog SET Category = 'Hatchback', LengthFeet = 14.0, WidthFeet = 6.0, HeightFeet = 4.9
WHERE Brand = 'AUDI' AND (
    Model LIKE 'A1%' OR Model LIKE 'A2%' OR Model LIKE 'S1%'
) AND Category NOT IN ('SUV','Coupe','Sedan');

-- ============================================================
-- VOLKSWAGEN: Proper fixes
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Hatchback', LengthFeet = 13.9, WidthFeet = 5.9, HeightFeet = 4.9
WHERE Brand = 'VOLKSWAGEN' AND (
    Model LIKE 'Golf%' OR Model LIKE 'GTI%' OR Model LIKE 'Polo%'
    OR Model LIKE 'Up%' OR Model LIKE 'Lupo%' OR Model LIKE 'Fox%'
    OR Model LIKE 'e-Golf%' OR Model LIKE 'GTE%'
);

UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 15.0, WidthFeet = 6.0, HeightFeet = 4.7
WHERE Brand = 'VOLKSWAGEN' AND (
    Model LIKE 'Jetta%' OR Model LIKE 'Passat%' OR Model LIKE 'Phaeton%'
    OR Model LIKE 'Arteon%' OR Model LIKE 'Rabbit%' OR Model LIKE 'Bora%'
    OR Model LIKE 'New Beetle%' OR Model LIKE 'Beetle%' OR Model LIKE 'Scirocco%'
) AND Category != 'Hatchback';

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 15.0, WidthFeet = 6.2, HeightFeet = 5.6
WHERE Brand = 'VOLKSWAGEN' AND (
    Model LIKE 'Tiguan%' OR Model LIKE 'Touareg%' OR Model LIKE 'Atlas%'
    OR Model LIKE 'Taos%' OR Model LIKE 'T-Roc%' OR Model LIKE 'T-Cross%'
    OR Model LIKE 'ID.4%' OR Model LIKE 'ID.6%'
) AND Category NOT IN ('Hatchback','Sedan');

UPDATE VehicleCatalog SET Category = 'Van', LengthFeet = 17.0, WidthFeet = 6.5, HeightFeet = 7.5
WHERE Brand = 'VOLKSWAGEN' AND (
    Model LIKE 'Transporter%' OR Model LIKE 'Caravelle%' OR Model LIKE 'Multivan%'
    OR Model LIKE 'Touran%' OR Model LIKE 'Sharan%' OR Model LIKE 'Routan%'
    OR Model LIKE 'EuroVan%' OR Model LIKE 'Vanagon%' OR Model LIKE 'Bus%'
    OR Model LIKE 'Microbus%' OR Model LIKE 'ID.Buzz%'
) AND Category NOT IN ('Hatchback','Sedan','SUV');

-- ============================================================
-- TESLA: Proper categories
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 15.5, WidthFeet = 6.1, HeightFeet = 4.7
WHERE Brand = 'TESLA' AND Model LIKE 'Model 3%';

UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 16.5, WidthFeet = 6.3, HeightFeet = 4.9
WHERE Brand = 'TESLA' AND Model LIKE 'Model S%';

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 15.5, WidthFeet = 6.2, HeightFeet = 5.7
WHERE Brand = 'TESLA' AND (Model LIKE 'Model X%' OR Model LIKE 'Model Y%');

UPDATE VehicleCatalog SET Category = 'Truck', LengthFeet = 19.0, WidthFeet = 6.8, HeightFeet = 6.3
WHERE Brand = 'TESLA' AND Model LIKE 'Cybertruck%';
