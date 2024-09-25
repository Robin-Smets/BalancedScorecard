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
	CONCAT(DATEPART(ISO_WEEK, OrderTable.OrderDate), '/', DATEPART(YEAR, OrderTable.OrderDate)) AS TimeUnitCalenderWeek,
	CONCAT(DATENAME(MONTH, OrderTable.OrderDate), '/', DATEPART(YEAR, OrderTable.OrderDate)) AS TimeUnitMonth, 
	CONCAT(DATEPART(QUARTER, OrderTable.OrderDate), '/', DATEPART(YEAR, OrderTable.OrderDate)) AS TimeUnitQuarter, 
	OrderTable.OrderDate,
	OrderTable.SalesOrderID,
	OrderTable.CustomerID,
	COALESCE(Person.FirstName + ' ' + Person.LastName, Store.Name) AS CustomerName,
	OrderTable.TerritoryID,
	SalesTerritory.Name AS TerritoryName,
	OrderTable.SalesPersonID,
	PersonSales.FirstName + ' ' + PersonSales.LastName AS SalesPersonName,
	Product.ProductID,
	Product.Name AS ProductName,
    
    -- Calculate proportional Freight and TaxAmt for each line item
    ROUND(OrderDetail.LineTotal + 
        (OrderDetail.LineTotal / OrderLineTotal.OrderLineTotal) * 
        (OrderTable.Freight + OrderTable.TaxAmt), 10) AS OrderVolume

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
    ON OrderTable.SalesOrderID = OrderLineTotal.SalesOrderID;
