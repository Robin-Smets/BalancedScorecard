using BalancedScorecard.PWA;
using BalancedScorecard.PWA.Enums;
using Microsoft.JSInterop;
using System.Data;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace BalancedScorecard.PWA.Services
{
    public class DataStoreService
    {
        private HttpClient _httpClient;
        private IJSRuntime _jsRuntime;
        private Dictionary<Tuple<string,string>, object> _args;
        private DataSet _dataStore;

        public DataStoreService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _args = new Dictionary<Tuple<string, string>, object>();
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _dataStore = new DataSet();
        }

        public async Task UpdateDataStore()
        {
            try
            {
                _dataStore.Clear();
                var table_names_json = await _httpClient.GetStringAsync("/data_store/table_names/");
                var table_names = JsonSerializer.Deserialize<List<string>>(table_names_json);
                foreach(var table_name in table_names)
                {
                    var csv_data = await _httpClient.GetStringAsync($"/data_store/{table_name}/");
                    var table = ConvertCsvToDataTable(csv_data);
                    table.TableName = table_name;
                    _dataStore.Tables.Add(table);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task LoadData()
        {
            try
            {
                await GetArgs(DataStoreServiceTask.LoadData);

                var plotGroups = _args.Where(x => x.Key.Item1.StartsWith("plot"))
                                      .GroupBy(x => x.Key.Item1);

                foreach (var group in plotGroups)
                {
                    var elementId = group.First(x => x.Key.Item2 == "elementId").Value;
                    var data = group.First(x => x.Key.Item2 == "data").Value;
                    var layout = group.First(x => x.Key.Item2 == "layout").Value;

                    await _jsRuntime.InvokeVoidAsync("drawPlot", elementId, data, layout);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task GetArgs(DataStoreServiceTask dataStoreServiceTask)
        {
            _args.Clear();

            if (dataStoreServiceTask == DataStoreServiceTask.LoadData)
            {
                // order volume plots
                var result = _dataStore.Tables["OrderVolume"].AsEnumerable().AsParallel()
                         .GroupBy(x => x["TimeUnitMonth"])
                         .Select(g => new { Key = g.Key.ToString(), Sum = g.Sum(x => (decimal) x["OrderVolume"])})
                         .ToList();

                _args[new Tuple<string, string>("plot", "elementId")] = "order-volume-bar-plot";  
                _args[new Tuple<string, string>("plot", "data")] = JsonSerializer.Serialize(new[]
                {
                    new 
                    { 
                        x = result[0], 
                        y = result[1], 
                        type = "bar"
                    }
                });

                _args[new Tuple<string, string>("plot", "layout")] = JsonSerializer.Serialize(new
                {
                    title = "Order Volume Over Month",
                    xaxis = new { title = "Month" },
                    yaxis = new { title = "Order Volume" }
                });
            }
        }

        public DataTable ConvertCsvToDataTable(string csvData, char delimiter = ';')
        {
            DataTable dataTable = new DataTable();

            // CSV-Daten in Zeilen aufteilen
            using (StringReader reader = new StringReader(csvData))
            {
                bool isFirstLine = true;
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    // Zeile in Spalten aufteilen
                    string[] fields = line.Split(delimiter);

                    // Erste Zeile: Spaltennamen hinzufügen
                    if (isFirstLine)
                    {
                        foreach (string field in fields)
                        {
                            dataTable.Columns.Add(field.Trim());
                        }
                        isFirstLine = false;
                    }
                    else
                    {
                        // Weitere Zeilen: Datenzeilen hinzufügen
                        DataRow row = dataTable.NewRow();
                        for (int i = 0; i < fields.Length; i++)
                        {
                            row[i] = fields[i].Trim();
                        }
                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }

        //private (string[] x, int[] y) ParseCsv(string csvData)
        //{
        //    var xValues = new List<int>();
        //    var yValues = new List<int>();

        //    var lines = csvData.Split('\n');

        //    foreach (var line in lines)
        //    {
        //        if (!string.IsNullOrWhiteSpace(line))
        //        {
        //            var values = line.Split(',');

        //            if (values.Length >= 2)
        //            {
        //                // CSV-Daten in int-Arrays umwandeln
        //                xValues.Add(values[0]);
        //                yValues.Add(int.Parse(values[1]));
        //            }
        //        }
        //    }

        //    return (xValues.ToArray(), yValues.ToArray());
        //}
    }
}
