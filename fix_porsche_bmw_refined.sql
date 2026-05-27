-- Fix Porsche Cayenne, Macan -> SUV
UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 15.8, WidthFeet = 6.4, HeightFeet = 5.5
WHERE Brand = 'PORSCHE' AND (Model LIKE 'Cayenne%' OR Model LIKE 'Macan%');

-- Fix Porsche Taycan -> Sedan (EV)
UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 16.0, WidthFeet = 6.3, HeightFeet = 4.8
WHERE Brand = 'PORSCHE' AND Model LIKE 'Taycan%';

-- Fix BMW i8 -> Coupe (sports car)
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.5, WidthFeet = 6.2, HeightFeet = 4.5
WHERE Brand = 'BMW' AND Model = 'i8';

-- Fix BMW i4 -> Sedan / Gran Coupe (EV)  
UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 15.8, WidthFeet = 6.2, HeightFeet = 4.9
WHERE Brand = 'BMW' AND Model IN ('i4','i5');

-- Fix BMW M3, M5 which are 3-door/4-door sport sedans
UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 15.5, WidthFeet = 6.1, HeightFeet = 4.7
WHERE Brand = 'BMW' AND Model IN ('M3','M340i','M5','M550i');

-- Fix Porsche 924 -> Coupe (sports coupe, not sedan)
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.2, WidthFeet = 5.9, HeightFeet = 4.5
WHERE Brand = 'PORSCHE' AND Model = '924';
