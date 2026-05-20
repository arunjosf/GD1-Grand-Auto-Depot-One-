-- PROMOTE USERS TO LOT OWNERS
UPDATE Users SET Role = 2 WHERE Id IN (48); 

-- Create a new one
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'sarah.lots@example.com')
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive, IsEmailVerified, IsDeleted, CreatedAt, UpdatedAt) 
    VALUES ('Sarah Connor', 'sarah.lots@example.com', 'hashed_pass', 2, 1, 1, 0, GETUTCDATE(), GETUTCDATE());
END

DECLARE @SarahId BIGINT = (SELECT Id FROM Users WHERE Email = 'sarah.lots@example.com');
DECLARE @MajidId BIGINT = 48;

-- CLEAR EXISTING PROPERTIES AND SLOTS SEEDED PREVIOUSLY TO PREVENT DUPLICATES
DELETE FROM VehicleStorageSlots WHERE PropertyId IN (SELECT Id FROM VehicleStorageProperties WHERE LotCode IN ('GD1-KL-0001', 'GD1-KL-0002', 'GD1-KA-0001', 'GD1-MH-0001'));
DELETE FROM VehicleStorageProperties WHERE LotCode IN ('GD1-KL-0001', 'GD1-KL-0002', 'GD1-KA-0001', 'GD1-MH-0001');

-- CREATE VEHICLE STORAGE PROPERTIES
INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES 
(@SarahId, 'GD1-KL-0001', 'EcoSafe Kochi Storage', 'Premium eco-friendly vehicle storage', '12 MG Road', 'Kochi', 'Kerala', 'India', 9.9312, 76.2673, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet', 450.00, 4.8, 12, GETUTCDATE(), GETUTCDATE(), 0),
(@SarahId, 'GD1-KL-0002', 'Airport QuickPark', 'Convenient parking near CIAL', 'Near Airport Gate 2', 'Kochi', 'Kerala', 'India', 10.1556, 76.3910, 'Active', 1, 1, 1, 0, 1, 'Washing', 200.00, 4.5, 25, GETUTCDATE(), GETUTCDATE(), 0),
(@MajidId, 'GD1-KA-0001', 'Bangalore Central Auto Depot', 'High-security depot in the heart of the city', 'MG Road, Indiranagar', 'Bangalore', 'Karnataka', 'India', 12.9716, 77.5946, 'Active', 1, 1, 1, 1, 1, 'Luxury service', 600.00, 4.9, 8, GETUTCDATE(), GETUTCDATE(), 0),
(@MajidId, 'GD1-MH-0001', 'Mumbai Marine Storage', 'Secure sea-side storage', 'Marine Drive Area', 'Mumbai', 'Maharashtra', 'India', 18.9220, 72.8347, 'Active', 1, 1, 1, 0, 0, 'Sea view', 800.00, 4.7, 15, GETUTCDATE(), GETUTCDATE(), 0);

DECLARE @Prop1 BIGINT = (SELECT Id FROM VehicleStorageProperties WHERE LotCode = 'GD1-KL-0001');
DECLARE @Prop2 BIGINT = (SELECT Id FROM VehicleStorageProperties WHERE LotCode = 'GD1-KL-0002');
DECLARE @Prop3 BIGINT = (SELECT Id FROM VehicleStorageProperties WHERE LotCode = 'GD1-KA-0001');
DECLARE @Prop4 BIGINT = (SELECT Id FROM VehicleStorageProperties WHERE LotCode = 'GD1-MH-0001');

-- CREATE VEHICLE STORAGE SLOTS FOR THESE PROPERTIES
INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES
(@Prop1, 'A-101', 'Private Garage', 0, 200.0, 12.0, NULL, GETUTCDATE(), GETUTCDATE(), 0),
(@Prop1, 'A-102', 'Private Garage', 0, 200.0, 12.0, NULL, GETUTCDATE(), GETUTCDATE(), 0),
(@Prop1, 'B-201', 'Private Garage', 0, 150.0, 10.0, NULL, GETUTCDATE(), GETUTCDATE(), 0),
(@Prop2, 'S-01', 'Private Garage', 0, 180.0, 9.0, NULL, GETUTCDATE(), GETUTCDATE(), 0),
(@Prop2, 'S-02', 'Private Garage', 0, 180.0, 9.0, NULL, GETUTCDATE(), GETUTCDATE(), 0),
(@Prop3, 'VIP-1', 'Private Garage', 0, 300.0, 15.0, NULL, GETUTCDATE(), GETUTCDATE(), 0),
(@Prop3, 'VIP-2', 'Private Garage', 0, 300.0, 15.0, NULL, GETUTCDATE(), GETUTCDATE(), 0),
(@Prop4, 'M-01', 'Private Garage', 0, 160.0, 10.0, NULL, GETUTCDATE(), GETUTCDATE(), 0);
