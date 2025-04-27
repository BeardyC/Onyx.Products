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
