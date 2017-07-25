DROP TABLE [dbo].[Foo];

CREATE TABLE [dbo].[Allergenes]([Id] int IDENTITY(1,1), [Name] NVARCHAR(MAX) NOT NULL )
CREATE TABLE [dbo].[Uploaders]([Id] int IDENTITY(1,1), [Name] NVARCHAR(MAX) NOT NULL, [Email] NVARCHAR(MAX) NOT NULL)
CREATE TABLE [dbo].[FCResults]([Id] int IDENTITY(1,1), [Uploader_Id] int NOT NULL, [Allergene_Id] int NOT NULL, [UploadGuid] NVARCHAR(MAX) NOT NULL)
CREATE TABLE [dbo].[AllergeneSubscriptions]([Id] int IDENTITY(1,1), [Allergene_Id] int NOT NULL, [Uploader_Id] int NOT NULL)
