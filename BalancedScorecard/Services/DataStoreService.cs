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
        //private IAppState _appState;

        public DataStoreService(IServiceProvider services) 
        {
            _services = services;
            //_appState = _services.GetRequiredService<IAppState>();
            //FromDateFilter = DateTime.Now;
            //UntilDateFilter = DateTime.Now;
            _dataStore = new DataSet();
            _sqlScriptsPath = "./Sql/Tables/";
            _connectionString = "DRIVER={ODBC Driver 18 for SQL Server};SERVER=localhost;DATABASE=AdventureWorks2022;UID=sa;PWD=@Splitsoul3141;TrustServerCertificate=yes;";
        }

        public async Task<(List<string>, List<decimal>)> CreatePlotDataSource(
            string groupByColumn,
            DateTime fromDateFilter,
            DateTime untilDateFilter,
            int top = 0,
            bool cutID = false,
            KPI kpi = KPI.Undefined
        )
        {
            var plotXValues = new List<string>();
            var plotYValues = new List<decimal>();
            var plotGroups = new Dictionary<string, decimal>();
            var groupNames = new Dictionary<string, string>();

            // Grundlegendes Filtering nach Datum
            var filteredRows = DataTables[kpi.ToString()].AsEnumerable()
                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) >= fromDateFilter)
                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) <= untilDateFilter);

            //if (kpi == KPI.OrderVolume)
            //{
            //    filteredRows = filteredRows.Where(x => x["Status"].ToString() is string status && (status == "1" || status == "2" || status == "3"));
            //}
            //if (kpi == KPI.Revenue)
            //{
            //    filteredRows = filteredRows.Where(x => x["Status"].ToString() is string status && (status == "5"));
            //}

            // Gruppierung und Aggregation
            foreach (var row in filteredRows)
            {
                var groupKey = row[groupByColumn].ToString();
                var aggregatableValue = Convert.ToDecimal(row[kpi.ToString()]);

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

        //public async Task<(List<string>, List<decimal>)> CreatePlotDataSource(string groupByColumn, DateTime fromDateFilter, DateTime untilDateFilter, int top = 0, bool cutID = false)
        //{
        //    var plotXValues = new List<string>();
        //    var plotYValues = new List<decimal>();
        //    var plotGroups = new Dictionary<string, decimal>();
        //    var groupNames = new Dictionary<string, string>();

        //    var filteredRows = DataTables["OrderVolume"].AsEnumerable()
        //                                                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) >= fromDateFilter)
        //                                                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()) <= untilDateFilter);

        //    foreach (var row in filteredRows)
        //    {
        //        var groupKey = row[groupByColumn].ToString();
        //        var aggregatableValue = Convert.ToDecimal(row["OrderVolume"]);

        //        if (plotGroups.ContainsKey(groupKey))
        //        {
        //            plotGroups[groupKey] += aggregatableValue;
        //        }
        //        else
        //        {
        //            plotGroups[groupKey] = aggregatableValue;
        //        }
        //    }

        //    if (top > 0)
        //    {
        //        var plotGroupsList = plotGroups.AsEnumerable()
        //                                       .OrderByDescending(x => x.Value)
        //                                       .Take(top);

        //        var newPlotGroups = new Dictionary<string,decimal>();

        //        foreach (var plotGroup in plotGroupsList)
        //        {
        //            newPlotGroups[plotGroup.Key] = plotGroup.Value;
        //        }

        //        plotGroups = newPlotGroups;
        //    }

        //    if (cutID)
        //    {
        //        var plotGroupsList = plotGroups.ToList();

        //        foreach (var plotGroup in plotGroupsList)
        //        {
        //            plotGroups.Remove(plotGroup.Key);
        //            var newKey = plotGroup.Key.Split("#")[1];
        //            plotGroups[newKey] = plotGroup.Value;
        //        }
        //    }

        //    foreach (var group in plotGroups)
        //    {
        //        plotXValues.Add(group.Key);
        //        plotYValues.Add(group.Value);
        //    }

        //    return (plotXValues, plotYValues);
        //}

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
