CREATE DATABASE RaceDay
GO

USE RaceDay
GO

--create tables
CREATE TABLE Users (
    userID INT IDENTITY(1,1) PRIMARY KEY,
    userName VARCHAR(100) NOT NULL,
    emailAddress VARCHAR(100) NOT NULL,
    contactNumber VARCHAR(10) NOT NULL,
    [password] VARCHAR(255) NOT NULL, --wrapped in [] to avoid potential keyword problems
    CONSTRAINT UQ_Users_EmailAddress UNIQUE (emailAddress)
)




CREATE TABLE Event_Organiser (
    organiserID INT IDENTITY(1,1) PRIMARY KEY,
    userID INT NOT NULL,
    CONSTRAINT UQ_EventOrganiser_UserID UNIQUE (userID),
    CONSTRAINT FK_Organiser_User FOREIGN KEY (userID) REFERENCES Users(userID)
)



CREATE TABLE Participant (
    participantID INT IDENTITY(1,1) PRIMARY KEY,
    userID INT NOT NULL,
    CONSTRAINT UQ_Participant_UserID UNIQUE (userID),
    CONSTRAINT FK_Participant_User FOREIGN KEY (userID) REFERENCES Users(userID)
)



CREATE TABLE Event (
    eventID INT IDENTITY(1,1) PRIMARY KEY,
    organiserID INT NOT NULL,
    eventName VARCHAR(100) NOT NULL,
    [description] VARCHAR(255),
    eventDate DATE NOT NULL,
    [location] VARCHAR(100) NOT NULL,
    eventType VARCHAR(20) NOT NULL,
    CONSTRAINT CK_Event_EventType CHECK (eventType IN ('Run', 'Walk', 'Cycle')),
    CONSTRAINT FK_Event_Organiser FOREIGN KEY (organiserID) REFERENCES Event_Organiser(organiserID)
)



CREATE TABLE Event_Categories (
    categoryID INT IDENTITY(1,1) PRIMARY KEY,
    eventID INT NOT NULL,
    categoryName VARCHAR(50) NOT NULL,
    distance VARCHAR(20),
    ageRange VARCHAR(30),
    CONSTRAINT FK_Category_Event FOREIGN KEY (eventID) REFERENCES Event(eventID),
    CONSTRAINT UQ_Category_Event_Name UNIQUE (eventID, categoryName)
)




CREATE TABLE Route (
    routeID INT IDENTITY(1,1) PRIMARY KEY,
    eventID INT NOT NULL,
    routeDescription VARCHAR(255),
    distance DECIMAL(5,2) NOT NULL,
    mapURL VARCHAR(255),
    CONSTRAINT UQ_Route_EventID UNIQUE (eventID),
    CONSTRAINT FK_Route_Event FOREIGN KEY (eventID) REFERENCES Event(eventID)
)




CREATE TABLE Event_Enrolment (
    enrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    eventID INT NOT NULL,
    categoryID INT NOT NULL,
    participantID INT NOT NULL,
    enrolmentDate DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    [status] VARCHAR(20) NOT NULL DEFAULT 'Registered',
    CONSTRAINT CK_Enrolment_Status CHECK (Status IN ('Registered', 'Cancelled')),
    CONSTRAINT FK_Enrolment_Event FOREIGN KEY (eventID) REFERENCES Event(eventID),
    CONSTRAINT FK_Enrolment_Category FOREIGN KEY (categoryID) REFERENCES Event_Categories(categoryID),
    CONSTRAINT FK_Enrolment_Participant FOREIGN KEY (participantID) REFERENCES Participant(participantID),
    CONSTRAINT UQ_Enrolment_Participant_Event_Category UNIQUE (participantID, eventID, categoryID)
)



CREATE TABLE Results (
    resultID INT IDENTITY(1,1) PRIMARY KEY,
    enrolmentID INT NOT NULL,
    position INT NOT NULL,
    finishTime TIME NOT NULL,
    dateOfResults DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    CONSTRAINT FK_Result_Enrolment FOREIGN KEY (enrolmentID) REFERENCES Event_Enrolment(enrolmentID),
    CONSTRAINT UQ_Result_EnrolmentID UNIQUE (enrolmentID),
    CONSTRAINT CK_Result_Position CHECK (position > 0)
)


--inserts

INSERT INTO Users(userName, emailAddress, contactNumber, [password])
VALUES
('Sarah Mokoena', 'sarah.mokoena@gmail.com', '0825652391', 'Mokoena#4821'),
('Thabo Ndlovu', 'thabo.ndlovu@gmail.com', '0832459045', 'Thabo$7319'),
('Lebo Khumalo', 'lebo.khumalo@gmail.com', '0694203267', 'Khumalo!5937'),
('Ayesha Patel', 'ayesha.patel@gmail.com', '0718517393', 'Ayesha@8642')



INSERT INTO Event_Organiser(userID)
VALUES
(1),
(2)



INSERT INTO Participant(userID)
VALUES
(3),
(4)



INSERT INTO Event(organiserID, eventName, [description], eventDate, [location], eventType)
VALUES
(1, 'Soweto Marathon', 'Annual road marathon through Soweto', '2027-11-07', 'Johannesburg', 'Run'),
(1, 'Cape Town Cycle Tour', 'Iconic cycling event around Cape Town', '2027-03-14', 'Cape Town', 'Cycle'),
(2, 'Durban Charity Walk', 'Community fundraising walk', '2027-06-20', 'Durban', 'Walk')



INSERT INTO Event_Categories(eventID, categoryName, distance, ageRange)
VALUES
(1, 'Senior', '42.2km', '18-39'),
(1, 'Veteran', '42.2km', '40+'),
(2, 'Open', '109km', '18+'),
(3, 'Open', '10km', 'All Ages')



INSERT INTO Route(eventID, routeDescription, distance, mapURL)
VALUES
(1, 'Official Soweto Marathon Route', 42.20, 'https://raceday.co.za/routes/soweto'),--pseudo linkd
(2, 'Scenic Cape Town Cycle Route', 109.00, 'https://raceday.co.za/routes/capetown'),
(3, 'Family-friendly Durban Walk Route', 10.00, 'https://raceday.co.za/routes/durban')



INSERT INTO Event_Enrolment(eventID, categoryID, participantID)
VALUES
(1, 1, 1),
(3, 4, 1),
(2, 3, 2),
(3, 4, 2)



INSERT INTO Results(enrolmentID, position, finishTime)
VALUES
(1, 128, '03:28:15'),
(3, 341, '04:15:42')

--displau

SELECT * FROM Users
SELECT * FROM Event_Organiser
SELECT * FROM Participant
SELECT * FROM Event
SELECT * FROM Event_Categories
SELECT * FROM Route
SELECT * FROM Event_Enrolment
SELECT * FROM Results
