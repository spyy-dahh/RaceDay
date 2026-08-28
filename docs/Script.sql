--CREATE DATABASE RaceDayDB;
--GO

USE RaceDayDB;
GO

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    PhoneNumber VARCHAR(15),
    Role VARCHAR(20) NOT NULL CHECK (Role IN ('Organiser','Participant')) --ensures that the only values entered are Organiser and Participant and dont add any other roles
)

CREATE TABLE Event (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(255),
    EventDate DATE NOT NULL,
    Location VARCHAR(100) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    EventType VARCHAR(20) NOT NULL CHECK (EventType IN ('Run','Walk','Cycle')), --ensures that the only values entered are Run,Walk,Cycle and no other event type

    CONSTRAINT FK_Event_Organiser 
    FOREIGN KEY (OrganiserID)
    REFERENCES Users(UserID)
)

CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName VARCHAR(50) NOT NULL,
    CategoryType VARCHAR(20) NOT NULL CHECK (CategoryType IN ('Age','Distance')), --ensures that category must be based on age and distance only

    CONSTRAINT FK_Category_Event
    FOREIGN KEY (EventID)
    REFERENCES Event(EventID)
)


CREATE TABLE Route (
    RouteID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL UNIQUE,
    RouteName VARCHAR(100) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    RouteDescription VARCHAR(255),

    CONSTRAINT FK_Route_Event
    FOREIGN KEY (EventID)
    REFERENCES Event(EventID)
)



CREATE TABLE Enrolment (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATE NOT NULL DEFAULT GETDATE(), --defaults to today's date if they dont add a date

    CONSTRAINT FK_Enrolment_User
    FOREIGN KEY (UserID)
    REFERENCES Users(UserID),

    CONSTRAINT FK_Enrolment_Event
    FOREIGN KEY (EventID)
    REFERENCES Event(EventID),

    CONSTRAINT FK_Enrolment_Category
    FOREIGN KEY (CategoryID)
    REFERENCES Category(CategoryID)
)



CREATE TABLE Result (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME NOT NULL,
    Position INT NOT NULL,

    CONSTRAINT FK_Result_Enrolment
    FOREIGN KEY (EnrolmentID)
    REFERENCES Enrolment(EnrolmentID)
)
INSERT INTO Users (FirstName, LastName, Email, Password, PhoneNumber, Role)
VALUES
('Sarah','Mokoena','sarahm@raceday.co.za','Pass123!','0825652391','Organiser'),
('Thabo','Ndlovu','thabondlovu432@raceday.co.za','Pass123!','0832459045','Organiser'),
('Lebo','Khumalo','lebok@gmail.com','Pass123!','0694203267','Participant'),
('Ayesha','Patel','ayeshapatel22@gmail.com','Pass123!','0718517393','Participant')

INSERT INTO Event
(OrganiserID, Name, Description, EventDate, Location, Distance, EventType)
VALUES
(1,'Soweto Marathon','Annual road marathon through Soweto','2027-11-07','Johannesburg',42.20,'Run'),

(1,'Cape Town Cycle Tour','Iconic cycling event around Cape Town','2027-03-14','Cape Town',109.00,'Cycle'),

(2,'Durban Charity Walk','Community fundraising walk','2027-06-20','Durban',10.00,'Walk')

INSERT INTO Category(EventID, CategoryName, CategoryType)
VALUES
(1,'Senior','Age'),
(1,'Veteran','Age'),
(1,'42.2km','Distance'),
(2,'Open','Age'),
(2,'109km','Distance'),
(3,'Open','Age'),
(3,'10km','Distance')

INSERT INTO Route(EventID, RouteName, Distance, RouteDescription)
VALUES
(1,'Soweto Marathon Route',42.20,
'Official Soweto Marathon route.'),

(2,'Cape Town Cycle Tour Route',109.00,
'Scenic coastal cycling route.'),

(3,'Durban Charity Walk Route',10.00,
'Family-friendly fundraising walk.');

INSERT INTO Enrolment (UserID, EventID, CategoryID)
VALUES
(3,1,3),   -- Lebo enters 42.2km Marathon
(3,3,7),   -- Lebo enters Durban Walk
(4,2,5),   -- Ayesha enters Cycle Tour
(4,3,7);   -- Ayesha enters Walk

INSERT INTO Result (EnrolmentID, FinishTime, Position)
VALUES
(1,'03:28:15',128),
(3,'04:15:42',341)