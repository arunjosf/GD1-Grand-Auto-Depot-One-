DECLARE @ManagerId bigint = 1;
DECLARE @VehicleId bigint = 1;

SELECT 
    v.Id as VehicleId,
    b.Id as BookingId,
    v.Brand,
    v.Model,
    v.RegistrationNo,
    v.Category,
    v.VerificationStatus, v.HasServiceRecommendation, v.ManagerServiceRemarks,
    o.Id as OwnerId,
    o.FullName as OwnerName,
    o.PhoneNumber as OwnerPhone,
    o.Email as OwnerEmail,
    lo.Id as LotOwnerId,
    lo.FullName as LotOwnerName,
    lo.PhoneNumber as LotOwnerPhone,
    b.StartDate as StoredSince,
    b.PricePerDay,
    vi.ImageUrl,
    od.LastOnDemandImageDate,
    srr.LastServiceReportDate
FROM LotManagers lm
INNER JOIN VehicleStorageProperties p ON lm.PropertyId = p.Id
INNER JOIN Users lo ON p.LotOwnerId = lo.Id
INNER JOIN Bookings b ON lm.PropertyId = b.PropertyId
INNER JOIN Vehicles v ON b.VehicleId = v.Id
INNER JOIN Users o ON b.OwnerId = o.Id
OUTER APPLY (SELECT TOP 1 ImageUrl FROM VehicleImages WHERE VehicleId = v.Id AND EventId IS NULL) vi
OUTER APPLY (SELECT TOP 1 CompletedAt as LastOnDemandImageDate FROM MaintenanceTasks mt WHERE mt.VehicleId = v.Id AND mt.Type = 0 AND mt.Status = 1 ORDER BY mt.CompletedAt DESC) od
OUTER APPLY (SELECT TOP 1 UpdatedAt as LastServiceReportDate FROM ServiceRequests sr INNER JOIN Bookings b2 ON sr.BookingId = b2.Id WHERE b2.VehicleId = v.Id AND sr.IsCompleted = 1 ORDER BY sr.UpdatedAt DESC) srr
WHERE lm.ManagerId = @ManagerId AND lm.IsActive = 1 AND v.Id = @VehicleId
