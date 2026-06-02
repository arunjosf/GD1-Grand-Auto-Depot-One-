SET NOCOUNT ON;
DECLARE @PropId BIGINT;

INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES (22, 'GD1-KE-0100', 'Neospace Parking Bay', 'Premium secure vehicle storage located in Kakkanchery. Fully certified.', '02 Neospace, KINFRA TECHNO INDUSTRIAL PARK', 'Kakkanchery', 'Kerala', 'India', 11.1578, 75.8858, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet,Washing', 500.00 + 104, 4.5 + 0.3702520997077441, 7, GETUTCDATE(), GETUTCDATE(), 0);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO PropertyImages (ApplicationId, VehicleStoragePropertyId, ImageUrl, Label, UploadedBy, IsMain, CreatedAt, UpdatedAt, IsDeleted) VALUES (NULL, @PropId, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139568/GD1_Auto_Depot/seed/modern_garage_front_1780139341116_1780139556.jpg', 'Property Main', 'System', 1, GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-101', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139572/GD1_Auto_Depot/seed/slot_1_1780139362282_1780139568.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-102', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139579/GD1_Auto_Depot/seed/slot_2_1780139377811_1780139571.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-103', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139592/GD1_Auto_Depot/seed/slot_3_1780139402332_1780139578.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-104', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139603/GD1_Auto_Depot/seed/slot_4_1780139430678_1780139591.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES (22, 'GD1-KE-0101', 'Hilite City Auto Vault', 'Premium secure vehicle storage located in Kozhikode. Fully certified.', 'Hilite City, Thondayad Bypass', 'Kozhikode', 'Kerala', 'India', 11.2588, 75.82, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet,Washing', 500.00 + 83, 4.5 + 0.15820390561198666, 36, GETUTCDATE(), GETUTCDATE(), 0);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO PropertyImages (ApplicationId, VehicleStoragePropertyId, ImageUrl, Label, UploadedBy, IsMain, CreatedAt, UpdatedAt, IsDeleted) VALUES (NULL, @PropId, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139568/GD1_Auto_Depot/seed/modern_garage_front_1780139341116_1780139556.jpg', 'Property Main', 'System', 1, GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-101', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139572/GD1_Auto_Depot/seed/slot_1_1780139362282_1780139568.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-102', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139579/GD1_Auto_Depot/seed/slot_2_1780139377811_1780139571.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-103', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139592/GD1_Auto_Depot/seed/slot_3_1780139402332_1780139578.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-104', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139603/GD1_Auto_Depot/seed/slot_4_1780139430678_1780139591.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES (22, 'GD1-KE-0102', 'Focus Mall Underground', 'Premium secure vehicle storage located in Kozhikode. Fully certified.', 'Focus Mall Area, Rajaji Road', 'Kozhikode', 'Kerala', 'India', 11.256, 75.783, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet,Washing', 500.00 + 250, 4.5 + 0.49976566672501443, 50, GETUTCDATE(), GETUTCDATE(), 0);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO PropertyImages (ApplicationId, VehicleStoragePropertyId, ImageUrl, Label, UploadedBy, IsMain, CreatedAt, UpdatedAt, IsDeleted) VALUES (NULL, @PropId, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139568/GD1_Auto_Depot/seed/modern_garage_front_1780139341116_1780139556.jpg', 'Property Main', 'System', 1, GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-101', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139572/GD1_Auto_Depot/seed/slot_1_1780139362282_1780139568.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-102', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139579/GD1_Auto_Depot/seed/slot_2_1780139377811_1780139571.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-103', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139592/GD1_Auto_Depot/seed/slot_3_1780139402332_1780139578.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-104', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139603/GD1_Auto_Depot/seed/slot_4_1780139430678_1780139591.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES (22, 'GD1-KE-0103', 'Kottooli Premium Storage', 'Premium secure vehicle storage located in Kozhikode. Fully certified.', 'Kottooli', 'Kozhikode', 'Kerala', 'India', 11.267, 75.811, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet,Washing', 500.00 + 175, 4.5 + 0.33244585018072936, 54, GETUTCDATE(), GETUTCDATE(), 0);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO PropertyImages (ApplicationId, VehicleStoragePropertyId, ImageUrl, Label, UploadedBy, IsMain, CreatedAt, UpdatedAt, IsDeleted) VALUES (NULL, @PropId, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139568/GD1_Auto_Depot/seed/modern_garage_front_1780139341116_1780139556.jpg', 'Property Main', 'System', 1, GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-101', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139572/GD1_Auto_Depot/seed/slot_1_1780139362282_1780139568.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-102', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139579/GD1_Auto_Depot/seed/slot_2_1780139377811_1780139571.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-103', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139592/GD1_Auto_Depot/seed/slot_3_1780139402332_1780139578.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-104', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139603/GD1_Auto_Depot/seed/slot_4_1780139430678_1780139591.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES (22, 'GD1-KE-0104', 'Meenchanda Transit Bay', 'Premium secure vehicle storage located in Kozhikode. Fully certified.', 'Meenchanda Bypass Road', 'Kozhikode', 'Kerala', 'India', 11.2223, 75.8016, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet,Washing', 500.00 + 280, 4.5 + 0.4816468290154147, 13, GETUTCDATE(), GETUTCDATE(), 0);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO PropertyImages (ApplicationId, VehicleStoragePropertyId, ImageUrl, Label, UploadedBy, IsMain, CreatedAt, UpdatedAt, IsDeleted) VALUES (NULL, @PropId, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139568/GD1_Auto_Depot/seed/modern_garage_front_1780139341116_1780139556.jpg', 'Property Main', 'System', 1, GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-101', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139572/GD1_Auto_Depot/seed/slot_1_1780139362282_1780139568.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-102', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139579/GD1_Auto_Depot/seed/slot_2_1780139377811_1780139571.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-103', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139592/GD1_Auto_Depot/seed/slot_3_1780139402332_1780139578.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-104', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139603/GD1_Auto_Depot/seed/slot_4_1780139430678_1780139591.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES (22, 'GD1-KE-0105', 'Ramanattukara Secure Hub', 'Premium secure vehicle storage located in Kozhikode. Fully certified.', 'Ramanattukara Junction', 'Kozhikode', 'Kerala', 'India', 11.1764, 75.8647, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet,Washing', 500.00 + 196, 4.5 + 0.27545440257130394, 53, GETUTCDATE(), GETUTCDATE(), 0);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO PropertyImages (ApplicationId, VehicleStoragePropertyId, ImageUrl, Label, UploadedBy, IsMain, CreatedAt, UpdatedAt, IsDeleted) VALUES (NULL, @PropId, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139568/GD1_Auto_Depot/seed/modern_garage_front_1780139341116_1780139556.jpg', 'Property Main', 'System', 1, GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-101', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139572/GD1_Auto_Depot/seed/slot_1_1780139362282_1780139568.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-102', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139579/GD1_Auto_Depot/seed/slot_2_1780139377811_1780139571.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-103', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139592/GD1_Auto_Depot/seed/slot_3_1780139402332_1780139578.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-104', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139603/GD1_Auto_Depot/seed/slot_4_1780139430678_1780139591.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES (22, 'GD1-KE-0106', 'Kondotty Airport Parking', 'Premium secure vehicle storage located in Malappuram. Fully certified.', 'Kondotty Town, Near Airport', 'Malappuram', 'Kerala', 'India', 11.1396, 75.962, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet,Washing', 500.00 + 18, 4.5 + 0.3478545123677036, 44, GETUTCDATE(), GETUTCDATE(), 0);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO PropertyImages (ApplicationId, VehicleStoragePropertyId, ImageUrl, Label, UploadedBy, IsMain, CreatedAt, UpdatedAt, IsDeleted) VALUES (NULL, @PropId, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139568/GD1_Auto_Depot/seed/modern_garage_front_1780139341116_1780139556.jpg', 'Property Main', 'System', 1, GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-101', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139572/GD1_Auto_Depot/seed/slot_1_1780139362282_1780139568.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-102', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139579/GD1_Auto_Depot/seed/slot_2_1780139377811_1780139571.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-103', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139592/GD1_Auto_Depot/seed/slot_3_1780139402332_1780139578.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-104', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139603/GD1_Auto_Depot/seed/slot_4_1780139430678_1780139591.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES (22, 'GD1-KE-0107', 'University Campus Garage', 'Premium secure vehicle storage located in Malappuram. Fully certified.', 'Tenhipalam, University Campus', 'Malappuram', 'Kerala', 'India', 11.1309, 75.8961, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet,Washing', 500.00 + 22, 4.5 + 0.3415013828888015, 18, GETUTCDATE(), GETUTCDATE(), 0);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO PropertyImages (ApplicationId, VehicleStoragePropertyId, ImageUrl, Label, UploadedBy, IsMain, CreatedAt, UpdatedAt, IsDeleted) VALUES (NULL, @PropId, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139568/GD1_Auto_Depot/seed/modern_garage_front_1780139341116_1780139556.jpg', 'Property Main', 'System', 1, GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-101', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139572/GD1_Auto_Depot/seed/slot_1_1780139362282_1780139568.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-102', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139579/GD1_Auto_Depot/seed/slot_2_1780139377811_1780139571.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-103', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139592/GD1_Auto_Depot/seed/slot_3_1780139402332_1780139578.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-104', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139603/GD1_Auto_Depot/seed/slot_4_1780139430678_1780139591.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES (22, 'GD1-KE-0108', 'Down Hill Auto Safe', 'Premium secure vehicle storage located in Malappuram. Fully certified.', 'Down Hill', 'Malappuram', 'Kerala', 'India', 11.0423, 76.0716, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet,Washing', 500.00 + 256, 4.5 + 0.499589406572887, 48, GETUTCDATE(), GETUTCDATE(), 0);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO PropertyImages (ApplicationId, VehicleStoragePropertyId, ImageUrl, Label, UploadedBy, IsMain, CreatedAt, UpdatedAt, IsDeleted) VALUES (NULL, @PropId, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139568/GD1_Auto_Depot/seed/modern_garage_front_1780139341116_1780139556.jpg', 'Property Main', 'System', 1, GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-101', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139572/GD1_Auto_Depot/seed/slot_1_1780139362282_1780139568.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-102', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139579/GD1_Auto_Depot/seed/slot_2_1780139377811_1780139571.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-103', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139592/GD1_Auto_Depot/seed/slot_3_1780139402332_1780139578.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-104', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139603/GD1_Auto_Depot/seed/slot_4_1780139430678_1780139591.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES (22, 'GD1-KE-0109', 'Manjeri Central Parking', 'Premium secure vehicle storage located in Malappuram. Fully certified.', 'Manjeri Town', 'Malappuram', 'Kerala', 'India', 11.1186, 76.1219, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet,Washing', 500.00 + 49, 4.5 + 0.35175701937680587, 25, GETUTCDATE(), GETUTCDATE(), 0);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO PropertyImages (ApplicationId, VehicleStoragePropertyId, ImageUrl, Label, UploadedBy, IsMain, CreatedAt, UpdatedAt, IsDeleted) VALUES (NULL, @PropId, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139568/GD1_Auto_Depot/seed/modern_garage_front_1780139341116_1780139556.jpg', 'Property Main', 'System', 1, GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-101', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139572/GD1_Auto_Depot/seed/slot_1_1780139362282_1780139568.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-102', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139579/GD1_Auto_Depot/seed/slot_2_1780139377811_1780139571.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-103', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139592/GD1_Auto_Depot/seed/slot_3_1780139402332_1780139578.jpg', GETUTCDATE(), GETUTCDATE(), 0);

INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-104', 'Private Garage', 0, 200.0, 12.0, 'https://res.cloudinary.com/djpuczbnc/image/upload/v1780139603/GD1_Auto_Depot/seed/slot_4_1780139430678_1780139591.jpg', GETUTCDATE(), GETUTCDATE(), 0);
