SET NOCOUNT ON;

DECLARE @U1 BIGINT, @U2 BIGINT, @U3 BIGINT, @U4 BIGINT, @U5 BIGINT;

INSERT INTO Users (FullName, Email, PhoneNumber, PasswordHash, Role, IsActive, IsEmailVerified, CreatedAt, UpdatedAt, IsDeleted) 
VALUES ('Rahul K', 'rahul.k@example.com', '9876543210', 'dummyhash', 1, 1, 1, GETUTCDATE(), GETUTCDATE(), 0);
SET @U1 = SCOPE_IDENTITY();

INSERT INTO Users (FullName, Email, PhoneNumber, PasswordHash, Role, IsActive, IsEmailVerified, CreatedAt, UpdatedAt, IsDeleted) 
VALUES ('Sneha M', 'sneha.m@example.com', '9876543211', 'dummyhash', 1, 1, 1, GETUTCDATE(), GETUTCDATE(), 0);
SET @U2 = SCOPE_IDENTITY();

INSERT INTO Users (FullName, Email, PhoneNumber, PasswordHash, Role, IsActive, IsEmailVerified, CreatedAt, UpdatedAt, IsDeleted) 
VALUES ('Mohammed R', 'mohammed.r@example.com', '9876543212', 'dummyhash', 1, 1, 1, GETUTCDATE(), GETUTCDATE(), 0);
SET @U3 = SCOPE_IDENTITY();

INSERT INTO Users (FullName, Email, PhoneNumber, PasswordHash, Role, IsActive, IsEmailVerified, CreatedAt, UpdatedAt, IsDeleted) 
VALUES ('Anjali P', 'anjali.p@example.com', '9876543213', 'dummyhash', 1, 1, 1, GETUTCDATE(), GETUTCDATE(), 0);
SET @U4 = SCOPE_IDENTITY();

INSERT INTO Users (FullName, Email, PhoneNumber, PasswordHash, Role, IsActive, IsEmailVerified, CreatedAt, UpdatedAt, IsDeleted) 
VALUES ('David V', 'david.v@example.com', '9876543214', 'dummyhash', 1, 1, 1, GETUTCDATE(), GETUTCDATE(), 0);
SET @U5 = SCOPE_IDENTITY();

DECLARE @Props TABLE (Id BIGINT);
INSERT INTO @Props SELECT TOP 10 Id FROM VehicleStorageProperties ORDER BY Id DESC;

-- Insert reviews for User 1
INSERT INTO Reviews (PropertyId, ReviewerId, Rating, Comment, SentimentScore, CreatedAt, UpdatedAt, IsDeleted)
SELECT Id, @U1, 5, 'Fantastic property! Very secure and clean.', 0.9, GETUTCDATE(), GETUTCDATE(), 0 FROM @Props;

-- Insert reviews for User 2
INSERT INTO Reviews (PropertyId, ReviewerId, Rating, Comment, SentimentScore, CreatedAt, UpdatedAt, IsDeleted)
SELECT Id, @U2, 4, 'Good location, very easy to access.', 0.8, GETUTCDATE(), GETUTCDATE(), 0 FROM @Props;

-- Insert reviews for User 3
INSERT INTO Reviews (PropertyId, ReviewerId, Rating, Comment, SentimentScore, CreatedAt, UpdatedAt, IsDeleted)
SELECT Id, @U3, 5, 'Highly recommend. The slots are spacious and well lit.', 0.95, GETUTCDATE(), GETUTCDATE(), 0 FROM @Props;

-- Insert reviews for User 4
INSERT INTO Reviews (PropertyId, ReviewerId, Rating, Comment, SentimentScore, CreatedAt, UpdatedAt, IsDeleted)
SELECT Id, @U4, 5, 'Premium experience. The best parking facility I have used.', 1.0, GETUTCDATE(), GETUTCDATE(), 0 FROM @Props;

-- Insert reviews for User 5
INSERT INTO Reviews (PropertyId, ReviewerId, Rating, Comment, SentimentScore, CreatedAt, UpdatedAt, IsDeleted)
SELECT Id, @U5, 4, 'Safe and reliable. Would definitely use again.', 0.85, GETUTCDATE(), GETUTCDATE(), 0 FROM @Props;

PRINT 'Seeded 5 users and 50 reviews successfully.';
