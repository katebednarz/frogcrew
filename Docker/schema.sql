DROP DATABASE IF EXISTS frogcrew;

CREATE DATABASE frogcrew;

USE frogcrew;

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
    gameStart DATETIME,
    venue VARCHAR(255),
    isFinalized BOOLEAN,
    FOREIGN KEY (scheduleId) REFERENCES Schedule(id)
);

-- Schedule_Game Relationship Table (to map games to schedules)
CREATE TABLE Schedule_Game (
    scheduleId INT,
    gameId INT,
    PRIMARY KEY (scheduleId, gameId),
    FOREIGN KEY (scheduleId) REFERENCES Schedule(id) ON DELETE CASCADE,
    FOREIGN KEY (gameId) REFERENCES Game(id) ON DELETE CASCADE
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
    open BOOLEAN,
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

INSERT INTO User (email,password,phoneNumber,firstName,lastName,role,payRate) VALUES
    ('kate.bednarz@tcu.edu', 'swiftie4lyfe', '8067817554', 'Kate', 'Bednarz', 'STUDENT', null)