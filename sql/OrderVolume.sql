SET LANGUAGE English;

SELECT
	DATEPART(ISO_WEEK, OrderTable.OrderDate) AS OrderDateCalenderWeek,
	DATEPART(MONTH, OrderTable.OrderDate) AS OrderDateMonth, 
	DATEPART(QUARTER, OrderTable.OrderDate) AS OrderDateQuarter, 
	DATEPART(YEAR, OrderTable.OrderDate) AS OrderDateYear,
	CONCAT(DATEPART(ISO_WEEK, OrderTable.OrderDate), '/', DATEPART(YEAR, OrderTable.OrderDate)) AS TimeUnitCalenderWeek,
	CONCAT(DATENAME(MONTH, OrderTable.OrderDate), '/', DATEPART(YEAR, OrderTable.OrderDate)) AS TimeUnitMonth, 
	CONCAT(DATEPART(QUARTER, OrderTable.OrderDate), '/', DATEPART(YEAR, OrderTable.OrderDate)) AS TimeUnitQuarter, 
	* FROM Sales.SalesOrderHeader AS OrderTable