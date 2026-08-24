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