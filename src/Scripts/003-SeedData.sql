IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Products_Colour')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_Colour ON dbo.Products(Colour);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
INSERT INTO dbo.Products (Name, Colour, Price)
VALUES
    ('Product1', 'Red', 29.99),
    ('Product2', 'Blue', 59.99),
    ('Product3', 'Green', 19.99),
    ('Product4', 'Yellow', 9.99);
END
GO
