DROP DATABASE IF EXISTS frogcrew;

CREATE DATABASE frogcrew;

USE frogcrew;

-- Things of Note:
-- 1. should report time in crewed positon be there
-- 2. game has a schedule id, but the schedule_game table already has that connection

-- User Table
CREATE TABLE User (
    id INT AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL,
    phoneNumber VARCHAR(10),
    firstName VARCHAR(255),
    lastName VARCHAR(255),
    role VARCHAR(100),
    payRate VARCHAR(100)
);

-- User Qualified Positions
CREATE TABLE UserQualifiedPositions (
    userId INT,
    position VARCHAR(255),
    PRIMARY KEY (userId, position),
    FOREIGN KEY (userId) REFERENCES User(id)
);

-- Schedule Table
CREATE TABLE Schedule (
    id INT AUTO_INCREMENT PRIMARY KEY,
    sport VARCHAR(255),
    season VARCHAR(255)
);

-- Game Table
CREATE TABLE Game (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scheduleId INT,
    opponent VARCHAR(255),
    gameDate DATE,
    gameStart TIME,
    venue VARCHAR(255),
    isFinalized BOOLEAN,
    FOREIGN KEY (scheduleId) REFERENCES Schedule(id)
);

CREATE TABLE CrewedUser (
    userId INT,
    gameId INT,
    crewedPosition VARCHAR(255),
    arrivalTime TIME,
    PRIMARY KEY (userId, gameId),
    FOREIGN KEY (userId) REFERENCES User(id),
    FOREIGN KEY (gameId) REFERENCES Game(id)
);

-- Availability Table
CREATE TABLE Availability (
    userId INT,
    gameId INT,
    open BOOLEAN NOT NULL,
    comment VARCHAR(255),
    PRIMARY KEY (userId, gameId),
    FOREIGN KEY (userId) REFERENCES User(id) ON DELETE CASCADE,
    FOREIGN KEY (gameId) REFERENCES Game(id) ON DELETE CASCADE
);

-- Notification Table
CREATE TABLE Notification (
    id INT AUTO_INCREMENT PRIMARY KEY,
    userId INT,
    title VARCHAR(255),
    content VARCHAR(255),
    date DATETIME,
    FOREIGN KEY (userId) REFERENCES User(id) ON DELETE CASCADE
);

-- User Test Values
INSERT INTO User (email,password,phoneNumber,firstName,lastName,role,payRate) VALUES
    ('kate.bednarz@tcu.edu', 'swiftie4lyfe', '8067817554', 'Kate', 'Bednarz', 'STUDENT', null),
    ('dave.park@tcu.edu', 'awsom3sauce', '1234567890', 'Dave', 'Park', 'FREELANCER','255'),
    ('aliya.suri@tcu.edu', '1<3Coffee', '1112223333', 'Aliya', 'Suri', 'STUDENT', null),
    ('michala.rogers@tcu.edu', 'superfrog', '1231231234', 'Michala', 'Rogers', 'STUDENT', null),
    ('james.clark@tcu.edu', 'h0ck3y4lif3', '3213214321', 'James', 'Clarke', 'FREELANCER', '265'),
    ('james.edmonson@tcu.edu', 'password', '9876543210', 'James', 'Edmonson', 'FREELANCER', '270'),
    ('manuel.burciaga@tcu.edu', 'wordpass', '9876543210', 'Manny', 'Burciaga', 'FREELANCER', '260');

-- UserQualifiedPositions Values
INSERT INTO UserQualifiedPositions VALUES
    (1,'PRODUCER'),
    (1,'ASST PROD'),
    (1,'DIRECTOR'),
    (1,'ASST DIRECTOR'),
    (2,'TECHNICAL DIR'),
    (2,'GRAPHICS'),
    (3,'BUG OP'),
    (3,'REPLAY EVS'),
    (4,'VIDEO'),
    (4,'EIC'),
    (4,'2ND ENG'),
    (5,'AUDIO'),
    (5,'CAMERA'),
    (6,'CAMERA'),
    (7,'CAMERA'),
    (7,'UTILITY');

-- Schedule Values
INSERT INTO Schedule (sport,season) VALUES
    ('Football', '2024-2025'),
    ('Women''s Basketball', '2024-2025'),
    ('Men''s Basketball', '2024-2025'),
    ('Baseball', '2024-2025'),
    ('Beach Volleyball', '2024-2025'),
    ('Soccer', '2024-2025');


-- Game Values
-- Football
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (1, 'LIU', '2024-09-07', '19:00:00', 'Carter', false),
    (1, 'UCF', '2024-09-14', '18:30:00', 'Carter', false),
    (1, 'Houston', '2024-10-04', '18:30:00', 'Carter', false),
    (1, 'Texas Tech', '2024-10-26', '14:30:00', 'Carter', false),
    (1, 'Oklahoma State', '2024-11-09', '18:00:00', 'Carter', false),
    (1, 'Arizona', '2024-11-23', '14:00:00', 'Carter', false);
-- Women's Basketball
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (2, 'Houston Christian', '2024-11-05', '18:30:00', 'Schollmaier Arena', false),
    (2, 'New Orleans', '2024-11-10', '14:00:00', 'Schollmaier Arena', false),
    (2, 'Texas State', '2024-11-13', '18:30:00', 'Schollmaier Arena', false),
    (2, 'NC State', '2024-11-17', '14:00:00', 'Schollmaier Arena', false),
    (2, 'Incarnate Word', '2024-11-21', '12:00:00', 'Schollmaier Arena', false),
    (2, 'Idaho State', '2024-11-24', '16:00:00', 'Schollmaier Arena', false),
    (2, 'Florida Atlantic', '2024-12-04', '18:30:00', 'Schollmaier Arena', false),
    (2, 'Louisiana Tech', '2024-12-15', '14:00:00', 'Schollmaier Arena', false),
    (2, 'Samford', '2024-12-17', '18:30:00', 'Schollmaier Arena', false),
    (2, 'Brown', '2024-12-29', '14:00:00', 'Schollmaier Arena', false),
    (2, 'Colorado', '2025-01-01', '18:30:00', 'Schollmaier Arena', false),
    (2, 'Cincinnati', '2025-01-04', '19:00:00', 'Schollmaier Arena', false),
    (2, 'UCF', '2025-01-14', '18:30:00', 'Schollmaier Arena', false),
    (2, 'Utah', '2025-01-17', '18:30:00', 'Schollmaier Arena', false),
    (2, 'Baylor', '2025-01-26', '14:00:00', 'Schollmaier Arena', false),
    (2, 'Texas Tech', '2025-02-08', '16:00:00', 'Schollmaier Arena', false),
    (2, 'BYU', '2025-02-11', '18:30:00', 'Schollmaier Arena', false),
    (2, 'West Virginia', '2025-02-23', '11:00:00', 'Schollmaier Arena', false),
    (2, 'Houston', '2025-02-26', '18:30:00', 'Schollmaier Arena', false);
-- Men's Basketball
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (3, 'Florida A&M', '2024-11-04', '19:00:00', 'Schollmaier Arena', False),
    (3, 'Florida Gulf Coast', '2024-11-08', '19:00:00', 'Schollmaier Arena', False),
    (3, 'Texas State', '2024-11-12', '19:00:00', 'Schollmaier Arena', False),
    (3, 'Alcorn State', '2024-11-19', '19:00:00', 'Schollmaier Arena', False),
    (3, 'Xavier', '2024-12-05', '19:00:00', 'Schollmaier Arena', False),
    (3, 'South Alabama', '2024-12-16', '19:00:00', 'Schollmaier Arena', False),
    (3, 'Montana State', '2024-12-22', '13:00:00', 'Schollmaier Arena', False),
    (3, 'Kansas State', '2025-01-04', '15:00:00', 'Schollmaier Arena', False),
    (3, 'BYU', '2025-01-11', '13:00:00', 'Schollmaier Arena', False),
    (3, 'Utah', '2025-01-15', '19:00:00', 'Schollmaier Arena', False),
    (3, 'Kansas', '2025-01-22', '18:00:00', 'Schollmaier Arena', False),
    (3, 'Colorado', '2025-02-02', '15:00:00', 'Schollmaier Arena', False),
    (3, 'West Virginia', '2025-02-05', '19:00:00', 'Schollmaier Arena', False),
    (3, 'Oklahoma State', '2025-02-12', '18:00:00', 'Schollmaier Arena', False),
    (3, 'Texas Tech', '2025-02-18', '19:00:00', 'Schollmaier Arena', False),
    (3, 'UCF', '2025-03-01', '15:00:00', 'Schollmaier Arena', False),
    (3, 'Baylor', '2025-03-04', '19:00:00', 'Schollmaier Arena', False);
-- Baseball
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (4, 'Tarleton State', '2025-02-25', '18:00:00', 'Lupton Stadium', False),
    (4, 'Southern Miss', '2025-02-28', '18:00:00', 'Lupton Stadium', False),
    (4, 'Southern Miss', '2025-03-01', '14:00:00', 'Lupton Stadium', False),
    (4, 'Southern Miss', '2025-03-02', '13:00:00', 'Lupton Stadium', False),
    (4, 'Air Force', '2025-03-04', '18:00:00', 'Lupton Stadium', False),
    (4, 'Air Force', '2025-03-05', '18:00:00', 'Lupton Stadium', False),
    (4, 'Fresno State', '2025-03-07', '18:00:00', 'Lupton Stadium', False),
    (4, 'Fresno State', '2025-03-08', '14:00:00', 'Lupton Stadium', False),
    (4, 'Fresno State', '2025-03-09', '13:00:00', 'Lupton Stadium', False),
    (4, 'Arizona State', '2025-03-14', '18:00:00', 'Lupton Stadium', False),
    (4, 'Arizona State', '2025-03-15', '14:00:00', 'Lupton Stadium', False),
    (4, 'Arizona State', '2025-03-16', '13:00:00', 'Lupton Stadium', False),
    (4, 'UTRGV', '2025-03-25', '18:00:00', 'Lupton Stadium', False),
    (4, 'UTRGV', '2025-03-26', '18:00:00', 'Lupton Stadium', False),
    (4, 'UTSA', '2025-04-01', '18:00:00', 'Lupton Stadium', False),
    (4, 'BYU', '2025-04-03', '18:00:00', 'Lupton Stadium', False),
    (4, 'BYU', '2025-04-04', '18:00:00', 'Lupton Stadium', False),
    (4, 'BYU', '2025-04-05', '14:00:00', 'Lupton Stadium', False),
    (4, 'Kansas', '2025-04-11', '18:00:00', 'Lupton Stadium', False),
    (4, 'Kansas', '2025-04-12', '14:00:00', 'Lupton Stadium', False),
    (4, 'Kansas', '2025-04-13', '13:00:00', 'Lupton Stadium', False),
    (4, 'Dallas Baptist', '2025-04-15', '18:00:00', 'Lupton Stadium', False),
    (4, 'Baylor', '2025-04-25', '18:00:00', 'Lupton Stadium', False),
    (4, 'Baylor', '2025-04-26', '14:00:00', 'Lupton Stadium', False),
    (4, 'Baylor', '2025-04-27', '13:00:00', 'Lupton Stadium', False),
    (4, 'UT Arlington', '2025-04-29', '18:00:00', 'Lupton Stadium', False),
    (4, 'Cincinnati', '2025-05-09', '18:00:00', 'Lupton Stadium', False),
    (4, 'Cincinnati', '2025-05-10', '14:00:00', 'Lupton Stadium', False),
    (4, 'Cincinnati', '2025-05-11', '13:00:00', 'Lupton Stadium', False);
-- Beach Volleyball
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (5, 'TCU Fall Challenge', '2024-10-03', '08:00:00', 'TCU Beach Volleyball Courts', False),
    (5, 'TCU Fall Challenge', '2024-10-04', '08:00:00', 'TCU Beach Volleyball Courts', False),
    (5, 'Purple vs. White Scrimmage', '2025-03-01', null, 'TCU Beach Volleyball Courts', False),
    (5, 'Big 12 Conference Championships', '2025-04-23', null, 'TCU Beach Volleyball Courts', False);
-- Soccer
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (6, 'Rice (Exhibition)', '2024-08-06', '19:00:00', 'Fort Worth, Texas', False),
    (6, 'Alabama', '2024-08-15', '19:00:00', 'Fort Worth, Texas', False),
    (6, 'Central Michigan', '2024-08-29', '19:00:00', 'Fort Worth, Texas', False),
    (6, 'Texas Tech', '2024-09-12', '19:00:00', 'Fort Worth, Texas', False),
    (6, 'Cal Poly', '2024-09-15', '13:00:00', 'Fort Worth, Texas', False),
    (6, 'BYU', '2024-09-19', '19:00:00', 'Fort Worth, Texas', False),
    (6, 'Arizona', '2024-09-22', '13:00:00', 'Fort Worth, Texas', False),
    (6, 'West Virginia', '2024-10-10', '19:00:00', 'Fort Worth, Texas', False),
    (6, 'Arizona State', '2024-10-13', '13:00:00', 'Fort Worth, Texas', False),
    (6, 'Stephen F. Austin', '2024-11-15', '19:00:00', 'Fort Worth, Texas', False);
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
    (1, 1, true, null),
    (1, 2, true, null),
    (1, 3, false, null);

-- Notification Values
INSERT INTO Notification (userId, title, content, date) VALUES
    (1, 'Game Scheduled', 'You have been scheduled to work the game', '2024-11-10 14:00:00');