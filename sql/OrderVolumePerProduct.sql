SELECT
    p.ProductID,
    p.Name AS ProductName,
    CAST(soh.OrderDate AS DATE) AS OrderDate,
    SUM(sod.LineTotal) AS OrderVolume
FROM
    Sales.SalesOrderDetail sod
JOIN
    Sales.SalesOrderHeader soh ON sod.SalesOrderID = soh.SalesOrderID
JOIN
    Production.Product p ON sod.ProductID = p.ProductID
GROUP BY
    p.ProductID,
    p.Name,
    CAST(soh.OrderDate AS DATE)
ORDER BY
    OrderVolume DESC;
