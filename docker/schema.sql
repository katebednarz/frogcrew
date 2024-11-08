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
    arrivalTime DATETIME,
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
    (1,'ASSISTANT_PRODUCER'),
    (1,'DIRECTOR'),
    (1,'ASSISTANT_DIRECTOR'),
    (2,'TECHNICAL_DIRECTOR'),
    (2,'GRAPHICS_OPERATOR'),
    (3,'BUG_OPERATOR'),
    (3,'EVS_REPLAY_LEAD'),
    (3,'EVS_REPLAY_R/O'),
    (4,'VIDEO_OPERATOR'),
    (4,'EIC'),
    (4,'ENG_2'),
    (5,'AUDIO_A1'),
    (5,'AUDIO_ASSISTANT_A2'),
    (5,'CAMERA_FIXED'),
    (6,'CAMERA_FIXED'),
    (7,'CAMERA_FIXED'),
    (5,'CAMERA_HANDHELD'),
    (6,'CAMERA_HANDHELD'),
    (7,'CAMERA_HANDHELD'),
    (6,'CAMERA_STEADICAM'),
    (7,'CAMERA_STEADICAM'),
    (7,'UTILITY'),
    (7,'TIME_OUT_COORDINATOR');

-- Schedule Values
INSERT INTO Schedule (sport,season) VALUES
    ('Football', 2024),
    ('Women''s Basketball', 2024);

-- Game Values
INSERT INTO Game (scheduleId, opponent, gameDate, gameStart, venue, isFinalized) VALUES
    (1, 'Texas Tech', '2024-10-26', '14:30:00', 'Carter', true),
    (1, 'Baylor', '2024-11-9', null, 'Carter', false),
    (1, 'Arizona', '2024-11-23', null, 'Carter', false),
    (2, 'Houston Christian', '2024-11-5', '18:30:00', 'Schollmaier', false),
    (2, 'New Orleans', '2024-11-10', '14:00:00', 'Schollmaier', false),
    (2, 'Texas State', '2024-11-13', '18:30:00', 'Schollmaier', false),
    (2, 'NC State', '2024-11-17', '14:00:00', 'Schollmaier', false),
    (2, 'Incarnate Word', '2024-11-21', '12:00:00', 'Schollmaier', false),
    (2, 'Idaho State', '2024-11-17', '14:00:00', 'Schollmaier', false),
    (2, 'Florida Atlantic', '2024-11-17', '14:00:00', 'Schollmaier', false);

-- CrewedUser Values
INSERT INTO CrewedUser VALUES
    (1, 1, 'PRODUCER', null),
    (2, 1, 'TECHNICAL_DIRECTOR', null),
    (3, 1, 'EVS_REPLAY_LEAD', null),
    (4, 1, 'VIDEO_OPERATOR', null),
    (5, 1, 'CAMERA_FIXED', null),
    (6, 1, 'CAMERA_HANDHELD', null),
    (7, 1, 'CAMERA_STEADICAM', null);

-- Availability Values
INSERT INTO Availability VALUES
    (1, 1, true, null),
    (1, 2, true, null),
    (1, 3, false, null);

-- Notification Values
INSERT INTO Notification (userId, title, content, date) VALUES
    (1, 'Game Scheduled', 'You have been scheduled to work the game', '2024-11-10 14:00:00');