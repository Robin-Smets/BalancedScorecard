SELECT
    sp.BusinessEntityID AS SalesPersonID,
    COALESCE(p.FirstName + ' ' + p.LastName, 'Unknown') AS SalesPersonName,
    CAST(soh.OrderDate AS DATE) AS OrderDate,
    SUM(soh.TotalDue) AS OrderVolume
FROM
    Sales.SalesOrderHeader soh
LEFT JOIN
    Sales.SalesPerson sp ON soh.SalesPersonID = sp.BusinessEntityID
LEFT JOIN
    Person.Person p ON sp.BusinessEntityID = p.BusinessEntityID
GROUP BY
    sp.BusinessEntityID,
    CAST(soh.OrderDate AS DATE),
    COALESCE(p.FirstName + ' ' + p.LastName, 'Unknown')
ORDER BY
    OrderVolume DESC;
