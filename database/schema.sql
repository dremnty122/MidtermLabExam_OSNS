-- Campus Event Management System | SQL Server (T-SQL) | 3NF
-- Run in an empty database. Drop order is reverse of dependency.

IF OBJECT_ID('dbo.Registrations','U') IS NOT NULL DROP TABLE dbo.Registrations;
IF OBJECT_ID('dbo.Events','U')        IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Venues','U')        IS NOT NULL DROP TABLE dbo.Venues;
IF OBJECT_ID('dbo.Users','U')         IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users (
    UserId       INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
    FullName     NVARCHAR(150) NOT NULL,
    Email        NVARCHAR(254) NOT NULL,
    Role         VARCHAR(10)   NOT NULL CONSTRAINT DF_Users_Role DEFAULT 'Student',
    CreatedAt    DATETIME2(0)  NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role  CHECK (Role IN ('Student','Admin')),
    CONSTRAINT CK_Users_Email CHECK (Email LIKE '%_@univ.edu.ph')
);

CREATE TABLE dbo.Venues (
    VenueId   INT IDENTITY(1,1) CONSTRAINT PK_Venues PRIMARY KEY,
    Name      NVARCHAR(100) NOT NULL,
    Capacity  INT           NOT NULL,
    CONSTRAINT UQ_Venues_Name UNIQUE (Name),
    CONSTRAINT CK_Venues_Capacity CHECK (Capacity > 0)
);

CREATE TABLE dbo.Events (
    EventId      INT IDENTITY(1,1) CONSTRAINT PK_Events PRIMARY KEY,
    Title        NVARCHAR(200)  NOT NULL,
    Description  NVARCHAR(2000) NULL,
    VenueId      INT            NOT NULL,
    OrganizerId  INT            NOT NULL,   -- an Admin user
    StartsAt     DATETIME2(0)   NOT NULL,
    EndsAt       DATETIME2(0)   NOT NULL,
    SeatLimit    INT            NOT NULL,
    CONSTRAINT FK_Events_Venues FOREIGN KEY (VenueId)     REFERENCES dbo.Venues(VenueId) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_Events_Users  FOREIGN KEY (OrganizerId) REFERENCES dbo.Users(UserId)  ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT CK_Events_Dates     CHECK (EndsAt > StartsAt),
    CONSTRAINT CK_Events_SeatLimit CHECK (SeatLimit > 0)
);

CREATE TABLE dbo.Registrations (
    RegistrationId INT IDENTITY(1,1) CONSTRAINT PK_Registrations PRIMARY KEY,
    EventId        INT          NOT NULL,
    UserId         INT          NOT NULL,
    Status         VARCHAR(10)  NOT NULL CONSTRAINT DF_Reg_Status DEFAULT 'Confirmed',
    RegisteredAt   DATETIME2(0) NOT NULL CONSTRAINT DF_Reg_At DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Reg_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId) ON DELETE CASCADE   ON UPDATE NO ACTION,
    CONSTRAINT FK_Reg_Users  FOREIGN KEY (UserId)  REFERENCES dbo.Users(UserId)   ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT UQ_Reg_Event_User UNIQUE (EventId, UserId),
    CONSTRAINT CK_Reg_Status CHECK (Status IN ('Confirmed','Cancelled'))
);
GO

-- Non-clustered indexes on every foreign key column
CREATE NONCLUSTERED INDEX IX_Events_VenueId        ON dbo.Events (VenueId);
CREATE NONCLUSTERED INDEX IX_Events_OrganizerId    ON dbo.Events (OrganizerId);
CREATE NONCLUSTERED INDEX IX_Registrations_EventId ON dbo.Registrations (EventId);
CREATE NONCLUSTERED INDEX IX_Registrations_UserId  ON dbo.Registrations (UserId);
GO

-- Sample data
INSERT dbo.Users (FullName, Email, Role) VALUES
 (N'Admin One', 'admin@univ.edu.ph', 'Admin'),
 (N'Juan Dela Cruz', 'juan.delacruz@univ.edu.ph', 'Student');
INSERT dbo.Venues (Name, Capacity) VALUES (N'Computer Lab 2', 30), (N'Lecture Hall A', 120);
INSERT dbo.Events (Title, VenueId, OrganizerId, StartsAt, EndsAt, SeatLimit)
 VALUES (N'Intro to Generative AI Workshop', 1, 1, '2026-10-14 13:00', '2026-10-14 16:00', 30);
INSERT dbo.Registrations (EventId, UserId) VALUES (1, 2);
GO
