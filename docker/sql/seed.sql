IF DB_ID('ProductsDB') IS NULL
BEGIN
    CREATE DATABASE ProductsDB;
END
GO

USE ProductsDB;
GO

BEGIN TRY
    IF OBJECT_ID('dbo.Products', 'U') IS NULL
    BEGIN
        CREATE TABLE Products (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            Name NVARCHAR(100) NOT NULL,
            Colour NVARCHAR(50) NOT NULL,
            Price DECIMAL(18,2) NOT NULL,
            CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
        );

        CREATE NONCLUSTERED INDEX IX_Products_Colour ON Products(Colour);

        INSERT INTO Products (Name, Colour, Price)
        VALUES 
        ('Red Shirt', 'Red', 29.99),
        ('Blue Jeans', 'Blue', 59.99),
        ('Green Hat', 'Green', 19.99),
        ('Yellow Socks', 'Yellow', 9.99);
    END
END TRY
BEGIN CATCH
    PRINT 'Error encountered during database setup: ' + ERROR_MESSAGE();
END CATCH
GO
