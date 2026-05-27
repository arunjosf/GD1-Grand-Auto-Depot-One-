-- ============================================================
-- MINI: Remove non-vehicle entries (trailers, companies)
-- Only keep actual MINI car models
-- ============================================================

-- MINI valid models: Cooper, Cooper S, Clubman, Countryman, Paceman, Roadster, Coupe, Convertible, JCW
UPDATE VehicleCatalog SET Category = 'Hatchback', LengthFeet = 12.5, WidthFeet = 5.6, HeightFeet = 4.8
WHERE Brand = 'MINI' AND (
    Model LIKE '%Cooper%' OR Model LIKE '%Hatch%' OR Model LIKE '%3-door%' OR Model LIKE '%5-door%'
);

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 13.5, WidthFeet = 5.9, HeightFeet = 5.3
WHERE Brand = 'MINI' AND (
    Model LIKE '%Countryman%' OR Model LIKE '%Paceman%' OR Model LIKE '%Aceman%'
) AND Category != 'Hatchback';

UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 12.5, WidthFeet = 5.6, HeightFeet = 4.6
WHERE Brand = 'MINI' AND (
    Model LIKE '%Coupe%' OR Model LIKE '%Roadster%' OR Model LIKE '%Convertible%' OR Model LIKE '%Cabrio%'
) AND Category NOT IN ('Hatchback','SUV');

UPDATE VehicleCatalog SET Category = 'Hatchback', LengthFeet = 13.0, WidthFeet = 5.7, HeightFeet = 5.0
WHERE Brand = 'MINI' AND (
    Model LIKE '%Clubman%'
) AND Category NOT IN ('SUV','Coupe');

-- Entries that don't match any real MINI model = mark as 'Other'
UPDATE VehicleCatalog SET Category = 'Other'
WHERE Brand = 'MINI' AND Category = 'Sedan';

-- ============================================================
-- FREIGHTLINER: All heavy commercial trucks
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Heavy Truck', LengthFeet = 25.0, WidthFeet = 8.0, HeightFeet = 12.0
WHERE Brand = 'FREIGHTLINER';

-- ============================================================
-- TRIUMPH: Motorcycles
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Motorcycle', LengthFeet = 7.2, WidthFeet = 3.0, HeightFeet = 4.2
WHERE Brand = 'TRIUMPH';

-- ============================================================
-- EAGLE: Motorcycles
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Motorcycle', LengthFeet = 7.2, WidthFeet = 3.0, HeightFeet = 4.2
WHERE Brand = 'EAGLE';

-- ============================================================
-- WINNEBAGO: RV / Motorhome 
-- ============================================================
UPDATE VehicleCatalog SET Category = 'RV', LengthFeet = 35.0, WidthFeet = 8.5, HeightFeet = 12.0
WHERE Brand = 'WINNEBAGO';

-- ============================================================
-- BLUE BIRD: School Bus
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Bus', LengthFeet = 40.0, WidthFeet = 8.0, HeightFeet = 10.0
WHERE Brand = 'BLUE BIRD';

-- ============================================================
-- ORION BUS: Bus
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Bus', LengthFeet = 40.0, WidthFeet = 8.0, HeightFeet = 10.0
WHERE Brand = 'ORION BUS';

-- ============================================================
-- RAM: Trucks and commercial vans
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Truck', LengthFeet = 19.5, WidthFeet = 6.7, HeightFeet = 6.5
WHERE Brand = 'RAM' AND (
    Model LIKE '1500%' OR Model LIKE '2500%' OR Model LIKE '3500%'
    OR Model LIKE '4500%' OR Model LIKE '5500%' OR Model LIKE 'ProMaster City%'
    OR Model LIKE 'Pickup%'
) AND Category != 'Van';

UPDATE VehicleCatalog SET Category = 'Van', LengthFeet = 19.0, WidthFeet = 6.5, HeightFeet = 8.0
WHERE Brand = 'RAM' AND (
    Model LIKE 'ProMaster%' OR Model LIKE 'Van%'
) AND Category != 'Truck';

-- ============================================================
-- FERRARI: Coupe/Sports Cars
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.5, WidthFeet = 6.3, HeightFeet = 4.3
WHERE Brand = 'FERRARI';

-- ============================================================
-- LAMBORGHINI: Coupe/Sports Cars
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.5, WidthFeet = 6.5, HeightFeet = 4.3
WHERE Brand = 'LAMBORGHINI';

-- ============================================================
-- McLAREN: Coupe/Sports Cars
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.0, WidthFeet = 6.3, HeightFeet = 4.2
WHERE Brand = 'MCLAREN';

-- ============================================================
-- BUGATTI: Coupe/Hypercars
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.0, WidthFeet = 6.6, HeightFeet = 4.3
WHERE Brand = 'BUGATTI';

-- ============================================================
-- KOENIGSEGG, PAGANI, RIMAC: Coupe
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.0, WidthFeet = 6.4, HeightFeet = 4.2
WHERE Brand IN ('KOENIGSEGG','PAGANI','RIMAC','SSC NORTH AMERICA');

-- ============================================================
-- ROLLS-ROYCE: Sedan / Coupe
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 17.5, WidthFeet = 6.5, HeightFeet = 5.0
WHERE Brand = 'ROLLS-ROYCE' AND (
    Model LIKE 'Phantom%' OR Model LIKE 'Ghost%' OR Model LIKE 'Silver%'
);

UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 17.0, WidthFeet = 6.5, HeightFeet = 4.8
WHERE Brand = 'ROLLS-ROYCE' AND (
    Model LIKE 'Wraith%' OR Model LIKE 'Dawn%' OR Model LIKE 'Spectre%'
) AND Category != 'Sedan';

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 17.5, WidthFeet = 6.8, HeightFeet = 6.2
WHERE Brand = 'ROLLS-ROYCE' AND (
    Model LIKE 'Cullinan%'
) AND Category NOT IN ('Sedan','Coupe');

-- ============================================================
-- BENTLEY: Sedan / Coupe / SUV
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 17.5, WidthFeet = 6.5, HeightFeet = 5.0
WHERE Brand = 'BENTLEY' AND (
    Model LIKE 'Flying Spur%' OR Model LIKE 'Mulsanne%' OR Model LIKE 'Arnage%' OR Model LIKE 'Azure%'
);

UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 16.5, WidthFeet = 6.4, HeightFeet = 4.7
WHERE Brand = 'BENTLEY' AND (
    Model LIKE 'Continental%' OR Model LIKE 'Brooklands%'
) AND Category != 'Sedan';

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 17.0, WidthFeet = 6.7, HeightFeet = 6.0
WHERE Brand = 'BENTLEY' AND Model LIKE 'Bentayga%' AND Category NOT IN ('Sedan','Coupe');

-- ============================================================
-- PORSCHE: Precise corrections
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.5, WidthFeet = 6.0, HeightFeet = 4.5
WHERE Brand = 'PORSCHE' AND (
    Model LIKE '911%' OR Model LIKE '718%' OR Model LIKE '944%' OR Model LIKE '968%'
    OR Model LIKE '928%' OR Model LIKE '914%' OR Model LIKE 'Cayman%' OR Model LIKE 'Boxster%'
);

UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 16.0, WidthFeet = 6.3, HeightFeet = 4.8
WHERE Brand = 'PORSCHE' AND (
    Model LIKE 'Panamera%'
) AND Category != 'Coupe';

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 15.8, WidthFeet = 6.4, HeightFeet = 5.5
WHERE Brand = 'PORSCHE' AND (
    Model LIKE 'Cayenne%' OR Model LIKE 'Macan%'
) AND Category NOT IN ('Coupe','Sedan');
