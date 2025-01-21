IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Schedule] (
    [id] int NOT NULL IDENTITY,
    [sport] nvarchar(255) NULL,
    [season] nvarchar(255) NULL,
    CONSTRAINT [PRIMARY] PRIMARY KEY ([id])
);
GO

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

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Game] (
    [id] int NOT NULL IDENTITY,
    [scheduleId] int NOT NULL,
    [opponent] nvarchar(255) NULL,
    [gameDate] date NULL,
    [gameStart] time NULL,
    [venue] nvarchar(255) NULL,
    [isFinalized] bit NOT NULL,
    CONSTRAINT [PRIMARY] PRIMARY KEY ([id]),
    CONSTRAINT [Game_ibfk_1] FOREIGN KEY ([scheduleId]) REFERENCES [Schedule] ([id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] int NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Notification] (
    [id] int NOT NULL IDENTITY,
    [userId] int NULL,
    [title] nvarchar(255) NULL,
    [content] nvarchar(255) NULL,
    [date] datetime NULL,
    CONSTRAINT [PRIMARY] PRIMARY KEY ([id]),
    CONSTRAINT [Notification_ibfk_1] FOREIGN KEY ([userId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserQualifiedPositions] (
    [userId] int NOT NULL,
    [position] nvarchar(450) NOT NULL,
    CONSTRAINT [PRIMARY] PRIMARY KEY ([userId], [position]),
    CONSTRAINT [UserQualifiedPositions_ibfk_1] FOREIGN KEY ([userId]) REFERENCES [User] ([Id])
);
GO

CREATE TABLE [Availability] (
    [userId] int NOT NULL,
    [gameId] int NOT NULL,
    [open] int NOT NULL,
    [comment] nvarchar(255) NULL,
    CONSTRAINT [PRIMARY] PRIMARY KEY ([userId], [gameId]),
    CONSTRAINT [Availability_ibfk_1] FOREIGN KEY ([userId]) REFERENCES [User] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [Availability_ibfk_2] FOREIGN KEY ([gameId]) REFERENCES [Game] ([id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CrewedUser] (
    [userId] int NOT NULL,
    [gameId] int NOT NULL,
    [crewedPosition] nvarchar(255) NOT NULL,
    [arrivalTime] time NULL,
    CONSTRAINT [PRIMARY] PRIMARY KEY ([userId], [gameId], [crewedPosition]),
    CONSTRAINT [CrewedUser_ibfk_1] FOREIGN KEY ([userId]) REFERENCES [User] ([Id]),
    CONSTRAINT [CrewedUser_ibfk_2] FOREIGN KEY ([gameId]) REFERENCES [Game] ([id])
);
GO

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

CREATE INDEX [gameId] ON [Availability] ([gameId]);
GO

CREATE INDEX [gameId] ON [CrewedUser] ([gameId]);
GO

CREATE INDEX [scheduleId] ON [Game] ([scheduleId]);
GO

CREATE INDEX [userId] ON [Notification] ([userId]);
GO

CREATE INDEX [EmailIndex] ON [User] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [User] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250121193435_Initial', N'8.0.10');
GO

COMMIT;
GO

