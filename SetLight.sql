CREATE DATABASE SetLight;
GO

USE SetLight;
GO

-- Tabla de usuarios del sistema
--CREATE TABLE Users (
--    UserId INT PRIMARY KEY IDENTITY(1,1),
--    FirstName VARCHAR(100) NOT NULL,
--    LastName VARCHAR(100) NOT NULL,
 --   Email VARCHAR(100) NOT NULL
--);

-- Tabla de clientes
CREATE TABLE Clients (
    ClientId INT PRIMARY KEY IDENTITY(1,1),
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Phone VARCHAR(20),
    Email VARCHAR(100)
);

-- Tabla de órdenes de alquiler
CREATE TABLE RentalOrders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATE NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    StatusOrder INT NOT NULL,         
    ClientId INT NOT NULL,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);

-- Tabla de categorías de equipos
CREATE TABLE EquipmentCategories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryName VARCHAR(100) NOT NULL
);

-- Tabla de equipos
CREATE TABLE Equipment (
    EquipmentId INT PRIMARY KEY IDENTITY(1,1),
    EquipmentName VARCHAR(100) NOT NULL,
    Brand VARCHAR(100) NOT NULL,
    Model VARCHAR(100) NOT NULL,
    SerialNumber VARCHAR(100) NOT NULL,
    Description VARCHAR(MAX) NOT NULL,
	RentalValue DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    CategoryId INT NOT NULL,
    Status INT NOT NULL,              
    FOREIGN KEY (CategoryId) REFERENCES EquipmentCategories(CategoryId)
);

-- Detalles de cada orden
CREATE TABLE OrderDetails (
    DetailId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    EquipmentId INT NOT NULL,
    Quantity INT NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES RentalOrders(OrderId),
    FOREIGN KEY (EquipmentId) REFERENCES Equipment(EquipmentId)
);

-- Historial de equipos
CREATE TABLE EquipmentHistory (
    HistoryId INT PRIMARY KEY IDENTITY(1,1),
    EquipmentId INT NOT NULL,
    ChangeDate DATE NOT NULL,
    Notes VARCHAR(MAX),
    FOREIGN KEY (EquipmentId) REFERENCES Equipment(EquipmentId)
);

-- Mantenimiento de equipos
CREATE TABLE Maintenance (
    MaintenanceId INT PRIMARY KEY IDENTITY(1,1),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    MaintenanceType INT NOT NULL,     
    MaintenanceStatus INT NOT NULL,  
    EquipmentId INT NOT NULL,
    FOREIGN KEY (EquipmentId) REFERENCES Equipment(EquipmentId)
);


ALTER TABLE Equipment
ADD Stock INT NOT NULL DEFAULT 0;
