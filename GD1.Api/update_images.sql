USE GD1;
GO

-- Update Property Images (Exterior)
UPDATE PropertyImages SET ImageUrl = 'https://images.unsplash.com/photo-1605810230434-7631ac76ec81?w=800&q=80' WHERE VehicleStoragePropertyId = 15;
UPDATE PropertyImages SET ImageUrl = 'https://images.unsplash.com/photo-1601628828688-632f38a5a7d0?w=800&q=80' WHERE VehicleStoragePropertyId = 16;
UPDATE PropertyImages SET ImageUrl = 'https://images.unsplash.com/photo-1595844730298-b960fa25fa65?w=800&q=80' WHERE VehicleStoragePropertyId = 17;
UPDATE PropertyImages SET ImageUrl = 'https://images.unsplash.com/photo-1616047006789-b7af5afb8c20?w=800&q=80' WHERE VehicleStoragePropertyId = 18;
UPDATE PropertyImages SET ImageUrl = 'https://images.unsplash.com/photo-1582268611958-ebfd161ef9cf?w=800&q=80' WHERE VehicleStoragePropertyId = 19;
UPDATE PropertyImages SET ImageUrl = 'https://images.unsplash.com/photo-1600607686527-6fb886090705?w=800&q=80' WHERE VehicleStoragePropertyId = 20;
UPDATE PropertyImages SET ImageUrl = 'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=800&q=80' WHERE VehicleStoragePropertyId = 21;
UPDATE PropertyImages SET ImageUrl = 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800&q=80' WHERE VehicleStoragePropertyId = 22;
UPDATE PropertyImages SET ImageUrl = 'https://images.unsplash.com/photo-1595521624992-48a59aef95e3?w=800&q=80' WHERE VehicleStoragePropertyId = 23;
UPDATE PropertyImages SET ImageUrl = 'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800&q=80' WHERE VehicleStoragePropertyId = 24;

-- Update Slot Images (Interior)
WITH CTE AS (
    SELECT Id, ROW_NUMBER() OVER(ORDER BY Id) % 10 as RM FROM VehicleStorageSlots WHERE PropertyId >= 15
)
UPDATE S
SET ImageUrl = CHOOSE(C.RM + 1, 
    'https://images.unsplash.com/photo-1574345520970-891fc4c9b32c?w=800&q=80',
    'https://images.unsplash.com/photo-1507136566006-ceac4a23a31c?w=800&q=80',
    'https://images.unsplash.com/photo-1517524008697-84bbe3c3fd98?w=800&q=80',
    'https://images.unsplash.com/photo-1617195737496-bc30194e3a19?w=800&q=80',
    'https://images.unsplash.com/photo-1596700772277-33afc5b43292?w=800&q=80',
    'https://images.unsplash.com/photo-1602028816407-16004b5003c2?w=800&q=80',
    'https://images.unsplash.com/photo-1563816578051-50e5025d57b3?w=800&q=80',
    'https://images.unsplash.com/photo-1621252981329-87364cc91ecf?w=800&q=80',
    'https://images.unsplash.com/photo-1587582423116-ec07293f0395?w=800&q=80',
    'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?w=800&q=80'
)
FROM VehicleStorageSlots S
JOIN CTE C ON S.Id = C.Id;
GO
