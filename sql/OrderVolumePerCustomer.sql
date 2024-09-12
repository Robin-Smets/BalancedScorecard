SELECT
    COALESCE(p.FirstName + ' ' + p.LastName, s.Name) AS CustomerName,
    c.CustomerID,
    CAST(soh.OrderDate AS DATE) AS OrderDate,
    SUM(soh.TotalDue) AS OrderVolume
FROM 
    Sales.SalesOrderHeader soh
JOIN 
    Sales.Customer c ON soh.CustomerID = c.CustomerID
LEFT JOIN 
    Person.Person p ON c.PersonID = p.BusinessEntityID
LEFT JOIN 
    Sales.Store s ON c.StoreID = s.BusinessEntityID
    c.CustomerID,
    CAST(soh.OrderDate AS DATE),
    COALESCE(p.FirstName + ' ' + p.LastName, s.Name)
ORDER BY 
    OrderVolume DESC;
