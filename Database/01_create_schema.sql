/* =========================================================
   VistaTiBooks - DDL (SQL Server)
   Creates DB + tables + constraints + recommended indexes.
   ========================================================= */

-- Create DB if it doesn't exist
IF DB_ID(N'VistaTiBooks') IS NULL
BEGIN
  CREATE DATABASE VistaTiBooks;
END
GO

USE VistaTiBooks;
GO

-- USERS
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.Users (
      Id        INT IDENTITY(1,1)  NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
      UserName  NVARCHAR(100)      NOT NULL,
      CreatedAt DATETIME2(3)       NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME()
  );

  -- If usernames must be unique, keep this. If not, remove it.
  CREATE UNIQUE INDEX UX_Users_UserName ON dbo.Users(UserName);
END
GO

-- FAVORITES
IF OBJECT_ID(N'dbo.Favorites', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.Favorites (
      Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Favorites PRIMARY KEY,
      UserId           INT               NOT NULL,
      ExternalId       NVARCHAR(100)     NOT NULL,
      Title            NVARCHAR(255)     NOT NULL,
      Authors          NVARCHAR(255)     NOT NULL,
      FirstPublishYear INT               NULL,
      CoverUrl         NVARCHAR(500)     NULL,
      CreatedAt        DATETIME2(3)      NOT NULL CONSTRAINT DF_Favorites_CreatedAt DEFAULT SYSUTCDATETIME(),

      CONSTRAINT FK_Favorites_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,

      CONSTRAINT UQ_Favorites_User_External
        UNIQUE (UserId, ExternalId)
  );

  -- Useful for queries like: get favorites by user
  CREATE INDEX IX_Favorites_UserId_CreatedAt
    ON dbo.Favorites(UserId, CreatedAt DESC);
END 
GO