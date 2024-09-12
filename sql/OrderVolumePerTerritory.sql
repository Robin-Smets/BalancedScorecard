SELECT
    t.TerritoryID,
    t.Name AS TerritoryName,
    CAST(soh.OrderDate AS DATE) AS OrderDate,
    SUM(soh.TotalDue) AS OrderVolume
FROM
    Sales.SalesOrderHeader soh
JOIN
    Sales.SalesTerritory t ON soh.TerritoryID = t.TerritoryID
GROUP BY
    t.TerritoryID,
    t.Name,
    CAST(soh.OrderDate AS DATE)
ORDER BY
    OrderVolume DESC;
