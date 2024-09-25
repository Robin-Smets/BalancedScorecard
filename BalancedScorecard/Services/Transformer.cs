using System.Data;

namespace BalancedScorecard.Services
{
    public class Transformer : ITransformer
    {
        private readonly IDataStoreService _dataStoreService;

        public Transformer(IDataStoreService dataStoreService) 
        { 
            _dataStoreService = dataStoreService;
        }

        public List<string> GetTopTenIDs(string featureColumn)
        {
            return _dataStoreService.DataTables["OrderVolume"]
                .AsEnumerable()
                .GroupBy(row => row[featureColumn].ToString())
                .OrderByDescending(group => group.Count()) // Beispiel: Ermittlung anhand der Häufigkeit
                .Take(10)
                .Select(group => group.Key)
                .ToList();
        }

        public DataTable FilterDataTableByTopTenIDs(DataTable originalData,
                                                    List<string> topSalesPersons,
                                                    List<string> topCustomers,
                                                    List<string> topProducts,
                                                    List<string> topTerritories)
        {
            // Neue DataTable erstellen, die nur die gewünschten Spalten enthält
            DataTable filteredData = new DataTable();

            // Relevante Spalten hinzufügen: Name Columns und OrderVolume
            filteredData.Columns.Add("SalesPersonName", typeof(string));
            filteredData.Columns.Add("CustomerName", typeof(string));
            filteredData.Columns.Add("ProductName", typeof(string));
            filteredData.Columns.Add("TerritoryName", typeof(string));
            filteredData.Columns.Add("OrderVolume", typeof(decimal));

            // Filtere die Daten basierend auf den ID-Spalten
            var filteredRows = originalData.AsEnumerable()
                .Where(row => topSalesPersons.Contains(row["SalesPersonID"].ToString()) &&
                              topCustomers.Contains(row["CustomerID"].ToString()) &&
                              topProducts.Contains(row["ProductID"].ToString()) &&
                              topTerritories.Contains(row["TerritoryID"].ToString()));

            // Für jede gefilterte Zeile nur die relevanten Spalten übernehmen
            foreach (var row in filteredRows)
            {
                DataRow newRow = filteredData.NewRow();
                newRow["SalesPersonName"] = row["SalesPersonName"];
                newRow["CustomerName"] = row["CustomerName"];
                newRow["ProductName"] = row["ProductName"];
                newRow["TerritoryName"] = row["TerritoryName"];
                newRow["OrderVolume"] = row["OrderVolume"];

                filteredData.Rows.Add(newRow);
            }

            return filteredData;
        }

        public DataTable CreateAverageOrderVolumeMatrix(DataTable originalData)
        {
            // Schritt 1: Alle eindeutigen Werte aus den nominalen Spalten extrahieren
            var salesPersons = originalData.AsEnumerable().Select(r => r["SalesPersonName"].ToString()).Distinct().ToList();
            var customers = originalData.AsEnumerable().Select(r => r["CustomerName"].ToString()).Distinct().ToList();
            var products = originalData.AsEnumerable().Select(r => r["ProductName"].ToString()).Distinct().ToList();
            var territories = originalData.AsEnumerable().Select(r => r["TerritoryName"].ToString()).Distinct().ToList();

            // Neue Tabelle erstellen
            DataTable transformedTable = new DataTable();

            // Schritt 2: Spalten für jeden eindeutigen Wert der nominalen Spalten hinzufügen
            foreach (var salesPerson in salesPersons)
                transformedTable.Columns.Add($"SalesPerson_{salesPerson}", typeof(decimal));

            foreach (var customer in customers)
                transformedTable.Columns.Add($"Customer_{customer}", typeof(decimal));

            foreach (var product in products)
                transformedTable.Columns.Add($"Product_{product}", typeof(decimal));

            foreach (var territory in territories)
                transformedTable.Columns.Add($"Territory_{territory}", typeof(decimal));

            // Y-Achsen Spalte hinzufügen
            transformedTable.Columns.Add("YAxis", typeof(string));

            // Schritt 3: Zeilen für jede Y-Achse (hier für jeden Produktnamen, Kunden usw.) erstellen
            var featureValues = transformedTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).Where(x => x != "YAxis").ToList();

            foreach (var featureValue in featureValues)
            {
                var row = transformedTable.NewRow();
                row["YAxis"] = featureValue;

                foreach (DataColumn column in transformedTable.Columns.Cast<DataColumn>().Where(x => x.ColumnName != "YAxis").ToList())
                {
                    row[column.ColumnName] = CalculateAverageOrderVolume(originalData, column.ColumnName, featureValue);
                }

                transformedTable.Rows.Add(row);
            }

            return transformedTable;
        }


        private decimal CalculateAverageOrderVolume(DataTable originalData, string x, string y)
        {
            try
            {
                var xSplit = x.Split('_');
                var xColumn = xSplit[0] + "Name";
                var xValue = xSplit[1];
                var ySplit = y.Split('_');
                var yColumn = ySplit[0] + "Name";
                var yValue = ySplit[1];

                var relevantRows = originalData.AsEnumerable()
                    .Where(row => row[xColumn].ToString() == xValue)
                    .Where(row => row[yColumn].ToString() == yValue)
                    .ToList();

                if (relevantRows.Count == 0)
                    return 0; // Keine Daten für diese Kombination

                // Durchschnitt berechnen
                return relevantRows.Average(row => Convert.ToDecimal(row["OrderVolume"]));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return 0;
            }
  
        }




    }
}
