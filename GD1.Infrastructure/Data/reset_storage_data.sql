-- FINAL RESET STORAGE DATA SCRIPT (Property -> Slot Model)
-- This script clears all existing application and lot data.

-- 1. Related to finalized Properties
DELETE FROM DamageReports;
DELETE FROM Handoffs;
DELETE FROM ServiceRequests;
DELETE FROM PickupVerifications;
DELETE FROM PickupRequests;
DELETE FROM BookingAgreements;
DELETE FROM DigitalAgreements;
DELETE FROM Bookings;
DELETE FROM Reviews;
DELETE FROM LotManagers;
DELETE FROM VehicleStorageSlots;
DELETE FROM VehicleStorageProperties;

-- 2. Related to Applications and Inspections
DELETE FROM InspectionSlotItems;
DELETE FROM InspectionReports;
DELETE FROM InspectionAssignments;
DELETE FROM FranchiseSlots;
DELETE FROM PropertyImages;
DELETE FROM FranchiseApplications;

-- Reset Identity Seeds
DBCC CHECKIDENT ('FranchiseApplications', RESEED, 0);
DBCC CHECKIDENT ('FranchiseSlots', RESEED, 0);
DBCC CHECKIDENT ('PropertyImages', RESEED, 0);
DBCC CHECKIDENT ('VehicleStorageProperties', RESEED, 0);
DBCC CHECKIDENT ('VehicleStorageSlots', RESEED, 0);
DBCC CHECKIDENT ('InspectionAssignments', RESEED, 0);
DBCC CHECKIDENT ('InspectionReports', RESEED, 0);
DBCC CHECKIDENT ('InspectionSlotItems', RESEED, 0);
DBCC CHECKIDENT ('Bookings', RESEED, 0);
DBCC CHECKIDENT ('Reviews', RESEED, 0);
