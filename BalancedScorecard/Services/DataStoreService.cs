using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Data;
using System.Globalization;
using System.Data.Odbc;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Components;
using BalancedScorecard.Components.Pages;
using System.Data.Common;
using System.Collections.Generic;
using BalancedScorecard.Enums;
using System.Reflection;
using Tensorflow;

namespace BalancedScorecard.Services
{
    public class DataStoreService : IDataStoreService
    {
        public DataTableCollection DataTables => _dataStore.Tables;

        /// <summary>
        /// 
        /// </summary>
        private DataSet _dataStore;
        private string _sqlScriptsPath;
        private string _connectionString;
        private readonly IServiceProvider _services;

        public DataStoreService(IServiceProvider services) 
        {
            _services = services;
            _dataStore = new DataSet();
            _sqlScriptsPath = "./Sql/Tables/";
            _connectionString = "DRIVER={ODBC Driver 18 for SQL Server};SERVER=localhost;DATABASE=AdventureWorks2022;UID=sa;PWD=@Splitsoul3141;TrustServerCertificate=yes;";
        }

        public async Task<(List<string>, List<string>, decimal[,])> CreateHeatMapDataSource(DateTime fromDateFilter, DateTime untilDateFilter)
        {
            var plotXValues = new List<string>();
            var plotYValues = new List<string>();
            var plotZValues = new List<decimal>();

            // Ersetze List mit Dictionary für schnellere Lookups
            var topTenIds = new Dictionary<string, int>();
            int idCounter = 0;

            // Verwende PLINQ zur parallelen Filterung
            var filteredRows = DataTables["Revenue"].AsEnumerable()
                .AsParallel()
                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) >= fromDateFilter)
                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) <= untilDateFilter);

            // Verwende Dictionaries für die Top-10-Listen zur schnellen Suche
            var topTenCustomers = filteredRows.GroupBy(row => row["Customer"])
                                              .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
                                              .Take(10)
                                              .ToDictionary(g => g.Key.ToString(), g => idCounter++);

            var topTenProducts = filteredRows.GroupBy(row => row["Product"])
                                             .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
                                             .Take(10)
                                             .ToDictionary(g => g.Key.ToString(), g => idCounter++);

            var topTenSalesPersonList = filteredRows.GroupBy(row => row["SalesPerson"])
                                                    .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
                                                    .Take(10)
                                                    .ToDictionary(g => g.Key.ToString(), g => idCounter++);

            var topTenTerritories = filteredRows.GroupBy(row => row["Territory"])
                                                .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
                                                .Take(10)
                                                .ToDictionary(g => g.Key.ToString(), g => idCounter++);

            // Erstelle z-Matrix
            var zLength = idCounter;
            var z = new decimal[zLength, zLength];

            // Parallelisiere die Aggregation der Revenue-Werte
            Parallel.ForEach(filteredRows, row =>
            {
                string customer = row["Customer"].ToString();
                string product = row["Product"].ToString();
                string salesPerson = row["SalesPerson"].ToString();
                string territory = row["Territory"].ToString();
                decimal revenue = Convert.ToDecimal(row["Revenue"]);

                // Nur wenn der Kunde, das Produkt, der Verkäufer oder das Territorium in den Top-10 ist
                if (topTenCustomers.ContainsKey(customer))
                {
                    int customerIndex = topTenCustomers[customer];
                    if (topTenProducts.ContainsKey(product))
                    {
                        int productIndex = topTenProducts[product];
                        z[customerIndex, productIndex] += revenue;
                    }
                    if (topTenSalesPersonList.ContainsKey(salesPerson))
                    {
                        int salesPersonIndex = topTenSalesPersonList[salesPerson];
                        z[customerIndex, salesPersonIndex] += revenue;
                    }
                    if (topTenTerritories.ContainsKey(territory))
                    {
                        int territoryIndex = topTenTerritories[territory];
                        z[customerIndex, territoryIndex] += revenue;
                    }
                }

                if (topTenProducts.ContainsKey(product))
                {
                    int productIndex = topTenProducts[product];
                    if (topTenSalesPersonList.ContainsKey(salesPerson))
                    {
                        int salesPersonIndex = topTenSalesPersonList[salesPerson];
                        z[productIndex, salesPersonIndex] += revenue;
                    }
                    if (topTenTerritories.ContainsKey(territory))
                    {
                        int territoryIndex = topTenTerritories[territory];
                        z[productIndex, territoryIndex] += revenue;
                    }
                }

                if (topTenSalesPersonList.ContainsKey(salesPerson) && topTenTerritories.ContainsKey(territory))
                {
                    int salesPersonIndex = topTenSalesPersonList[salesPerson];
                    int territoryIndex = topTenTerritories[territory];
                    z[salesPersonIndex, territoryIndex] += revenue;
                }
            });

            // Labels für X- und Y-Achsen hinzufügen
            foreach (var topTenId in topTenCustomers.Concat(topTenProducts).Concat(topTenSalesPersonList).Concat(topTenTerritories))
            {
                var label = $"{topTenId.Key}";
                plotXValues.Add(label);
                plotYValues.Add(label);
            }

            // Konvertiere z-Matrix in eine Liste für Plotly
            //for (int i = 0; i < zLength; i++)
            //{
            //    for (int j = 0; j < zLength; j++)
            //    {
            //        plotZValues.Add(z[i, j]);
            //    }
            //}

            return (plotXValues, plotYValues, z);
        }


        //public async Task<(List<string>, List<string>, List<decimal>)> CreateHeatMapDataSource(DateTime fromDateFilter, DateTime untilDateFilter)
        //{
        //    var plotXValues = new List<string>();
        //    var plotYValues = new List<string>();
        //    var plotZValues = new List<decimal>();

        //    var topTenTasks = new List<Task>();

        //    var topTenCustomers = new List<string>();
        //    var topTenProducts = new List<string>();
        //    var topTenSalesPersonList = new List<string>();
        //    var topTenTerritories = new List<string>();

        //    var topTenIds = new List<Tuple<string, string>>();

        //    var filteredRows = DataTables["Revenue"].AsEnumerable()
        //        .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) >= fromDateFilter)
        //        .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) <= untilDateFilter);

        //    topTenTasks.Add(
        //        Task.Run(async () =>
        //        {
        //            foreach (var topTenCustomer in filteredRows.GroupBy(row => row["Customer"])
        //                                                       .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
        //                                                       .Take(10))
        //            {
        //                topTenCustomers.Add(topTenCustomer.Key.ToString());
        //            }
        //        })
        //    );

        //    topTenTasks.Add(
        //        Task.Run(async () =>
        //        {
        //            foreach (var topTenProduct in filteredRows.GroupBy(row => row["Product"])
        //                                   .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
        //                                   .Take(10))
        //            {
        //                topTenProducts.Add(topTenProduct.Key.ToString());
        //            }
        //        })
        //    );

        //    topTenTasks.Add(
        //        Task.Run(async () =>
        //        {
        //            foreach (var topTenSalesPerson in filteredRows.GroupBy(row => row["SalesPerson"])
        //                                   .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
        //                                   .Take(10))
        //            {
        //                topTenSalesPersonList.Add(topTenSalesPerson.Key.ToString());
        //            }
        //        })
        //    );

        //    topTenTasks.Add(
        //        Task.Run(async () =>
        //        {
        //            foreach (var topTenTerritory in filteredRows.GroupBy(row => row["Territory"])
        //               .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
        //               .Take(10))
        //            {
        //                topTenTerritories.Add(topTenTerritory.Key.ToString());
        //            }
        //        })
        //    );

        //    await Task.WhenAll(topTenTasks);

        //    foreach (var item in topTenCustomers)
        //    {
        //        topTenIds.Add(Tuple.Create("Customer", item));
        //    }

        //    foreach (var item in topTenProducts)
        //    {
        //        topTenIds.Add(Tuple.Create("Product", item));
        //    }

        //    foreach (var item in topTenSalesPersonList)
        //    {
        //        topTenIds.Add(Tuple.Create("SalesPerson", item));
        //    }

        //    foreach (var item in topTenTerritories)
        //    {
        //        topTenIds.Add(Tuple.Create("Territory", item));
        //    }

        //    var zLength = topTenIds.Count();

        //    var z = new decimal[zLength, zLength];

        //    foreach (var row in filteredRows)
        //    {
        //        // Hier fehlt Logik
        //    }

        //    foreach (var topTenId in topTenIds)
        //    {
        //        var label = $"{topTenId.Item1}_{topTenId.Item2.Split("#")[1]}";
        //        plotXValues.Add(label);
        //        plotYValues.Add(label);
        //    }

        //    return (plotXValues, plotYValues, plotZValues);
        //}

        public async Task<(List<string>, List<decimal>)> CreatePlotDataSource(
            string groupByColumn,
            DateTime fromDateFilter,
            DateTime untilDateFilter,
            int top = 0,
            bool cutID = false,
            Tuple<string, string> whereFilter = null
        )
        {
            var plotXValues = new List<string>();
            var plotYValues = new List<decimal>();
            var plotGroups = new Dictionary<string, decimal>();
            var groupNames = new Dictionary<string, string>();

            // Grundlegendes Filtering nach Datum
            var filteredRows = DataTables["Revenue"].AsEnumerable()
                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) >= fromDateFilter)
                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) <= untilDateFilter);

            if (whereFilter != null && whereFilter.Item1 != "")
            {
                filteredRows = filteredRows.Where(x => x[whereFilter.Item1].ToString() == whereFilter.Item2);
            }

            // Gruppierung und Aggregation
            foreach (var row in filteredRows)
            {
                var groupKey = row[groupByColumn].ToString();
                var aggregatableValue = Convert.ToDecimal(row["Revenue"]);

                if (plotGroups.ContainsKey(groupKey))
                {
                    plotGroups[groupKey] += aggregatableValue;
                }
                else
                {
                    plotGroups[groupKey] = aggregatableValue;
                }
            }

            // Optional: Top X auswählen
            if (top > 0)
            {
                var plotGroupsList = plotGroups.AsEnumerable()
                                               .OrderByDescending(x => x.Value)
                                               .Take(top);

                var newPlotGroups = new Dictionary<string, decimal>();

                foreach (var plotGroup in plotGroupsList)
                {
                    newPlotGroups[plotGroup.Key] = plotGroup.Value;
                }

                plotGroups = newPlotGroups;
            }

            // Optional: IDs schneiden
            if (cutID)
            {
                var plotGroupsList = plotGroups.ToList();

                foreach (var plotGroup in plotGroupsList)
                {
                    plotGroups.Remove(plotGroup.Key);
                    var newKey = plotGroup.Key.Split("#")[1];
                    plotGroups[newKey] = plotGroup.Value;
                }
            }

            // Plotdaten sammeln
            foreach (var group in plotGroups)
            {
                plotXValues.Add(group.Key);
                plotYValues.Add(group.Value);
            }

            return (plotXValues, plotYValues);
        }

        private DateTime? CreateDateTimeFromString(string dateString, string format = "dd.MM.yyyy HH:mm:ss")
        {
            try
            {
                var dateTime = DateTime.ParseExact(dateString, format, CultureInfo.InvariantCulture);
                return dateTime;
            }
            catch (FormatException)
            {
                Console.WriteLine("Das Format des Datums ist ungültig.");
                return null;
            }
        }

        public async Task UpdateDataStore()
        {

            var sqlScripts = GetSqlFilesContent(_sqlScriptsPath);

            using (OdbcConnection connection = new OdbcConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Schleife durch jedes SQL-Skript
                foreach (var sqlScript in sqlScripts)
                {

                    using (OdbcCommand command = new OdbcCommand(sqlScript.Value, connection))
                    {
                        using (System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            // Erstelle eine neue DataTable
                            DataTable dataTable = new DataTable();

                            // Lade die Daten vom OdbcDataReader in die DataTable
                            dataTable.Load(reader);

                            // Optional: Gib der DataTable einen Namen basierend auf dem SQL-Skript oder einem Index
                            dataTable.TableName = sqlScript.Key; // Verwende den Dateinamen als Tabellenname, falls `sqlScript.Key` der Dateiname ist.

                            // Füge die DataTable dem DataSet (_dataStore) hinzu
                            _dataStore.Tables.Add(dataTable);
                        }
                    }
                }

                connection.Close();
            }
        }

        private Dictionary<string, string> GetSqlFilesContent(string directoryPath)
        {
            Dictionary<string, string> sqlFilesDictionary = new Dictionary<string, string>();

            // Überprüfen, ob das Verzeichnis existiert
            if (Directory.Exists(directoryPath))
            {
                // Suchen nach allen .sql-Dateien im Verzeichnis
                string[] sqlFiles = Directory.GetFiles(directoryPath, "*.sql");

                foreach (var filePath in sqlFiles)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath); // Dateiname ohne Erweiterung
                    string fileContent = File.ReadAllText(filePath); // Inhalt der Datei lesen

                    sqlFilesDictionary[fileNameWithoutExtension] = fileContent; // Hinzufügen zum Dictionary
                }
            }
            else
            {
                Console.WriteLine($"Das Verzeichnis '{directoryPath}' existiert nicht.");
            }

            return sqlFilesDictionary;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await UpdateDataStore();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
