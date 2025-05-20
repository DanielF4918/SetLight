CREATE DATABASE SetLight;
GO

USE SetLight;
GO


CREATE DATABASE SetLight;
GO

USE SetLight;
GO

-- Tabla de usuarios del sistema
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL
);

-- Tabla de clientes
CREATE TABLE Clients (
    ClientId INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100)
);

-- Catálogo de estados de órdenes
CREATE TABLE OrderStatuses (
    OrderStatusId INT PRIMARY KEY IDENTITY(1,1),
    StatusName NVARCHAR(50) NOT NULL
);

-- Tabla de órdenes de alquiler
CREATE TABLE RentalOrders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATE NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    StatusOrderId INT NOT NULL,
    ClientId INT NOT NULL,
    FOREIGN KEY (StatusOrderId) REFERENCES OrderStatuses(OrderStatusId),
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);

-- Tabla de categorías de equipos
CREATE TABLE EquipmentCategories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL
);

-- Catálogo de estados de equipo
CREATE TABLE EquipmentStatuses (
    StatusId INT PRIMARY KEY IDENTITY(1,1),
    StatusName NVARCHAR(50) NOT NULL
);

-- Tabla de equipos
CREATE TABLE Equipment (
    EquipmentId INT PRIMARY KEY IDENTITY(1,1),
    EquipmentName NVARCHAR(100) NOT NULL,
    Brand NVARCHAR(100) NOT NULL,
    Model NVARCHAR(100) NOT NULL,
    SerialNumber NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    CategoryId INT NOT NULL,
    StatusId INT NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES EquipmentCategories(CategoryId),
    FOREIGN KEY (StatusId) REFERENCES EquipmentStatuses(StatusId)
);

-- Detalles de cada orden (qué equipos incluye)
CREATE TABLE OrderDetails (
    DetailId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    EquipmentId INT NOT NULL,
    Quantity INT NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES RentalOrders(OrderId),
    FOREIGN KEY (EquipmentId) REFERENCES Equipment(EquipmentId)
);

-- Historial de uso/cambios de los equipos
CREATE TABLE EquipmentHistory (
    HistoryId INT PRIMARY KEY IDENTITY(1,1),
    EquipmentId INT NOT NULL,
    ChangeDate DATE NOT NULL,
    Notes NVARCHAR(MAX),
    FOREIGN KEY (EquipmentId) REFERENCES Equipment(EquipmentId)
);

-- Registro de mantenimientos realizados a los equipos
CREATE TABLE Maintenance (
    MaintenanceId INT PRIMARY KEY IDENTITY(1,1),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    MaintenanceType INT NOT NULL,       
    MaintenanceStatus INT NOT NULL,    
    EquipmentId INT NOT NULL,
    FOREIGN KEY (EquipmentId) REFERENCES Equipment(EquipmentId)
);
