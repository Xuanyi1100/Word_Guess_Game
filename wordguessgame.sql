create database WordGuessGame;
use WordGuessGame;

-- User Table
CREATE TABLE User (
    UserID INT PRIMARY KEY AUTO_INCREMENT,
    UserName VARCHAR(255) NOT NULL UNIQUE,
    UserIP VARCHAR(45) NOT NULL,
    UserPort INT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Session Table
CREATE TABLE GameSession (
	GameSessionID INT PRIMARY KEY AUTO_INCREMENT,
    SessionID VARCHAR(255) NOT NULL,
    UserID INT NOT NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME,
    Status ENUM('win', 'quit', 'lose', 'active') NOT NULL,
    GameStringID INT NOT NULL,
    WordsToFound INT DEFAULT 0 CHECK (WordsToFound >= 0),
    TotalWords INT DEFAULT 0,
    CONSTRAINT FK_Session_UserID_User_UserID FOREIGN KEY (UserID) REFERENCES User(UserID),
    CONSTRAINT FK_Session_GameStringID_GameString_GameStringID FOREIGN KEY (GameStringID) REFERENCES GameString(GameStringID)
);


-- GameWord Table
CREATE TABLE GameWord (
    GameWordID INT PRIMARY KEY AUTO_INCREMENT,
    GameWordText VARCHAR(255) NOT NULL
);

-- GameString Table
CREATE TABLE GameString (
    GameStringID INT PRIMARY KEY AUTO_INCREMENT,
    GameStringText CHAR(80) NOT NULL
);

-- SpeedRecord Table
CREATE TABLE SpeedRecord (
    SpeedRecordID INT PRIMARY KEY AUTO_INCREMENT,
    UserID INT NOT NULL,
    GameSessionID INT NOT NULL,
    TimeUse INT NOT NULL, -- Time in seconds
    GameStringID INT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_SpeedRecord_UserID_User_UserID FOREIGN KEY (UserID) REFERENCES User(UserID),
    CONSTRAINT FK_SpeedRecord_GameSessionID_GameSession_GameSessionID FOREIGN KEY (GameSessionID) REFERENCES GameSession(GameSessionID),
    CONSTRAINT FK_SpeedRecord_GameStringID_GameString_GameStringID FOREIGN KEY (GameStringID) REFERENCES GameString(GameStringID)
);

-- GameStringGameWord Table
CREATE TABLE GameStringGameWord (
    GameStringID INT NOT NULL,
    GameWordID INT NOT NULL,
    
    PRIMARY KEY (GameStringID , GameWordID),
    CONSTRAINT FK_GameStringGameWord_GameWordID_GameWord_GameWordID FOREIGN KEY (GameWordID) REFERENCES GameWord(GameWordID),
    CONSTRAINT FK_GameStringGameWord_GameStringID_GameString_GameStringID FOREIGN KEY (GameStringID) REFERENCES GameString(GameStringID)
);

-- SessionGuessWord Table
CREATE TABLE SessionGuessWord (
    SessionGuessWordID INT PRIMARY KEY AUTO_INCREMENT,
    GameSessionID INT NOT NULL,
    GuessWord VARCHAR(255) NOT NULL,
    CONSTRAINT FK_SessionGuessWord_GameSessionID FOREIGN KEY (GameSessionID) REFERENCES GameSession(GameSessionID)
);



-- function to get random game string
DELIMITER $$

CREATE FUNCTION GetRandomGameString()
RETURNS CHAR(80)
READS SQL DATA
BEGIN
    RETURN (
        SELECT GameStringText
        FROM GameString
        ORDER BY RAND()
        LIMIT 1
    );
END$$

DELIMITER ;

SELECT GetRandomGameString();