-- ============================================================
-- FORD: Fix heavy trucks, Transit vans, and F-series trucks
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Truck', LengthFeet = 19.5, WidthFeet = 6.7, HeightFeet = 6.5
WHERE Brand = 'Ford' AND (
    Model LIKE 'F-1%' OR Model LIKE 'F-2%' OR Model LIKE 'F-3%' OR Model LIKE 'F-4%' OR Model LIKE 'F-5%'
    OR Model LIKE 'F-6%' OR Model LIKE 'F-7%' OR Model LIKE 'F-8%'
    OR Model LIKE 'F 1%' OR Model LIKE 'Super Duty%' OR Model LIKE 'F Series%'
    OR Model IN ('F100','F150','F-150','F250','F-250','F350','F-350','F450','F-450','F650','F-650','F750','F-750')
    OR Model LIKE 'Ranger%' AND Model NOT LIKE '%Pickup%'
);

UPDATE VehicleCatalog SET Category = 'Van', LengthFeet = 18.5, WidthFeet = 6.5, HeightFeet = 7.5
WHERE Brand = 'Ford' AND (
    Model LIKE 'Transit%' OR Model LIKE 'E-Series%' OR Model LIKE 'Econoline%'
    OR Model LIKE 'Club Wagon%' OR Model LIKE 'Aerostar%' OR Model LIKE 'Windstar%'
    OR Model LIKE 'Freestar%' OR Model = 'Transit Connect'
);

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 16.0, WidthFeet = 6.3, HeightFeet = 5.8
WHERE Brand = 'Ford' AND (
    Model LIKE 'Explorer%' OR Model LIKE 'Expedition%' OR Model LIKE 'Escape%'
    OR Model LIKE 'Edge%' OR Model LIKE 'EcoSport%' OR Model LIKE 'Bronco%'
    OR Model LIKE 'Flex%' OR Model LIKE 'Territory%' OR Model LIKE 'Puma%'
    OR Model LIKE 'Freestyle%'
) AND Category NOT IN ('Truck','Van');

UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 15.5, WidthFeet = 6.0, HeightFeet = 4.7
WHERE Brand = 'Ford' AND (
    Model LIKE 'Fusion%' OR Model LIKE 'Taurus%' OR Model LIKE 'Focus%' OR Model LIKE 'Fiesta%'
    OR Model LIKE 'Mondeo%' OR Model LIKE 'Galaxy%' OR Model LIKE 'Contour%'
    OR Model LIKE 'Crown Victoria%' OR Model LIKE 'Five Hundred%' OR Model IN ('Pinto','Fairlane','Falcon','LTD')
) AND Category NOT IN ('Truck','Van','SUV');

-- ============================================================
-- GMC: Fix heavy trucks
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Truck', LengthFeet = 19.5, WidthFeet = 6.7, HeightFeet = 6.5
WHERE Brand = 'GMC' AND (
    Model LIKE 'Sierra%' OR Model LIKE 'Canyon%' OR Model LIKE 'Topkick%'
    OR Model LIKE 'C3%' OR Model LIKE 'C4%' OR Model LIKE 'C5%' OR Model LIKE 'C6%' OR Model LIKE 'C7%'
    OR Model LIKE 'W3%' OR Model LIKE 'W4%' OR Model LIKE 'W5%' OR Model LIKE 'W7%'
    OR Model LIKE '1500%' OR Model LIKE '2500%' OR Model LIKE '3500%'
    OR Model LIKE 'P3%' OR Model LIKE 'P6%' OR Model LIKE 'P7%' OR Model LIKE 'Pickup%'
);

UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 16.5, WidthFeet = 6.5, HeightFeet = 6.0
WHERE Brand = 'GMC' AND (
    Model LIKE 'Yukon%' OR Model LIKE 'Terrain%' OR Model LIKE 'Acadia%'
    OR Model LIKE 'Jimmy%' OR Model LIKE 'Envoy%' OR Model LIKE 'Typhoon%'
    OR Model LIKE 'Suburban%'
) AND Category != 'Truck';

UPDATE VehicleCatalog SET Category = 'Van', LengthFeet = 18.5, WidthFeet = 6.5, HeightFeet = 7.0
WHERE Brand = 'GMC' AND (
    Model LIKE 'Safari%' OR Model LIKE 'Savana%' OR Model LIKE 'Vandura%'
) AND Category NOT IN ('Truck','SUV');
