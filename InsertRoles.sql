USE [SetLight]
GO

INSERT INTO [dbo].[AspNetRoles]
           ([Id]
           ,[Name])
     VALUES
           (NEWID()
           ,'Administrador')
GO

USE [SetLight]
GO

INSERT INTO [dbo].[AspNetRoles]
           ([Id]
           ,[Name])
     VALUES
           (NEWID()
           ,'Tecnico')
GO


USE [SetLight]
GO

INSERT INTO [dbo].[AspNetRoles]
           ([Id]
           ,[Name])
     VALUES
           (NEWID()
           ,'Colaborador')
GO


