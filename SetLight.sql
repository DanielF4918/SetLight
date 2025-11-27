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

-- Tabla de �rdenes de alquiler
CREATE TABLE RentalOrders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATE NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    StatusOrder INT NOT NULL,         
    ClientId INT NOT NULL,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);

-- Tabla de categor�as de equipos
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
ADD Status INT NOT�NULL;

ALTER TABLE RentalOrders ADD RutaComprobante VARCHAR(255) NULL;


ALTER TABLE RentalOrders
ADD EmpleadoId INT NULL;

ALTER TABLE RentalOrders
ADD CONSTRAINT FK_RentalOrders_Empleado
FOREIGN KEY (EmpleadoId) REFERENCES Empleado(IdEmpleado);


ALTER TABLE RentalOrders
ADD DescuentoManual DECIMAL(18, 2) NULL;


USE [SetLight]; -- Aseg�rate de estar usando la base de datos correcta
GO

SELECT *
FROM AspNetRoles;



USE [SetLight];
GO

INSERT INTO Empleado (
    Nombre,
    Apellido,
    TelefonoCelular,
    CorreoElectronico,
    RolId,
    Estado
)
VALUES (
    'Carlos',
    'Ramrez',
    '88889999',
    'admin69@setlight.com',
    '31F16267-19E9-4FD8-B518-43706DF8000C', -- ID del rol Administrador
    1
);

---Modificaciones 13-09-2025
ALTER TABLE dbo.Empleado
ADD Cedula NVARCHAR(20) NULL,                          
    ContactoEmergenciaNombre NVARCHAR(60) NULL,
    ContactoEmergenciaTelefono NVARCHAR(20) NULL,
    ContactoEmergenciaParentesco NVARCHAR(30) NULL,
    TipoSangre NVARCHAR(3) NULL,                          
    Alergias NVARCHAR(500) NULL,
    InfoMedica NVARCHAR(1000) NULL;                   


	USE SetLight;
GO

-- Agrega una sola columna para la imagen (ruta o URL)
IF COL_LENGTH('dbo.Equipment', 'ImageUrl') IS NULL
BEGIN
    ALTER TABLE dbo.Equipment
    ADD ImageUrl NVARCHAR(500) NULL;   
END


----Cambios 28/10/2025----

ALTER TABLE dbo.Empleado
ADD CONSTRAINT UQ_Empleado_Correo UNIQUE (CorreoElectronico);
GO

-- Crear restricción única para Cedula
ALTER TABLE dbo.Empleado
ADD CONSTRAINT UQ_Empleado_Cedula UNIQUE (Cedula);
GO

--Agregar imagen para empleados
IF COL_LENGTH('dbo.Empleado', 'FotoPerfil') IS NULL
BEGIN
    ALTER TABLE dbo.Empleado
    ADD FotoPerfil NVARCHAR(500) NULL;
END



---Cambios en Mantenimiento--
ALTER TABLE dbo.Maintenance
ADD Comments NVARCHAR(500) NULL,
    Cost DECIMAL(10,2) NULL,
    EvidencePath NVARCHAR(255) NULL;


ALTER TABLE dbo.Maintenance
ADD FinalizadoPor NVARCHAR(256) NULL;



	----en caso de tener problemas con las migraciones
	ALTER TABLE dbo.Maintenance
DROP COLUMN Comments, Cost, EvidencePath;
	ALTER TABLE dbo.Maintenance
DROP COLUMN FinalizadoPor



----para obtener el nombre del tecnico
ALTER TABLE Maintenance
ADD IdEmpleado INT NULL;  -- técnico que atiende el mantenimiento

ALTER TABLE Maintenance
ADD CONSTRAINT FK_Maintenance_Empleado
FOREIGN KEY (IdEmpleado) REFERENCES Empleado(IdEmpleado);

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'IdEmpleado'
      AND Object_ID = Object_ID('dbo.Maintenance')
)
BEGIN
    ALTER TABLE dbo.Maintenance
        ADD IdEmpleado INT NULL;
END



---datos de empresa en la tabla de clientes

-- Agregar columna: Nombre de la empresa del cliente (opcional)
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = 'EmpresaNombre'
      AND Object_ID = Object_ID('dbo.Clients')
)
BEGIN
    ALTER TABLE dbo.Clients
        ADD EmpresaNombre NVARCHAR(150) NULL;
END
GO

-- Agregar columna: Teléfono de la empresa del cliente (opcional)
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = 'EmpresaTelefono'
      AND Object_ID = Object_ID('dbo.Clients')
)
BEGIN
    ALTER TABLE dbo.Clients
        ADD EmpresaTelefono NVARCHAR(25) NULL;
END
GO
 -- Cambio en el Null de Mantenimiento del EndDate --
ALTER TABLE [dbo].[Maintenance]
ALTER COLUMN [EndDate] DATETIME NULL;



