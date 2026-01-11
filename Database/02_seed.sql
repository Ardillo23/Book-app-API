/* =========================================================
   VistaTiBooks - Seed data (optional)
   Creates a default user if none exists.
   ========================================================= */

USE VistaTiBooks;
GO

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Users
    WHERE UserName = N'User Test'
)
BEGIN
    INSERT INTO dbo.Users (UserName)
    VALUES (N'User Test');
END
GO
