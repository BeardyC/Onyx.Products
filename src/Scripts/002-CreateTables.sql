IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Products_Colour')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Products_Colour ON dbo.Products(Colour);
    END
GO