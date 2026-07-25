USE master;
GO

-- 1.Creacion de BD (sino existe)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name='Northwind_prueba')
BEGIN
	CREATE DATABASE Northwind_prueba;
END
GO

USE Northwind_prueba;
GO

-- 2.Crear tablas
CREATE TABLE Categories(
	CategoryID INT PRIMARY KEY IDENTITY(1,1),
	CategoryName VARCHAR(80) NOT NULL
);
GO

CREATE TABLE Suppliers(
	SupplierID INT PRIMARY KEY IDENTITY(1,1),
	CompanyName VARCHAR(80)
);
GO

CREATE TABLE Products(
	ProductID INT PRIMARY KEY IDENTITY(1,1),
	ProductName VARCHAR(80) NOT NULL,
	UnitPrice MONEY NOT NULL,
	UnitsInStock SMALLINT NOT NULL,
	Descontinued BIT NOT NULL DEFAULT 0,

	SupplierID INT FOREIGN KEY REFERENCES Suppliers(SupplierID),
	CategoryID INT FOREIGN KEY REFERENCES Categories(CategoryID)
);
GO

-- 3.Insertar datos de prueba (para que los dropdowns y la tabla no esten vacios)
INSERT INTO Categories(CategoryName)
VALUES ('Bebidas'),('Condimentos'),('Lacteos');

INSERT INTO Suppliers(CompanyName)
VALUES ('Exotic Liquids'),('New Orleands Cajun Delights');

INSERT INTO Suppliers(CompanyName) --AÑADI POR ERROR MIO
VALUES ('Tokyo Traders');

INSERT INTO Products(ProductName,UnitPrice,UnitsInStock,Descontinued,SupplierID,CategoryID)
VALUES
('Chai',18.00,39,0,1,1),
('Chang',19.00,17,0,1,1),
('Anissed Syrup',10.00,13,0,2,2),
('Chef Anton',22.00,53,0,2,2),
('Mishi Kobe Niku',97.00,29,0,3,3);
GO

select * from Products;
select * from Categories;
select * from Suppliers;

--PROCEDURES

--Procedure "listado-products-porNombre"
CREATE OR ALTER PROCEDURE sp_list_products_by_name
	@ProductName VARCHAR(80)
AS
BEGIN
	SELECT
		p.ProductID,
		p.ProductName,
		p.UnitPrice,
		p.UnitsInStock,
		
		s.CompanyName,
		c.CategoryName
	FROM Products p
	JOIN Categories c ON p.CategoryID=c.CategoryID
	JOIN Suppliers s ON p.SupplierID=s.SupplierID
	WHERE p.ProductName LIKE '%'+@ProductName+'%';
END
GO

select * from Products;

--5.Procedure "listado-categorias" -> DROPDOWN CATEGORIAS
CREATE OR ALTER PROCEDURE sp_list_categories
AS
BEGIN
	SELECT c.CategoryID,c.CategoryName
	FROM Categories c
	ORDER BY c.CategoryName;
END
GO

exec sp_list_categories;

--6.Procedure "listado-suppliers" -> DROPDOWN SUPPLIERS
CREATE OR ALTER PROCEDURE sp_list_suppliers
AS
BEGIN
	SELECT s.SupplierID,s.CompanyName
	FROM Suppliers s
	ORDER BY s.CompanyName
END
GO

exec sp_list_suppliers;

--7.Procedure "insert-product"
CREATE OR ALTER PROCEDURE sp_insert_product
@ProductName VARCHAR(80),
@UnitPrice MONEY,
@UnitsInStock SMALLINT,
-- @Descontinued BIT = 0, --acabado
@SupplierID INT,
@CategoryID INT
AS
BEGIN
	INSERT INTO Products(ProductName,UnitPrice,UnitsInStock,Descontinued,SupplierID,CategoryID)
	VALUES (@ProductName,@UnitPrice,@UnitsInStock,0,@SupplierID,@CategoryID);
END
GO

--8.Procedure "update-product"
CREATE OR ALTER PROCEDURE sp_update_product
	@ProductID AS INT,
	@ProductName AS VARCHAR(80),
	@UnitPrice AS MONEY,
	@UnitsInStock AS SMALLINT,
	@Descontinued AS BIT,
	@SupplierID AS INT,
	@CategoryID AS INT
AS
BEGIN
	UPDATE [dbo].[Products]
   SET [ProductName] = @ProductName
      ,[UnitPrice] = @UnitPrice
      ,[UnitsInStock] = @UnitsInStock
      ,[Descontinued] = @Descontinued
      ,[SupplierID] = @SupplierID
      ,[CategoryID] = @CategoryID
	WHERE ProductID=@ProductID;
END
GO

--9.Procedure get-product-by-id
CREATE OR ALTER PROCEDURE sp_get_product_by_id
	@ProductID AS INT
AS
BEGIN
	SELECT [ProductID]
      ,[ProductName]
      ,[UnitPrice]
      ,[UnitsInStock]
      ,[Descontinued]
      ,[SupplierID]
      ,[CategoryID]
  FROM [dbo].[Products]
  WHERE ProductID=@ProductID;
END
GO

--verificamos los tipos de datos de las columnas de Products
exec sp_help Products;

select * from Products;

/*eliminacion de duplicados en supplier (error mio)
delete from Suppliers
where SupplierID in (4,5,6);*/
