-- SaigonBite Cloud-Kitchen Database Schema & Seed Data (Team 9 - Tier 1)
-- Supports BR1-BR18, Single Users Schema (BR18), Soft-Delete (NFR5), Snapshot (BR17), Audit Trail (NFR6)

IF DB_ID('SaigonBiteDB') IS NULL
    CREATE DATABASE SaigonBiteDB;
GO
USE SaigonBiteDB;
GO

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    FullName NVARCHAR(120) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role NVARCHAR(30) NOT NULL CHECK (Role IN ('LocalCustomer','ForeignTourist','KitchenStaff','Shipper','Admin')),
    HubID INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT DATEADD(HOUR, 7, GETUTCDATE())
);

CREATE TABLE LocalCustomerProfiles (
    UserID INT PRIMARY KEY FOREIGN KEY REFERENCES Users(UserID),
    PhoneNumber VARCHAR(10) NOT NULL UNIQUE,
    IsOtpVerified BIT NOT NULL DEFAULT 1
);

CREATE TABLE ForeignTouristProfiles (
    UserID INT PRIMARY KEY FOREIGN KEY REFERENCES Users(UserID),
    PassportNumber VARCHAR(9) NOT NULL UNIQUE,
    Nationality NVARCHAR(80) NOT NULL,
    Email NVARCHAR(120) NOT NULL,
    HotelAddress NVARCHAR(250) NOT NULL,
    IsCodeVerified BIT NOT NULL DEFAULT 1
);

CREATE TABLE Hubs (
    HubID INT IDENTITY(1,1) PRIMARY KEY,
    HubName NVARCHAR(100) NOT NULL,
    District NVARCHAR(80) NOT NULL,
    MaxCapacity INT NOT NULL DEFAULT 100,
    CurrentLoadPct DECIMAL(5,2) NOT NULL DEFAULT 0,
    HeavyRainFlag BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE HubDistrictDistances (
    HubID INT NOT NULL FOREIGN KEY REFERENCES Hubs(HubID),
    District NVARCHAR(80) NOT NULL,
    DistanceKm DECIMAL(5,2) NOT NULL,
    PRIMARY KEY (HubID, District)
);

CREATE TABLE VirtualBrands (
    BrandID INT IDENTITY(1,1) PRIMARY KEY,
    BrandName NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE MenuItems (
    ItemID INT IDENTITY(1,1) PRIMARY KEY,
    HubID INT NOT NULL FOREIGN KEY REFERENCES Hubs(HubID),
    BrandID INT NOT NULL FOREIGN KEY REFERENCES VirtualBrands(BrandID),
    ItemName NVARCHAR(120) NOT NULL,
    UnitPrice DECIMAL(12,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Available',
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE Vouchers (
    VoucherCode NVARCHAR(40) PRIMARY KEY,
    DiscountType NVARCHAR(10) NOT NULL CHECK (DiscountType IN ('PERCENT','FIXED')),
    DiscountValue DECIMAL(12,2) NOT NULL,
    MaxCap DECIMAL(12,2) NULL,
    MinSubtotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE Orders (
    OrderID INT IDENTITY(1040,1) PRIMARY KEY,
    CustomerID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    HubID INT NOT NULL FOREIGN KEY REFERENCES Hubs(HubID),
    ShipperID INT NULL FOREIGN KEY REFERENCES Users(UserID),
    DeliveryAddress NVARCHAR(250) NOT NULL,
    DistanceKm DECIMAL(5,2) NOT NULL,
    VoucherCode NVARCHAR(40) NULL FOREIGN KEY REFERENCES Vouchers(VoucherCode),
    SubtotalS DECIMAL(12,2) NOT NULL,
    QuietKitchenQ DECIMAL(12,2) NOT NULL,
    VoucherV DECIMAL(12,2) NOT NULL,
    FoodTotalF DECIMAL(12,2) NOT NULL,
    DeliveryFeeD DECIMAL(12,2) NOT NULL,
    RushSurchargeR DECIMAL(12,2) NOT NULL,
    RainSurchargeR2 DECIMAL(12,2) NOT NULL,
    RoundedTotal DECIMAL(12,2) NOT NULL,
    PaymentMethod NVARCHAR(30) NOT NULL,
    PaymentStatus NVARCHAR(30) NOT NULL,
    OrderStatus NVARCHAR(30) NOT NULL CHECK (OrderStatus IN ('Placed','Confirmed','Preparing','Ready','Out for Delivery','Delivered','Cancelled','Failed')),
    CreatedAtUtc7 DATETIME2 NOT NULL DEFAULT DATEADD(HOUR, 7, GETUTCDATE())
);

CREATE TABLE OrderLines (
    OrderLineID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderID),
    ItemID INT NOT NULL FOREIGN KEY REFERENCES MenuItems(ItemID),
    ItemNameSnapshot NVARCHAR(120) NOT NULL,
    BrandCategorySnapshot NVARCHAR(50) NOT NULL,
    UnitPriceSnapshot DECIMAL(12,2) NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity BETWEEN 1 AND 20)
);

CREATE TABLE OrderPriceSnapshots (
    OrderID INT PRIMARY KEY FOREIGN KEY REFERENCES Orders(OrderID),
    PlacementUtc7 DATETIME2 NOT NULL,
    HubLoadPctSnapshot DECIMAL(5,2) NOT NULL,
    RainFlagSnapshot BIT NOT NULL,
    DistanceKmSnapshot DECIMAL(5,2) NOT NULL
);

CREATE TABLE OrderStatusHistory (
    HistoryID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderID),
    FromState NVARCHAR(30) NULL,
    ToState NVARCHAR(30) NOT NULL,
    ActorID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    ActorRole NVARCHAR(30) NOT NULL,
    ChangedAtUtc7 DATETIME2 NOT NULL DEFAULT DATEADD(HOUR, 7, GETUTCDATE()),
    Reason NVARCHAR(250) NULL
);

-- Seed Data matching Team 9 Figma Prototype
INSERT INTO Hubs (HubName, District, MaxCapacity, CurrentLoadPct, HeavyRainFlag) VALUES
(N'District 1', N'District 1', 100, 45.00, 0),
(N'Binh Thanh', N'Binh Thanh', 100, 92.00, 0),
(N'Thu Duc',    N'Thu Duc',    100, 22.00, 1);

INSERT INTO HubDistrictDistances (HubID, District, DistanceKm) VALUES
(1, N'District 1', 1.8), (1, N'District 3', 3.2), (1, N'Binh Thanh', 5.5),
(3, N'Thu Duc',    3.5), (3, N'District 9', 6.2);

INSERT INTO VirtualBrands (BrandName, Category) VALUES
(N'Pho 24',         N'Noodles'),
(N'Bun Cha 36',     N'Noodles'),
(N'Com Tam SG',     N'Rice'),
(N'Banh Mi Hoa Ma', N'Bread'),
(N'Drinks Bar',     N'Drinks');

INSERT INTO MenuItems (HubID, BrandID, ItemName, UnitPrice, Status, IsActive) VALUES
(3, 1, N'Pho Bo Tai',     65000, 'Available', 1),
(3, 2, N'Bun Cha Ha Noi', 55000, 'Available', 1),
(3, 3, N'Com Tam Suon',   60000, 'Available', 1),
(3, 4, N'Banh Mi Thit',   30000, 'Available', 1),
(3, 5, N'Tra Dao Cam Sa', 35000, 'Available', 1),
(1, 1, N'Pho Bo Tai',     65000, 'Available', 1),
(1, 3, N'Com Tam Suon',   60000, 'Available', 1);

INSERT INTO Vouchers (VoucherCode, DiscountType, DiscountValue, MaxCap, MinSubtotal, IsActive) VALUES
(N'WELCOME15K', 'FIXED',   15000, NULL,  100000, 1),
(N'SAVE20PCT',  'PERCENT', 20,    30000, 100000, 1);

-- Demo Accounts (SHA-256 hash of 'Password123!')
INSERT INTO Users (Username, FullName, PasswordHash, Role, HubID) VALUES
(N'0901234567', N'Nguyen Van A',  N'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'LocalCustomer', NULL),
(N'C1234567',   N'John Smith',    N'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'ForeignTourist', NULL),
(N'kitchen_td', N'Kitchen Thu Duc', N'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'KitchenStaff', 3),
(N'shipper_td', N'Shipper Thu Duc', N'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'Shipper', 3),
(N'admin',      N'System Admin',  N'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'Admin', NULL);

INSERT INTO LocalCustomerProfiles (UserID, PhoneNumber) VALUES (1, '0901234567');
INSERT INTO ForeignTouristProfiles (UserID, PassportNumber, Nationality, Email, HotelAddress)
VALUES (2, 'C1234567', N'United States', N'john@mail.com', N'25 Le Loi, District 1');
GO
