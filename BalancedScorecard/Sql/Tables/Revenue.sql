WITH OrderDetails AS (
    -- Sum LineTotal for each order to get the base amount before adding freight and taxes
    SELECT 
        OrderTable.SalesOrderID,
        SUM(CAST(OrderDetail.LineTotal AS DECIMAL(38, 20))) AS OrderLineTotal
    FROM Sales.SalesOrderDetail AS OrderDetail
    JOIN Sales.SalesOrderHeader AS OrderTable
        ON OrderDetail.SalesOrderID = OrderTable.SalesOrderID
    GROUP BY OrderTable.SalesOrderID
)

SELECT
	DATEPART(ISO_WEEK, OrderTable.OrderDate) AS OrderDateCalenderWeek,
	DATEPART(MONTH, OrderTable.OrderDate) AS OrderDateMonth, 
	DATEPART(QUARTER, OrderTable.OrderDate) AS OrderDateQuarter, 
	DATEPART(YEAR, OrderTable.OrderDate) AS OrderDateYear,
	CONCAT(DATEPART(ISO_WEEK, OrderTable.OrderDate), '/', DATEPART(YEAR, OrderTable.OrderDate)) AS CW,
	CONCAT(DATENAME(MONTH, OrderTable.OrderDate), '/', DATEPART(YEAR, OrderTable.OrderDate)) AS Month, 
	CONCAT(DATEPART(QUARTER, OrderTable.OrderDate), '/', DATEPART(YEAR, OrderTable.OrderDate)) AS Quarter, 
	OrderTable.OrderDate,
	OrderTable.SalesOrderID,

	OrderTable.CustomerID,
	COALESCE(Person.FirstName + ' ' + Person.LastName, Store.Name) AS CustomerName,
	CONCAT(OrderTable.CustomerID, '#', COALESCE(Person.FirstName + ' ' + Person.LastName, Store.Name)) AS Customer,

	OrderTable.TerritoryID,
	SalesTerritory.Name AS TerritoryName,
	CONCAT(OrderTable.TerritoryID, '#', SalesTerritory.Name) AS Territory,

	OrderTable.SalesPersonID,
	PersonSales.FirstName + ' ' + PersonSales.LastName AS SalesPersonName,
    CONCAT(OrderTable.SalesPersonID, '#', PersonSales.FirstName + ' ' + PersonSales.LastName) AS SalesPerson,

	Product.ProductID,
	Product.Name AS ProductName,
    CONCAT(Product.ProductID, '#', Product.Name) AS Product,

    -- Calculate proportional Freight and TaxAmt for each line item
    OrderDetail.LineTotal + 
        (OrderDetail.LineTotal / OrderLineTotal.OrderLineTotal) * 
        (OrderTable.Freight + OrderTable.TaxAmt) AS Revenue,

    -- Gesamtsumme von OrderVolume (über alle Zeilen hinweg)
    SUM(OrderDetail.LineTotal + 
        (OrderDetail.LineTotal / OrderLineTotal.OrderLineTotal) * 
        (OrderTable.Freight + OrderTable.TaxAmt))
        OVER () AS TotalOrderVolume,

    -- Prozentualer Anteil des Zeilen OrderVolume am Gesamt OrderVolume
     ((OrderDetail.LineTotal + 
        (OrderDetail.LineTotal / OrderLineTotal.OrderLineTotal) * 
        (OrderTable.Freight + OrderTable.TaxAmt)) * 100) /
		    (SUM(OrderDetail.LineTotal + 
        (OrderDetail.LineTotal / OrderLineTotal.OrderLineTotal) * 
        (OrderTable.Freight + OrderTable.TaxAmt))
        OVER ())
        AS OrderVolumePercentage,

        OrderTable.Status


FROM Sales.SalesOrderHeader AS OrderTable
INNER JOIN Sales.SalesOrderDetail AS OrderDetail
    ON OrderTable.SalesOrderID = OrderDetail.SalesOrderID
INNER JOIN Production.Product AS Product
    ON OrderDetail.ProductID = Product.ProductID
LEFT JOIN Sales.Customer AS Customer
    ON OrderTable.CustomerID = Customer.CustomerID
LEFT JOIN Person.Person AS Person
    ON Customer.PersonID = Person.BusinessEntityID
LEFT JOIN Sales.Store AS Store
    ON Customer.StoreID = Store.BusinessEntityID
LEFT JOIN Sales.SalesPerson AS SalesPerson
    ON OrderTable.SalesPersonID = SalesPerson.BusinessEntityID
LEFT JOIN Person.Person AS PersonSales
    ON SalesPerson.BusinessEntityID = PersonSales.BusinessEntityID
LEFT JOIN Sales.SalesTerritory AS SalesTerritory
    ON OrderTable.TerritoryID = SalesTerritory.TerritoryID
JOIN OrderDetails AS OrderLineTotal
    ON OrderTable.SalesOrderID = OrderLineTotal.SalesOrderID

    WHERE OrderTable.Status = 5;
