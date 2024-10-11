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
using System.Linq;
using Radzen;
using System.Diagnostics.Metrics;

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
            var topFeatures = new List<string>();
            var valueMatrix = new Dictionary<string, Dictionary<string, decimal>>();
            decimal[,] z;

            var filteredRows = DataTables["Revenue"].AsEnumerable()
                .AsParallel()
                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) >= fromDateFilter)
                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) <= untilDateFilter);


            var topTenCustomers = filteredRows.GroupBy(row => row["Customer"])
                                              .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
                                              .Take(10);

            var topTenProducts = filteredRows.GroupBy(row => row["Product"])
                                             .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
                                             .Take(10);

            var topTenSalesPersonList = filteredRows.GroupBy(row => row["SalesPerson"])
                                                    .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
                                                    .Take(10);

            var topTenTerritories = filteredRows.GroupBy(row => row["Territory"])
                                                .OrderByDescending(group => group.Sum(x => Convert.ToDecimal(x["Revenue"])))
                                                .Take(10);

            Parallel.Invoke(
                () => topFeatures.AddRange(topTenCustomers.ToList().Select(customers => $"Customer_{customers.Key.ToString()}")),
                () => topFeatures.AddRange(topTenProducts.ToList().Select(products => $"Product_{products.Key.ToString()}")),
                () => topFeatures.AddRange(topTenSalesPersonList.ToList().Select(salesPerson => $"SalesPerson_{salesPerson.Key.ToString()}")),
                () => topFeatures.AddRange(topTenTerritories.ToList().Select(territories => $"Territory_{territories.Key.ToString()}"))
            );

            foreach (var rowId in topFeatures)
            {
                var row = new Dictionary<string, decimal>();

                foreach (var columnName in topFeatures)
                {
                    row[columnName] = 0;
                }

                valueMatrix[rowId] = row;
            }

            Parallel.ForEach(filteredRows, row =>
            {
                var revenue = Convert.ToDecimal(row["Revenue"]);

                var customer = $"Customer_{row["Customer"].ToString()}";

                var product = $"Product_{row["Product"].ToString()}";
                var salesPerson = $"SalesPerson_{row["SalesPerson"].ToString()}";
                var territory = $"Territory_{row["Territory"].ToString()}";
                var features = new List<string>()
                {
                    customer,
                    product, 
                    salesPerson, 
                    territory
                };

                var featureCombinations = new List<Tuple<string, string>>();
                foreach (var item1 in features)
                {
                    foreach (var item2 in features)
                    {
                        if (item1 != item2)
                        {
                            featureCombinations.Add(Tuple.Create(item1, item2));
                        }

                    }
                }

                foreach (var featureCombination in featureCombinations)
                {
                    var feature1 = featureCombination.Item1;
                    var feature2 = featureCombination.Item2;

                    if (topFeatures.Contains(feature1) && topFeatures.Contains(feature2))
                    {
                        var value = valueMatrix[feature1][feature2];
                        value += revenue;
                        valueMatrix[feature1][feature2] = value;
                        valueMatrix[feature2][feature1] = value;
                    }
                }
            });

            z = new decimal[topFeatures.Count(),  topFeatures.Count()];

            for (int xIndex = 0; xIndex < topFeatures.Count(); xIndex++)
            {
                var xId = topFeatures[xIndex];

                for (int yIndex = 0; yIndex < topFeatures.Count(); yIndex++)
                {
                    var yId = topFeatures[yIndex];

                    z[xIndex, yIndex] = valueMatrix[xId][yId];
                }
            }

            return (topFeatures, topFeatures, z);
        }

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
            try
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
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
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
