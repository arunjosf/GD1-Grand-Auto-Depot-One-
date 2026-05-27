-- ============================================================
-- SUZUKI: Fix motorcycles (huge catalog: GS, GSX, DR, SV, Boulevard, V-Strom, etc.)
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Motorcycle', LengthFeet = 7.0, WidthFeet = 3.0, HeightFeet = 4.2
WHERE Brand = 'SUZUKI' AND (
    Model LIKE 'GS%' OR Model LIKE 'GSX%' OR Model LIKE 'GSF%' OR Model LIKE 'GSR%'
    OR Model LIKE 'DR%' OR Model LIKE 'SV%' OR Model LIKE 'TL%' OR Model LIKE 'DL%'
    OR Model LIKE 'Boulevard%' OR Model LIKE 'V-Strom%' OR Model LIKE 'Hayabusa%'
    OR Model LIKE 'Intruder%' OR Model LIKE 'Bandit%' OR Model LIKE 'Gladius%'
    OR Model LIKE 'Katana%' OR Model LIKE 'Inazuma%' OR Model LIKE 'RG%'
    OR Model LIKE 'RM%' OR Model LIKE 'VL%' OR Model LIKE 'VZ%' OR Model LIKE 'AN%'
    OR Model LIKE 'Burgman%' OR Model LIKE 'Address%' OR Model LIKE 'Avenis%'
    OR Model LIKE 'Access%' OR Model LIKE 'Let''s%' OR Model LIKE 'Skywave%'
    OR Model LIKE 'Sixteen%' OR Model LIKE 'Swish%' OR Model LIKE 'Avenis%'
    OR Model LIKE 'EN%' OR Model LIKE 'GN%' OR Model LIKE 'GT%'
    OR Model LIKE 'Raider%' OR Model LIKE 'Gixxer%' OR Model LIKE 'Slingshot%'
);

-- Suzuki ATVs
UPDATE VehicleCatalog SET Category = 'ATV', LengthFeet = 7.5, WidthFeet = 4.5, HeightFeet = 4.5
WHERE Brand = 'SUZUKI' AND (
    Model LIKE 'LT%' OR Model LIKE 'QuadRunner%' OR Model LIKE 'King Quad%'
    OR Model LIKE 'Ozark%' OR Model LIKE 'Eiger%' OR Model LIKE 'Vinson%'
    OR Model LIKE 'Burgman%' AND Category = 'ATV'
);

-- Suzuki cars
UPDATE VehicleCatalog SET Category = 'Hatchback', LengthFeet = 12.5, WidthFeet = 5.5, HeightFeet = 5.0
WHERE Brand = 'SUZUKI' AND (
    Model LIKE 'Swift%' OR Model LIKE 'Alto%' OR Model LIKE 'Celerio%'
    OR Model LIKE 'WagonR%' OR Model LIKE 'Ignis%' OR Model LIKE 'Baleno%'
    OR Model LIKE 'Splash%' OR Model LIKE 'SX4%'
) AND Category NOT IN ('Motorcycle','ATV');

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 14.5, WidthFeet = 6.0, HeightFeet = 5.5
WHERE Brand = 'SUZUKI' AND (
    Model LIKE 'Grand Vitara%' OR Model LIKE 'Vitara%' OR Model LIKE 'Samurai%'
    OR Model LIKE 'Sidekick%' OR Model LIKE 'Jimny%' OR Model LIKE 'Escudo%'
    OR Model LIKE 'XL7%' OR Model LIKE 'Kizashi%'
) AND Category NOT IN ('Motorcycle','ATV');
