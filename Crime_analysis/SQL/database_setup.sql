-- ═══════════════════════════════════════════════
--  CRIME ANALYSIS DATABASE
--  Cities: Abbottabad, Haripur, Mansehra
--  Tables: Users, Areas, CrimeTypes, Crimes,
--          Suspects, Officers, Reports
-- ═══════════════════════════════════════════════

USE master;
GO

-- Drop and recreate database
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'CrimeAnalysis')
BEGIN
    ALTER DATABASE CrimeAnalysis SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CrimeAnalysis;
END
GO

CREATE DATABASE CrimeAnalysis;
GO

USE CrimeAnalysis;
GO

-- ═══════════════════════════════════════════════
--  TABLE 1 — Users (login system)
-- ═══════════════════════════════════════════════
CREATE TABLE Users (
    UserId    INT PRIMARY KEY IDENTITY(1,1),
    Username  NVARCHAR(50)  NOT NULL UNIQUE,
    Password  NVARCHAR(255) NOT NULL,
    Role      NVARCHAR(20)  NOT NULL CHECK(Role IN ('Admin','User')),
    FullName  NVARCHAR(100) NOT NULL,
    CreatedAt DATE          DEFAULT GETDATE()
);
GO

-- ═══════════════════════════════════════════════
--  TABLE 2 — Areas
-- ═══════════════════════════════════════════════
CREATE TABLE Areas (
    AreaId   INT PRIMARY KEY IDENTITY(1,1),
    AreaName NVARCHAR(100) NOT NULL,
    City     NVARCHAR(100) NOT NULL
);
GO

-- ═══════════════════════════════════════════════
--  TABLE 3 — CrimeTypes
-- ═══════════════════════════════════════════════
CREATE TABLE CrimeTypes (
    TypeId   INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(100) NOT NULL,
    Category NVARCHAR(100)
);
GO

-- ═══════════════════════════════════════════════
--  TABLE 4 — Officers
-- ═══════════════════════════════════════════════
CREATE TABLE Officers (
    OfficerId   INT PRIMARY KEY IDENTITY(1,1),
    FullName    NVARCHAR(100) NOT NULL,
    BadgeNumber NVARCHAR(20)  NOT NULL UNIQUE,
    Rank        NVARCHAR(50),
    City        NVARCHAR(100),
    Phone       NVARCHAR(20)
);
GO

-- ═══════════════════════════════════════════════
--  TABLE 5 — Crimes (main table)
-- ═══════════════════════════════════════════════
CREATE TABLE Crimes (
    CrimeId     INT PRIMARY KEY IDENTITY(1,1),
    TypeId      INT FOREIGN KEY REFERENCES CrimeTypes(TypeId),
    AreaId      INT FOREIGN KEY REFERENCES Areas(AreaId),
    OfficerId   INT FOREIGN KEY REFERENCES Officers(OfficerId),
    CrimeDate   DATE          NOT NULL,
    Severity    NVARCHAR(10)  CHECK(Severity IN ('Low','Medium','High')),
    Status      NVARCHAR(20)  CHECK(Status IN ('Open','Closed','Investigating'))
                              DEFAULT 'Open',
    Description NVARCHAR(255)
);
GO

-- ═══════════════════════════════════════════════
--  TABLE 6 — Suspects
-- ═══════════════════════════════════════════════
CREATE TABLE Suspects (
    SuspectId   INT PRIMARY KEY IDENTITY(1,1),
    CrimeId     INT FOREIGN KEY REFERENCES Crimes(CrimeId),
    FullName    NVARCHAR(100) NOT NULL,
    Age         INT,
    Gender      NVARCHAR(10)  CHECK(Gender IN ('Male','Female','Other')),
    Status      NVARCHAR(20)  CHECK(Status IN ('Arrested','Wanted','Released'))
                              DEFAULT 'Wanted',
    Description NVARCHAR(255)
);
GO

-- ═══════════════════════════════════════════════
--  TABLE 7 — Reports (export history)
-- ═══════════════════════════════════════════════
CREATE TABLE Reports (
    ReportId    INT PRIMARY KEY IDENTITY(1,1),
    GeneratedBy NVARCHAR(100),
    GeneratedAt DATETIME      DEFAULT GETDATE(),
    ReportType  NVARCHAR(50),
    FilePath    NVARCHAR(255)
);
GO

-- ═══════════════════════════════════════════════
--  INSERT USERS
--  Admin and normal users
-- ═══════════════════════════════════════════════
INSERT INTO Users (Username, Password, Role, FullName) VALUES
('admin',   'admin123',  'Admin', 'System Administrator'),
('user1',   'user123',   'User',  'Ali Hassan'),
('user2',   'user123',   'User',  'Sara Khan');
GO

-- ═══════════════════════════════════════════════
--  INSERT AREAS
-- ═══════════════════════════════════════════════
INSERT INTO Areas (AreaName, City) VALUES
('Cant',           'Abbottabad'),
('Jinnahabad',     'Abbottabad'),
('Nawan Shehr',    'Abbottabad'),
('Haripur City',   'Haripur'),
('Ghazi',          'Haripur'),
('Hattar',         'Haripur'),
('Mansehra City',  'Mansehra'),
('Oghi',           'Mansehra'),
('Balakot',        'Mansehra');
GO

-- ═══════════════════════════════════════════════
--  INSERT CRIME TYPES
-- ═══════════════════════════════════════════════
INSERT INTO CrimeTypes (TypeName, Category) VALUES
('Robbery',     'Theft'),
('Assault',     'Violence'),
('Burglary',    'Theft'),
('Car Theft',   'Theft'),
('Fraud',       'Financial'),
('Kidnapping',  'Violence');
GO

-- ═══════════════════════════════════════════════
--  INSERT OFFICERS
-- ═══════════════════════════════════════════════
INSERT INTO Officers (FullName, BadgeNumber, Rank, City, Phone) VALUES
('Inspector Ali',     'KPK001', 'Inspector',     'Abbottabad', '0300-1111111'),
('DSP Ahmed',         'KPK002', 'DSP',           'Abbottabad', '0300-2222222'),
('Inspector Sara',    'KPK003', 'Inspector',     'Haripur',    '0300-3333333'),
('Constable Bilal',   'KPK004', 'Constable',     'Haripur',    '0300-4444444'),
('Inspector Usman',   'KPK005', 'Inspector',     'Mansehra',   '0300-5555555'),
('DSP Fatima',        'KPK006', 'DSP',           'Mansehra',   '0300-6666666');
GO

-- ═══════════════════════════════════════════════
--  INSERT 500 SAMPLE CRIMES
-- ═══════════════════════════════════════════════
DECLARE @i INT = 1;

WHILE @i <= 500
BEGIN
    INSERT INTO Crimes (TypeId, AreaId, OfficerId, CrimeDate,
                        Severity, Status, Description)
    VALUES (
        (ABS(CHECKSUM(NEWID())) % 6) + 1,
        (ABS(CHECKSUM(NEWID())) % 9) + 1,
        (ABS(CHECKSUM(NEWID())) % 6) + 1,
        DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 365), '2024-12-31'),
        CASE (ABS(CHECKSUM(NEWID())) % 3)
            WHEN 0 THEN 'Low'
            WHEN 1 THEN 'Medium'
            ELSE        'High'
        END,
        CASE (ABS(CHECKSUM(NEWID())) % 3)
            WHEN 0 THEN 'Open'
            WHEN 1 THEN 'Closed'
            ELSE        'Investigating'
        END,
        'Reported crime incident #' + CAST(@i AS NVARCHAR)
    );
    SET @i = @i + 1;
END;
GO

-- ═══════════════════════════════════════════════
--  INSERT SAMPLE SUSPECTS
-- ═══════════════════════════════════════════════
DECLARE @j INT = 1;

WHILE @j <= 200
BEGIN
    INSERT INTO Suspects (CrimeId, FullName, Age, Gender, Status, Description)
    VALUES (
        (ABS(CHECKSUM(NEWID())) % 500) + 1,
        'Suspect ' + CAST(@j AS NVARCHAR),
        (ABS(CHECKSUM(NEWID())) % 40) + 18,
        CASE (ABS(CHECKSUM(NEWID())) % 2)
            WHEN 0 THEN 'Male'
            ELSE        'Female'
        END,
        CASE (ABS(CHECKSUM(NEWID())) % 3)
            WHEN 0 THEN 'Arrested'
            WHEN 1 THEN 'Wanted'
            ELSE        'Released'
        END,
        'Suspect description #' + CAST(@j AS NVARCHAR)
    );
    SET @j = @j + 1;
END;
GO

-- ═══════════════════════════════════════════════
--  VERIFY ALL DATA
-- ═══════════════════════════════════════════════
SELECT COUNT(*) AS TotalUsers    FROM Users;
SELECT COUNT(*) AS TotalAreas    FROM Areas;
SELECT COUNT(*) AS TotalTypes    FROM CrimeTypes;
SELECT COUNT(*) AS TotalOfficers FROM Officers;
SELECT COUNT(*) AS TotalCrimes   FROM Crimes;
SELECT COUNT(*) AS TotalSuspects FROM Suspects;
GO