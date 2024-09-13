import pyodbc

conn_str = (
    "DRIVER={ODBC Driver 18 for SQL Server};"
    "SERVER=localhost;"  # oder IP-Adresse des Servers
    "DATABASE=AdventureWorks2022;"
    "UID=sa;"
    "PWD=@Splitsoul3141;"
    "TrustServerCertificate=yes;"
)

try:
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    cursor.execute("SELECT COUNT(*) FROM Sales.SalesOrderHeader")
    for row in cursor:
        print(row)
except pyodbc.Error as e:
    print("Error:", e)