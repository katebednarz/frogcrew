CREATE DATABASE frogcrew;
Go

USE frogcrew;

-- User Table
CREATE TABLE [User] (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email NVARCHAR(255) NOT NULL,
    password NVARCHAR(255) NOT NULL,
    phoneNumber NVARCHAR(10) NOT NULL,
    firstName NVARCHAR(255) NOT NULL,
    lastName NVARCHAR(255) NOT NULL,
    role NVARCHAR(100) NOT NULL,
    payRate NVARCHAR(100)
);

-- User Qualified Positions
CREATE TABLE UserQualifiedPositions (
    userId INT,
    position NVARCHAR(255),
    PRIMARY KEY (userId, position),
    FOREIGN KEY (userId) REFERENCES [User](id)
);

-- Schedule Table
CREATE TABLE Schedule (
    id INT IDENTITY(1,1) PRIMARY KEY,
    sport NVARCHAR(255),
    season NVARCHAR(255)
);

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

-- Notification Table
CREATE TABLE Notification (
    id INT IDENTITY(1,1) PRIMARY KEY,
    userId INT,
    title VARCHAR(255),
    content VARCHAR(255),
    date DATETIME,
    FOREIGN KEY (userId) REFERENCES [User](id) ON DELETE CASCADE
);

-- User Test Values
INSERT INTO [User] (email, password, phoneNumber, firstName, lastName, [role], payRate)
VALUES
    ('kate.bednarz@tcu.edu', 'swiftie4lyfe', '8067817554', 'Kate', 'Bednarz', 'STUDENT', NULL),
    ('dave.park@tcu.edu', 'awsom3sauce', '1234567890', 'Dave', 'Park', 'FREELANCER', '255'),
    ('aliya.suri@tcu.edu', '1<3Coffee', '1112223333', 'Aliya', 'Suri', 'STUDENT', NULL),
    ('michala.rogers@tcu.edu', 'superfrog', '1231231234', 'Michala', 'Rogers', 'STUDENT', NULL),
    ('james.clark@tcu.edu', 'h0ck3y4lif3', '3213214321', 'James', 'Clarke', 'FREELANCER', '265'),
    ('james.edmonson@tcu.edu', 'password', '9876543210', 'James', 'Edmonson', 'FREELANCER', '270'),
    ('manuel.burciaga@tcu.edu', 'wordpass', '9876543210', 'Manny', 'Burciaga', 'FREELANCER', '260'),
    ('m.martin@tcu.edu', 'admin', '9876543210', 'Mike', 'Martin', 'ADMIN', NULL);

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