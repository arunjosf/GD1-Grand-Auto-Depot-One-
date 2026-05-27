-- ============================================================
-- CHEVROLET: Fix trucks and vans
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Truck', LengthFeet = 19.5, WidthFeet = 6.7, HeightFeet = 6.5
WHERE Brand = 'CHEVROLET' AND (
    Model LIKE 'Silverado%' OR Model LIKE 'Colorado%' OR Model LIKE 'S-10%' OR Model LIKE 'S10%'
    OR Model LIKE 'C/K%' OR Model LIKE 'CK%' OR Model LIKE 'C10%' OR Model LIKE 'C20%' OR Model LIKE 'C30%'
    OR Model LIKE 'K10%' OR Model LIKE 'K20%' OR Model LIKE 'K30%'
    OR Model LIKE 'Kodiak%' OR Model LIKE 'Low Cab%' OR Model LIKE 'Medium%' OR Model LIKE 'Titan%'
    OR Model LIKE 'T-Series%' OR Model LIKE 'W3%' OR Model LIKE 'W4%' OR Model LIKE 'W5%'
    OR Model LIKE 'C3%' OR Model LIKE 'C4%' OR Model LIKE 'C5%' OR Model LIKE 'C6%' OR Model LIKE 'C7%' OR Model LIKE 'C8%'
    OR Model LIKE 'Pickup%' OR Model LIKE 'Apache%' OR Model LIKE 'El Camino%'
);

UPDATE VehicleCatalog SET Category = 'Van', LengthFeet = 18.5, WidthFeet = 6.5, HeightFeet = 7.0
WHERE Brand = 'CHEVROLET' AND (
    Model LIKE 'Express%' OR Model LIKE 'Astro%' OR Model LIKE 'Beauville%'
    OR Model LIKE 'Chevy Van%' OR Model LIKE 'Sportvan%' OR Model LIKE 'G-Series%'
    OR Model LIKE 'G10%' OR Model LIKE 'G20%' OR Model LIKE 'G30%'
) AND Category != 'Truck';

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 16.5, WidthFeet = 6.5, HeightFeet = 6.0
WHERE Brand = 'CHEVROLET' AND (
    Model LIKE 'Suburban%' OR Model LIKE 'Tahoe%' OR Model LIKE 'Equinox%'
    OR Model LIKE 'Traverse%' OR Model LIKE 'Blazer%' OR Model LIKE 'Trailblazer%'
    OR Model LIKE 'Trax%' OR Model LIKE 'Captiva%' OR Model LIKE 'Tracker%'
    OR Model LIKE 'Blazer EV%'
) AND Category NOT IN ('Truck','Van');

UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 15.5, WidthFeet = 6.0, HeightFeet = 4.7
WHERE Brand = 'CHEVROLET' AND (
    Model LIKE 'Malibu%' OR Model LIKE 'Impala%' OR Model LIKE 'Cruze%'
    OR Model LIKE 'Sonic%' OR Model LIKE 'Cavalier%' OR Model LIKE 'Cobalt%'
    OR Model LIKE 'Spark%' OR Model LIKE 'Aveo%' OR Model LIKE 'Caprice%'
    OR Model LIKE 'Volt%' OR Model LIKE 'Bolt%' OR Model LIKE 'Monte Carlo%'
    OR Model LIKE 'Lumina%' OR Model LIKE 'Celebrity%' OR Model LIKE 'Beretta%'
    OR Model LIKE 'Citation%' OR Model LIKE 'Nova%'
) AND Category NOT IN ('Truck','Van','SUV');

UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.5, WidthFeet = 6.2, HeightFeet = 4.2
WHERE Brand = 'CHEVROLET' AND (
    Model LIKE 'Corvette%' OR Model LIKE 'Camaro%' OR Model LIKE 'Cruze%' AND Model LIKE '%Coupe%'
) AND Category NOT IN ('Truck','Van','SUV','Sedan');

-- ============================================================
-- MERCEDES-BENZ: Fix trucks and sprinters
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Van', LengthFeet = 19.0, WidthFeet = 6.5, HeightFeet = 8.0
WHERE Brand = 'MERCEDES-BENZ' AND (
    Model LIKE 'Sprinter%' OR Model LIKE 'Vito%' OR Model LIKE 'Viano%'
    OR Model LIKE 'Metris%' OR Model LIKE 'MB100%' OR Model LIKE 'V-Class%'
    OR Model LIKE 'Citan%' OR Model LIKE 'eSprinter%'
);

UPDATE VehicleCatalog SET Category = 'Truck', LengthFeet = 20.0, WidthFeet = 7.0, HeightFeet = 8.5
WHERE Brand = 'MERCEDES-BENZ' AND (
    Model LIKE 'Actros%' OR Model LIKE 'Arocs%' OR Model LIKE 'Atego%'
    OR Model LIKE 'Axor%' OR Model LIKE 'Unimog%' OR Model LIKE 'Zetros%'
    OR Model LIKE 'Antos%' OR Model LIKE 'G55%' AND Model LIKE '%AMG%'
    OR Model LIKE 'X-Class%'
) AND Category != 'Van';

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 16.0, WidthFeet = 6.3, HeightFeet = 5.8
WHERE Brand = 'MERCEDES-BENZ' AND (
    Model LIKE 'G-Class%' OR Model LIKE 'GLE%' OR Model LIKE 'GLC%' OR Model LIKE 'GLA%'
    OR Model LIKE 'GLB%' OR Model LIKE 'GLS%' OR Model LIKE 'ML%' OR Model LIKE 'GL %'
    OR Model LIKE 'EQB%' OR Model LIKE 'EQC%'
) AND Category NOT IN ('Truck','Van');

UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 16.0, WidthFeet = 6.1, HeightFeet = 4.9
WHERE Brand = 'MERCEDES-BENZ' AND (
    Model LIKE 'C-Class%' OR Model LIKE 'E-Class%' OR Model LIKE 'S-Class%'
    OR Model LIKE 'A-Class%' OR Model LIKE 'B-Class%' OR Model LIKE 'CLA%'
    OR Model LIKE 'EQE%' OR Model LIKE 'EQS%' AND Model NOT LIKE '%SUV%'
    OR Model LIKE '190%' OR Model LIKE '220%' OR Model LIKE '230%'
    OR Model LIKE '240%' OR Model LIKE '260%' OR Model LIKE '280%'
    OR Model LIKE '300%' OR Model LIKE '320%' OR Model LIKE '350%'
    OR Model LIKE '380%' OR Model LIKE '420%' OR Model LIKE '450%'
    OR Model LIKE '500%' OR Model LIKE '560%' OR Model LIKE '600%'
) AND Category NOT IN ('Truck','Van','SUV');

UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 15.0, WidthFeet = 6.1, HeightFeet = 4.7
WHERE Brand = 'MERCEDES-BENZ' AND (
    Model LIKE 'C-Class Coupe%' OR Model LIKE 'E-Class Coupe%' OR Model LIKE 'S-Class Coupe%'
    OR Model LIKE 'CLS%' OR Model LIKE 'SL%' OR Model LIKE 'SLC%' OR Model LIKE 'SLK%'
    OR Model LIKE 'CLK%' OR Model LIKE 'AMG GT%' OR Model LIKE 'SLS%'
) AND Category NOT IN ('Truck','Van','SUV','Sedan');
