CREATE DATABASE frogcrew;
Go

USE frogcrew;

SET QUOTED_IDENTIFIER ON;

-- User Table
CREATE TABLE [User] (
    [Id] int NOT NULL IDENTITY,
    [FirstName] nvarchar(50) NULL,
    [LastName] nvarchar(50) NULL,
    [PayRate] nvarchar(25) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY ([Id])
);
GO

-- User Qualified Positions
CREATE TABLE UserQualifiedPositions (
    userId INT,
    position NVARCHAR(255),
    PRIMARY KEY (userId, position),
    FOREIGN KEY (userId) REFERENCES [User](id)
);
GO

-- Schedule Table
CREATE TABLE Schedule (
    id INT IDENTITY(1,1) PRIMARY KEY,
    sport NVARCHAR(255),
    season NVARCHAR(255)
);
GO

-- Game Table
CREATE TABLE Game (
    id INT IDENTITY(1,1) PRIMARY KEY,
    scheduleId INT,
    opponent NVARCHAR(255),
    gameDate DATE,
    gameStart TIME,
    venue NVARCHAR(255),
    isFinalized BIT
    FOREIGN KEY (scheduleId) REFERENCES Schedule(id)
);
GO

-- CrewedUser Table
CREATE TABLE CrewedUser (
    userId INT,
    gameId INT,
    crewedPosition VARCHAR(255),
    arrivalTime TIME,
    PRIMARY KEY (userId, gameId, crewedPosition),
    FOREIGN KEY (userId) REFERENCES [User](id),
    FOREIGN KEY (gameId) REFERENCES Game(id)
);
GO

-- Availability Table
CREATE TABLE Availability (
    userId INT,
    gameId INT,
    [open] BIT NOT NULL,
    comment VARCHAR(255),
    PRIMARY KEY (userId, gameId),
    FOREIGN KEY (userId) REFERENCES [User](id) ON DELETE CASCADE,
    FOREIGN KEY (gameId) REFERENCES Game(id) ON DELETE CASCADE
);
GO

-- Notification Table
CREATE TABLE Notification (
    id INT IDENTITY(1,1) PRIMARY KEY,
    userId INT,
    title VARCHAR(255),
    content VARCHAR(255),
    date DATETIME,
    FOREIGN KEY (userId) REFERENCES [User](id) ON DELETE CASCADE
);
GO

-- AspNetRoles Table
CREATE TABLE [AspNetRoles] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

-- AspNetRoleClaims Table
CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUserClaims
CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUserLogins
CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUserRoles
CREATE TABLE [AspNetUserRoles] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUserTokens
CREATE TABLE [AspNetUserTokens] (
    [UserId] int NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);
GO

-- Invitations Table
CREATE TABLE Invitations (
    [InviteToken] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_Invitations] PRIMARY KEY ([InviteToken])
);

-- Trade Board Table
CREATE TABLE TradeBoard (
    [DropperID] int NOT NULL,
    [GameId] int NOT NULL,
    [Position] nvarchar(255) NOT NULL,
    [Status] nvarchar(255) NOT NULL,
    [ReceiverID] int NULL,
    CONSTRAINT [PK_TradeBoard] PRIMARY KEY ([DropperID], [GameId]),
)

-- Indexes

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [User] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [User] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [UserQualifiedPositionsIndex] ON [UserQualifiedPositions] ([userId]);
GO

-- User Values
INSERT INTO [User] (FirstName, LastName, PayRate, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount) VALUES
    ('Kate','Bednarz',null,'kate.bednarz@tcu.edu','KATE.BEDNARZ@TCU.EDU','kate.bednarz@tcu.edu','KATE.BEDNARZ@TCU.EDU',0,'AQAAAAIAAYagAAAAEB4ENT4XTLogUHTairPIvQ5cclWtwIIPxmBpNf8P4TSGMkY7GHTJqZq1QBgW328HVw==','GXVIZJY7LUAZVWWUUCZBGWN5UQWC3WIO','57d2b946-1889-4a59-a819-5ce3716b5831','1234512345',0,0,null,1,0),
    ('Dave','Park',250,'dave.park@tcu.edu','DAVE.PARK@TCU.EDU','dave.park@tcu.edu','DAVE.PARK@TCU.EDU',0,'AQAAAAIAAYagAAAAEAml3Yg61kzSQ+3LbxQhtEdFOG9Cp3sVXIZ+7xhI1ATyxYvYbPWP+N+u+gMZRLbqZg==','Q5P45UEJUUNJCJCB63EKISHL32L2PM7I','65f00c08-6f90-4e4d-956c-f3aadc72c7b8','1234567890',0,0,null,1,0),
    ('Aliya','Suri',null,'aliya.suri@tcu.edu','ALIYA.SURI@TCU.EDU','aliya.suri@tcu.edu','ALIYA.SURI@TCU.EDU',0,'AQAAAAIAAYagAAAAECrLmLFEaCNQ+QpJEszAw2hTF4QLXA1EKhvoehBZiAYJ7/W+4wBOZsziSvTivqoSkg==','NYSH3MURGPGRAMISHNCR7EZCJDJEPU5V','c7d729fb-454a-44b0-aa32-0280f55c38d1','1112223333',0,0,null,1,0),
    ('Michala','Rogers',null,'michala.rogers@tcu.edu','MICHALA.ROGERS@TCU.EDU','michala.rogers@tcu.edu','MICHALA.ROGERS@TCU.EDU',0,'AQAAAAIAAYagAAAAEGio/sQhVyYelYxUuY1JlPCgA5s4GAplUXAor85ppcCz7Su67T4fQEmgLOJ90tJkbw==','SWHDYYUFD2DZ4U7EM7KSLWUEN74FE7YQ','315646f4-6822-482d-a779-6561d894a573','1231231234',0,0,null,1,0),
    ('James','Clarke',260,'james.clarke@tcu.edu','JAMES.CLARKE@TCU.EDU','james.clarke@tcu.edu','JAMES.CLARKE@TCU.EDU',0,'AQAAAAIAAYagAAAAEPXDnLGyqSdktHAyJbHTELODH67iijhA9IngN6iddRM9nqttHtPT4ssOSp0D6r7hlA==','JBQ66N7YSGCCX2ZXOLW3RRJGZWZWCMEB','7426ce63-8636-4be9-be85-95f1aa44c076','3213214321',0,0,null,1,0),
    ('James','Edmonson',270,'james.edmonson@tcu.edu','JAMES.EDMONSON@TCU.EDU','james.edmonson@tcu.edu','JAMES.EDMONSON@TCU.EDU',0,'AQAAAAIAAYagAAAAENNjgAUa30Q3oHwNGOM/4klbGWLptXEEHtRvQcvoGwGO6M3fJWFpDE6TboWJTV/LpQ==','Z2WGIHH44OPWAYTGPWEJ4WQRF2YCT6DF','884ef31d-9ce3-48f7-a679-79092987644c','9876543210',0,0,null,1,0),
    ('Manny','Burciaga',280,'manuel.burciaga@tcu.edu','MANUEL.BURCIAGA@TCU.EDU','manuel.burciaga@tcu.edu','MANUEL.BURCIAGA@TCU.EDU',0,'AQAAAAIAAYagAAAAENlDIZ1dT6Ril7CE6IsrU+fRcO9meA9n3SsWaZS8yW4KFuv56pv/RtngmYM4mwT15A==','YQFMZNUSFLP5RINJWKP4DE7PMGGSHTCD','a02dcd8e-1523-4b24-bdaf-0f4f2a5a9eec','9876543210',0,0,null,1,0),
    ('Mike','Martin',null,'m.martin@tcu.edu','M.MARTIN@TCU.EDU','m.martin@tcu.edu','M.MARTIN@TCU.EDU',0,'AQAAAAIAAYagAAAAELchPSUc5T7AMrm2j7v31sXaKlSgL5rP9WbtJ+cCwgkVeoTfay8dsaer5zZLfis7yw==','4ZRQUMRFJR5UPEMKU4ZJ4COU44A5ONEX','cb3ffecd-392a-4caf-9355-8f67e06974c5','9876543210',0,0,null,1,0);

-- UserQualifiedPositions Values
INSERT INTO UserQualifiedPositions VALUES
    (1,'PRODUCER'),
    (2,'ASSISTANT PRODUCER'),
    (3,'DIRECTOR'),
    (4,'ASSISTANT DIRECTOR'),
    (5,'TECHNICAL DIRECTOR'),
    (6,'GRAPHICS OPERATOR'),
    (7,'BUG OPERATOR'),
    (1,'EVS REPLAY-LEAD'),
    (2,'EVS REPLAY-R/O'),
    (3,'VIDEO OPERATOR'),
    (4,'EIC'),
    (5,'ENG 2'),
    (6,'AUDIO A1'),
    (7,'AUDIO ASSISTANT A2'),
    (1,'CAMERA-FIXED'),
    (2,'CAMERA-HANDHELD'),
    (3,'CAMERA-STEADICAM'),
    (4,'UTILITY'),
    (5,'TIME OUT COORDINATOR');

-- Schedule Values
INSERT INTO Schedule (sport,season) VALUES
    ('Football', '2024-2025'),
    ('Women''s Basketball', '2024-2025'),
    ('Men''s Basketball', '2024-2025'),
    ('Baseball', '2024-2025'),
    ('Volleyball', '2024-2025'),
    ('Soccer', '2024-2025');

-- Game Values
-- Football
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (1, 'LIU', '2024-09-07', '19:00:00', 'Carter', 0),
    (1, 'UCF', '2024-09-14', '18:30:00', 'Carter', 0),
    (1, 'Houston', '2024-10-04', '18:30:00', 'Carter', 0),
    (1, 'Texas Tech', '2024-10-26', '14:30:00', 'Carter', 0),
    (1, 'Oklahoma State', '2024-11-09', '18:00:00', 'Carter', 0),
    (1, 'Arizona', '2024-11-23', '14:00:00', 'Carter', 0);
-- Women's Basketball
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (2, 'Houston Christian', '2024-11-05', '18:30:00', 'Schollmaier Arena', 0),
    (2, 'New Orleans', '2024-11-10', '14:00:00', 'Schollmaier Arena', 0),
    (2, 'Texas State', '2024-11-13', '18:30:00', 'Schollmaier Arena', 0),
    (2, 'NC State', '2024-11-17', '14:00:00', 'Schollmaier Arena', 0),
    (2, 'Incarnate Word', '2024-11-21', '12:00:00', 'Schollmaier Arena', 0),
    (2, 'Idaho State', '2024-11-24', '16:00:00', 'Schollmaier Arena', 0),
    (2, 'Florida Atlantic', '2024-12-04', '18:30:00', 'Schollmaier Arena', 0),
    (2, 'Louisiana Tech', '2024-12-15', '14:00:00', 'Schollmaier Arena', 0),
    (2, 'Samford', '2024-12-17', '18:30:00', 'Schollmaier Arena', 0),
    (2, 'Brown', '2024-12-29', '14:00:00', 'Schollmaier Arena', 0),
    (2, 'Colorado', '2025-01-01', '18:30:00', 'Schollmaier Arena', 0),
    (2, 'Cincinnati', '2025-01-04', '19:00:00', 'Schollmaier Arena', 0),
    (2, 'UCF', '2025-01-14', '18:30:00', 'Schollmaier Arena', 0),
    (2, 'Utah', '2025-01-17', '18:30:00', 'Schollmaier Arena', 0),
    (2, 'Baylor', '2025-01-26', '14:00:00', 'Schollmaier Arena', 0),
    (2, 'Texas Tech', '2025-02-08', '16:00:00', 'Schollmaier Arena', 0),
    (2, 'BYU', '2025-02-11', '18:30:00', 'Schollmaier Arena', 0),
    (2, 'West Virginia', '2025-02-23', '11:00:00', 'Schollmaier Arena', 0),
    (2, 'Houston', '2025-02-26', '18:30:00', 'Schollmaier Arena', 0);
-- Men's Basketball
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (3, 'Florida A&M', '2024-11-04', '19:00:00', 'Schollmaier Arena', 0),
    (3, 'Florida Gulf Coast', '2024-11-08', '19:00:00', 'Schollmaier Arena', 0),
    (3, 'Texas State', '2024-11-12', '19:00:00', 'Schollmaier Arena', 0),
    (3, 'Alcorn State', '2024-11-19', '19:00:00', 'Schollmaier Arena', 0),
    (3, 'Xavier', '2024-12-05', '19:00:00', 'Schollmaier Arena', 0),
    (3, 'South Alabama', '2024-12-16', '19:00:00', 'Schollmaier Arena', 0),
    (3, 'Montana State', '2024-12-22', '13:00:00', 'Schollmaier Arena', 0),
    (3, 'Kansas State', '2025-01-04', '15:00:00', 'Schollmaier Arena', 0),
    (3, 'BYU', '2025-01-11', '13:00:00', 'Schollmaier Arena', 0),
    (3, 'Utah', '2025-01-15', '19:00:00', 'Schollmaier Arena', 0),
    (3, 'Kansas', '2025-01-22', '18:00:00', 'Schollmaier Arena', 0),
    (3, 'Colorado', '2025-02-02', '15:00:00', 'Schollmaier Arena', 0),
    (3, 'West Virginia', '2025-02-05', '19:00:00', 'Schollmaier Arena', 0),
    (3, 'Oklahoma State', '2025-02-12', '18:00:00', 'Schollmaier Arena', 0),
    (3, 'Texas Tech', '2025-02-18', '19:00:00', 'Schollmaier Arena', 0),
    (3, 'UCF', '2025-03-01', '15:00:00', 'Schollmaier Arena', 0),
    (3, 'Baylor', '2025-03-04', '19:00:00', 'Schollmaier Arena', 0);
-- Baseball
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (4, 'Tarleton State', '2025-02-25', '18:00:00', 'Lupton Stadium', 0),
    (4, 'Southern Miss', '2025-02-28', '18:00:00', 'Lupton Stadium', 0),
    (4, 'Southern Miss', '2025-03-01', '14:00:00', 'Lupton Stadium', 0),
    (4, 'Southern Miss', '2025-03-02', '13:00:00', 'Lupton Stadium', 0),
    (4, 'Air Force', '2025-03-04', '18:00:00', 'Lupton Stadium', 0),
    (4, 'Air Force', '2025-03-05', '18:00:00', 'Lupton Stadium', 0),
    (4, 'Fresno State', '2025-03-07', '18:00:00', 'Lupton Stadium', 0),
    (4, 'Fresno State', '2025-03-08', '14:00:00', 'Lupton Stadium', 0),
    (4, 'Fresno State', '2025-03-09', '13:00:00', 'Lupton Stadium', 0),
    (4, 'Arizona State', '2025-03-14', '18:00:00', 'Lupton Stadium', 0),
    (4, 'Arizona State', '2025-03-15', '14:00:00', 'Lupton Stadium', 0),
    (4, 'Arizona State', '2025-03-16', '13:00:00', 'Lupton Stadium', 0),
    (4, 'UTRGV', '2025-03-25', '18:00:00', 'Lupton Stadium', 0),
    (4, 'UTRGV', '2025-03-26', '18:00:00', 'Lupton Stadium', 0),
    (4, 'UTSA', '2025-04-01', '18:00:00', 'Lupton Stadium', 0),
    (4, 'BYU', '2025-04-03', '18:00:00', 'Lupton Stadium', 0),
    (4, 'BYU', '2025-04-04', '18:00:00', 'Lupton Stadium', 0),
    (4, 'BYU', '2025-04-05', '14:00:00', 'Lupton Stadium', 0),
    (4, 'Kansas', '2025-04-11', '18:00:00', 'Lupton Stadium', 0),
    (4, 'Kansas', '2025-04-12', '14:00:00', 'Lupton Stadium', 0),
    (4, 'Kansas', '2025-04-13', '13:00:00', 'Lupton Stadium', 0),
    (4, 'Dallas Baptist', '2025-04-15', '18:00:00', 'Lupton Stadium', 0),
    (4, 'Baylor', '2025-04-25', '18:00:00', 'Lupton Stadium', 0),
    (4, 'Baylor', '2025-04-26', '14:00:00', 'Lupton Stadium', 0),
    (4, 'Baylor', '2025-04-27', '13:00:00', 'Lupton Stadium', 0),
    (4, 'UT Arlington', '2025-04-29', '18:00:00', 'Lupton Stadium', 0),
    (4, 'Cincinnati', '2025-05-09', '18:00:00', 'Lupton Stadium', 0),
    (4, 'Cincinnati', '2025-05-10', '14:00:00', 'Lupton Stadium', 0),
    (4, 'Cincinnati', '2025-05-11', '13:00:00', 'Lupton Stadium', 0);
-- Volleyball
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (5, 'Texas A&M Commerce', '2024-09-12', '13:00:00', 'Fort Worth, Texas', 0),
    (5, 'Prairie View A&M', '2024-09-12', '19:00:00', 'Fort Worth, Texas', 0),
    (5, 'UCLA', '2024-09-13', '18:30:00', 'Fort Worth, Texas', 0),
    (5, 'UT Arlington', '2024-09-17', '18:30:00', 'Fort Worth, Texas', 0),
    (5, 'Denver', '2024-09-20', '18:30:00', 'Fort Worth, Texas', 0),
    (5, 'Rice', '2024-09-21', '17:00:00', 'Fort Worth, Texas', 0),
    (5, 'Arizona State', '2024-09-25', '20:00:00', 'Fort Worth, Texas', 0),
    (5, 'Arizona', '2024-09-27', '18:30:00', 'Fort Worth, Texas', 0),
    (5, 'Baylor', '2024-10-06', '14:00:00', 'Fort Worth, Texas', 0),
    (5, 'Cincinnati', '2024-10-19', '13:00:00', 'Fort Worth, Texas', 0),
    (5, 'Colorado', '2024-10-30', '18:30:00', 'Fort Worth, Texas', 0),
    (5, 'Iowa State', '2024-11-01', '18:30:00', 'Fort Worth, Texas', 0),
    (5, 'UCF', '2024-11-07', '18:30:00', 'Fort Worth, Texas', 0),
    (5, 'Houston', '2024-11-20', '18:30:00', 'Fort Worth, Texas', 0),
    (5, 'Texas Tech', '2024-11-22', '18:30:00', 'Fort Worth, Texas', 0);
-- Soccer
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (6, 'Rice (Exhibition)', '2024-08-06', '19:00:00', 'Fort Worth, Texas', 0),
    (6, 'Alabama', '2024-08-15', '19:00:00', 'Fort Worth, Texas', 0),
    (6, 'Central Michigan', '2024-08-29', '19:00:00', 'Fort Worth, Texas', 0),
    (6, 'Texas Tech', '2024-09-12', '19:00:00', 'Fort Worth, Texas', 0),
    (6, 'Cal Poly', '2024-09-15', '13:00:00', 'Fort Worth, Texas', 0),
    (6, 'BYU', '2024-09-19', '19:00:00', 'Fort Worth, Texas', 0),
    (6, 'Arizona', '2024-09-22', '13:00:00', 'Fort Worth, Texas', 0),
    (6, 'West Virginia', '2024-10-10', '19:00:00', 'Fort Worth, Texas', 0),
    (6, 'Arizona State', '2024-10-13', '13:00:00', 'Fort Worth, Texas', 0),
    (6, 'Stephen F. Austin', '2024-11-15', '19:00:00', 'Fort Worth, Texas', 0);

-- CrewedUser Values
INSERT INTO CrewedUser VALUES
    (1, 1, 'PRODUCER', null),
    (2, 1, 'TECHNICAL DIR', null),
    (3, 1, 'REPLAY EVS 1', null),
    (4, 1, 'VIDEO', null),
    (5, 1, 'CAMERA 1', null),
    (6, 1, 'CAMERA 2', null),
    (7, 1, 'CAMERA 3', null);

-- Availability Values
INSERT INTO Availability VALUES
    (1, 1, 1, null),
    (2, 1, 1, null),
    (3, 1, 1, null),
    (4, 1, 1, null),
    (5, 1, 1, null),
    (6, 1, 1, null),
    (7, 1, 1, null);

-- Notification Values
INSERT INTO Notification (userId, title, content, date) VALUES
    (1, 'Game Scheduled', 'You have been scheduled to work the game', '2024-11-10 14:00:00');

-- AspNetRoles Values
INSERT INTO AspNetRoles (Name, NormalizedName, ConcurrencyStamp) VALUES
    ('ADMIN','ADMIN',null),
    ('STUDENT','STUDENT',null),
    ('FREELANCER','FREELANCER',null);

-- AspNetUserRoles Values
INSERT INTO AspNetUserRoles VALUES
    (1,2),
    (2,3),
    (3,2),
    (4,2),
    (5,3),
    (6,3),
    (7,3),
    (8,1);