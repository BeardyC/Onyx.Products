USE ProductsDB;
GO

IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
CREATE TABLE dbo.Products (
                              Id INT IDENTITY(1,1) PRIMARY KEY,
                              Name NVARCHAR(100) NOT NULL,
                              Colour NVARCHAR(50) NOT NULL,
                              Price DECIMAL(18,2) NOT NULL,
                              CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
END
