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

--Devolucion de Ordenes
CREATE TABLE dbo.ReturnDetails (
    ReturnDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    EquipmentId INT NOT NULL,
    ReturnDate DATE NOT NULL DEFAULT GETDATE(),
    ConditionReport NVARCHAR(255),
    IsReturned BIT NOT NULL DEFAULT 0,
    RequiresMaintenance BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_ReturnDetails_RentalOrders FOREIGN KEY (OrderId) REFERENCES dbo.RentalOrders(OrderId),
    CONSTRAINT FK_ReturnDetails_Equipment FOREIGN KEY (EquipmentId) REFERENCES dbo.Equipment(EquipmentId)
);

--Empleados
CREATE TABLE Empleado (
    IdEmpleado INT PRIMARY KEY IDENTITY(1,1),
    IdEmpleadoGuid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), -- ID interno
    Nombre NVARCHAR(30) NOT NULL,
    Apellido NVARCHAR(30) NOT NULL,
    TelefonoCelular NVARCHAR(10) NOT NULL,
    CorreoElectronico NVARCHAR(50) NOT NULL UNIQUE,
    RolId NVARCHAR(128) NOT NULL,
    IdUsuarioIdentity NVARCHAR(128) NULL, -- se completa en el registro
    Estado BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Empleado_AspNetRoles FOREIGN KEY (RolId)
        REFERENCES AspNetRoles(Id)
)




ALTER TABLE Equipment
ADD Stock INT NOT NULL DEFAULT 0;


ALTER TABLE Clients
ADD Status INT NOT NULL;

ALTER TABLE RentalOrders ADD RutaComprobante VARCHAR(255) NULL;


ALTER TABLE RentalOrders
ADD EmpleadoId INT NULL;

ALTER TABLE RentalOrders
ADD CONSTRAINT FK_RentalOrders_Empleado
FOREIGN KEY (EmpleadoId) REFERENCES Empleado(IdEmpleado);


ALTER TABLE RentalOrders
ADD DescuentoManual DECIMAL(18, 2) NULL;

