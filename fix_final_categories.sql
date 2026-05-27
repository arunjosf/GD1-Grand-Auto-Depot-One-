-- Mercedes L/LP/LPS series = Heavy Trucks (not Sedan)
UPDATE VehicleCatalog SET Category = 'Heavy Truck', LengthFeet = 25.0, WidthFeet = 8.0, HeightFeet = 12.0
WHERE Brand = 'MERCEDES-BENZ' AND (
    Model LIKE 'L1%' OR Model LIKE 'L2%' OR Model LIKE 'L3%' OR Model LIKE 'LP%'
    OR Model LIKE 'LPS%' OR Model LIKE 'Actros%' OR Model LIKE 'Arocs%'
    OR Model LIKE 'Atego%' OR Model LIKE 'Unimog%' OR Model LIKE 'eSprinter%'
);

-- Mercedes GL-Class, GLC-Class, GLE-Class, GLK-Class, ML-Class, M-Class -> SUV
UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 16.0, WidthFeet = 6.3, HeightFeet = 5.8
WHERE Brand = 'MERCEDES-BENZ' AND (
    Model IN ('G-Class','GL-Class','GLA-Class','GLB-Class','GLC-Class','GLE-Class','GLS-Class','GLK-Class','ML-Class','M-Class','EQC-Class','EQB-Class','EQE-Class SUV','EQS-Class SUV','R-Class')
);

-- Mercedes SL-Class, SLK-Class, SLC-Class, CLK-Class, CL-Class, CLS-Class, SLS-Class, SLR McLaren -> Coupe
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 15.0, WidthFeet = 6.1, HeightFeet = 4.7
WHERE Brand = 'MERCEDES-BENZ' AND (
    Model IN ('SL-Class','SLK-Class','SLC-Class','CLK-Class','CL-Class','SLS-Class','SLR McLaren','CLE')
    OR Model LIKE 'CLS-Class%'
);

-- Mercedes Metris, Sprinter, eSprinter -> Van
UPDATE VehicleCatalog SET Category = 'Van', LengthFeet = 19.0, WidthFeet = 6.5, HeightFeet = 8.0
WHERE Brand = 'MERCEDES-BENZ' AND Model IN ('Metris','Sprinter','eSprinter');

-- Ford heavy trucks: L/LT/LS/LN/CF/CL series = Heavy Truck
UPDATE VehicleCatalog SET Category = 'Heavy Truck', LengthFeet = 25.0, WidthFeet = 8.0, HeightFeet = 12.0
WHERE Brand = 'Ford' AND (
    Model LIKE 'L8%' OR Model LIKE 'L9%' OR Model LIKE 'LL9%' OR Model LIKE 'LLA9%'
    OR Model LIKE 'LLS9%' OR Model LIKE 'LS8%' OR Model LIKE 'LS9%' OR Model LIKE 'LA8%'
    OR Model LIKE 'LA9%' OR Model LIKE 'LN7%' OR Model LIKE 'LN8%' OR Model LIKE 'LN9%'
    OR Model LIKE 'LNT%' OR Model LIKE 'LTA%' OR Model LIKE 'LTS%' OR Model LIKE 'LTL%'
    OR Model LIKE 'LTLA%' OR Model LIKE 'LTLS%' OR Model LIKE 'LT8%' OR Model LIKE 'LT9%'
    OR Model LIKE 'CF7%' OR Model LIKE 'CF8%' OR Model LIKE 'CFT%' OR Model LIKE 'CL9%'
    OR Model LIKE 'CLT%' OR Model LIKE 'CT8%' OR Model LIKE 'C8%' OR Model LIKE 'C800%'
    OR Model LIKE 'A8%' OR Model LIKE 'A9%' OR Model LIKE 'AT8%' OR Model LIKE 'AT9%'
    OR Model LIKE 'FT8%' OR Model LIKE 'FT9%'
    OR Model IN ('Low Cab Forward','Commercial Chassis','Motorhome Chassis')
);

-- Ford B/P series (buses, delivery) = Bus/Heavy Truck
UPDATE VehicleCatalog SET Category = 'Bus', LengthFeet = 35.0, WidthFeet = 8.0, HeightFeet = 10.0
WHERE Brand = 'Ford' AND (
    Model LIKE 'B6%' OR Model LIKE 'B7%' OR Model LIKE 'B8%'
    OR Model LIKE 'P6%' OR Model LIKE 'P7%' OR Model LIKE 'P8%'
);

-- Ford E-series (Econoline vans) -> Van
UPDATE VehicleCatalog SET Category = 'Van', LengthFeet = 18.5, WidthFeet = 6.5, HeightFeet = 7.0
WHERE Brand = 'Ford' AND (
    Model LIKE 'E-1%' OR Model LIKE 'E-2%' OR Model LIKE 'E-3%' OR Model LIKE 'E-4%' OR Model LIKE 'E-5%'
    OR Model IN ('Transit','Transit Connect','Windstar','Aerostar')
) AND Category NOT IN ('Truck','Heavy Truck','Bus');

-- Ford Bronco, Bronco II, Excursion, Expedition, Explorer -> SUV  
UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 16.0, WidthFeet = 6.4, HeightFeet = 6.0
WHERE Brand = 'Ford' AND (
    Model IN ('Bronco','Bronco II','Excursion','Expedition','Explorer','Explorer Sport')
    OR Model LIKE 'Bronco%' OR Model LIKE 'Expedition%'
) AND Category NOT IN ('Truck','Heavy Truck','Bus','Van');

-- Ford Mustang, Thunderbird, Probe, ZX2 -> Coupe
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 15.0, WidthFeet = 6.1, HeightFeet = 4.5
WHERE Brand = 'Ford' AND (
    Model LIKE 'Mustang%' OR Model LIKE 'Thunderbird%'
    OR Model IN ('Probe','ZX2','GT')
) AND Category NOT IN ('Truck','Heavy Truck','SUV','Van','Bus');

-- Ford garbage/non-vehicle entries -> Other
UPDATE VehicleCatalog SET Category = 'Other'
WHERE Brand = 'Ford' AND (
    Model LIKE '%Trailer%' OR Model LIKE '%Manufacturing%' OR Model LIKE '%Welding%'
    OR Model LIKE '%Tanks%' OR Model LIKE '%Supply%' OR Model LIKE '%LLC%'
    OR Model LIKE '%Inc.%' OR Model LIKE '%LTD%'
    OR Model IN ('Affordable Aluminum','Recreational Vehicle')
);

-- MINI: Fix remaining wrong categories
-- Cooper, Hardtop, Clubman -> Hatchback
UPDATE VehicleCatalog SET Category = 'Hatchback', LengthFeet = 12.5, WidthFeet = 5.6, HeightFeet = 4.8
WHERE Brand = 'MINI' AND Model IN ('Cooper','Hardtop','Cooper S','Electric');

-- MINI Countryman, Paceman -> SUV
UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 13.5, WidthFeet = 5.9, HeightFeet = 5.3
WHERE Brand = 'MINI' AND Model IN ('Countryman','Paceman');

-- MINI Clubman -> Hatchback (wagon-ish)
UPDATE VehicleCatalog SET Category = 'Hatchback', LengthFeet = 13.0, WidthFeet = 5.7, HeightFeet = 5.0
WHERE Brand = 'MINI' AND Model = 'Clubman';

-- MINI Cooper Convertible -> Convertible (use Coupe category)
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 12.5, WidthFeet = 5.6, HeightFeet = 4.6
WHERE Brand = 'MINI' AND Model IN ('Cooper Convertible','Cooper Coupe','Cooper Roadster');

-- MINI non-vehicle junk entries -> Other  
UPDATE VehicleCatalog SET Category = 'Other'
WHERE Brand = 'MINI' AND Model IN (
    'Delta Waseca Mini','Carolina Trikes & Minis','Mobile Frac Storage Tank',
    'ODB Trailer','Mobile Mini Inc.','Minitears Company','Dominight LLC',
    'Miller','Santa Barbara','Pete','KW','FL','Mack','Pony','MiniMixx',
    'GEMINI AUTO & TRAILER INC','Dominion Motorcycle','My Mini Trailer LLC.',
    'Los Lobos Mini Choppers, LLC','MiniKamp','R.V. Mini Mart, Inc.',
    'MINI MONSOON','Brockway Mini Homes'
);

-- ASTON MARTIN: All are Coupes (grand tourers) except DBX (SUV)
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 15.0, WidthFeet = 6.3, HeightFeet = 4.7
WHERE Brand = 'ASTON MARTIN' AND Model NOT IN ('DBX') AND Category != 'SUV';

-- Aston Martin DB9, DB11, DB12, Rapide, Vantage, Virage, Lagonda, Vanquish Zagato, Valhalla, Valiant -> Coupe
-- (already covered above but let's be explicit)
UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 15.5, WidthFeet = 6.4, HeightFeet = 5.6
WHERE Brand = 'ASTON MARTIN' AND Model = 'DBX';
