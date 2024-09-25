using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Data;
using System.Globalization;
using System.Data.Odbc;
using System.Text;
using System.Security.Cryptography;

namespace BalancedScorecard.Services
{
    public class DataStoreService : IDataStoreService
    {
        public DateTime? FromDateFilter { get; set; }
        public DateTime? UntilDateFilter { get; set; }
        public DataTableCollection DataTables => _dataStore.Tables;
        private DataSet _dataStore;
        private string _localDataStorePath;
        private string _sqlScriptsPath;
        private string _connectionString;
        private bool _loadFromCsv;
        private bool _cacheDataStore;

        public DataStoreService() 
        {
            FromDateFilter = DateTime.Now;
            UntilDateFilter = DateTime.Now;
            _dataStore = new DataSet();
            _localDataStorePath = "./DataStore/";
            _sqlScriptsPath = "./Sql/Tables/";
            _connectionString = "DRIVER={ODBC Driver 18 for SQL Server};SERVER=localhost;DATABASE=AdventureWorks2022;UID=sa;PWD=@Sql42;TrustServerCertificate=yes;";
            _loadFromCsv = true;
            _cacheDataStore = true;
        }

        public async Task LoadData()
        {
            _dataStore.Tables.Clear();
            FromDateFilter = FromDateFilter.Value.Date;
            UntilDateFilter = UntilDateFilter.Value.Date
                                                   .AddDays(1)
                                                   .AddTicks(-1);

            if (_loadFromCsv)
            {
                await LoadDataFromCsv();
            }
            else
            {
                throw new NotImplementedException("LoadDataFromDb not implemented.");
            }

        }

        public async Task LoadDataFromCsv()
        {
            EnsureDirectoryExistsAndHidden(_localDataStorePath);
            var file_paths = GetCsvFilePaths(_localDataStorePath);

            foreach (var file_path in file_paths)
            {
                var table = LoadCsvToDataTable(file_path);
                table.TableName = Path.GetFileNameWithoutExtension(file_path);
                _dataStore.Tables.Add(table);
            }

            if (_cacheDataStore)
            {
                
            }
            else
            {
                throw new NotImplementedException("Load data without caching not implemented.");
            }
        }

        public async Task UpdateDataStore()
        {
            EnsureDirectoryExistsAndHidden(_localDataStorePath);
            var sqlScripts = GetSqlFilesContent(_sqlScriptsPath);
            foreach (var sqlScript in sqlScripts) 
            {
                ExportToCsv(_connectionString, sqlScript.Value, $"{_localDataStorePath}{sqlScript.Key}.csv");
            }
        }

        public async Task EncryptDataStore(string key)
        {
            EnsureDirectoryExistsAndHidden(_localDataStorePath);
            var file_paths = GetCsvFilePaths(_localDataStorePath);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32)); // 256 Bit Key
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))

                foreach (var file_path in file_paths)
                {
                    var plainText = File.ReadAllText(file_path);

                    using (var ms = new MemoryStream())
                    {
                        ms.Write(aes.IV, 0, aes.IV.Length); // IV an den Anfang des Streams anhängen

                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }

                        var encryptedText = Convert.ToBase64String(ms.ToArray());

                        File.WriteAllText(file_path, encryptedText);
                    }
                }

            }
        }

        public async Task DecryptDataStore(string key)
        {
            EnsureDirectoryExistsAndHidden(_localDataStorePath);
            var file_paths = GetCsvFilePaths(_localDataStorePath);

            using (Aes aes = Aes.Create())
            {
                // Sicherstellen, dass der Schlüssel auf 256 Bit (32 Byte) eingestellt ist
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));

                foreach (var file_path in file_paths)
                {
                    try
                    {
                        // Lese den verschlüsselten Text
                        var cipherText = await File.ReadAllTextAsync(file_path);
                        var fullCipher = Convert.FromBase64String(cipherText);

                        // IV aus den ersten 16 Bytes des Ciphertext extrahieren
                        byte[] iv = new byte[aes.BlockSize / 8];
                        Array.Copy(fullCipher, iv, iv.Length);

                        string decryptedText;

                        // Entschlüsselung
                        using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
                        using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        using (var sr = new StreamReader(cs))
                        {
                            decryptedText = await sr.ReadToEndAsync(); // Asynchrone Methode verwenden
                        }

                        // Schreiben der entschlüsselten Daten in die Datei
                        await File.WriteAllTextAsync(file_path, decryptedText); // Asynchrone Methode verwenden
                        Console.WriteLine($"File decrypted: {file_path}");
                    }
                    catch (Exception ex)
                    {
                        // Fange Ausnahmen und protokolliere sie
                        Console.WriteLine($"Fehler beim Entschlüsseln der Datei {file_path}: {ex.Message}");
                    }
                }
            }
        }


        private void EnsureDirectoryExistsAndHidden(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                File.SetAttributes(directoryPath, File.GetAttributes(directoryPath) | FileAttributes.Hidden);
                Console.WriteLine($"Verzeichnis erstellt und versteckt: {directoryPath}");
            }
            else
            {
                // Falls das Verzeichnis existiert, stellen wir sicher, dass es versteckt ist
                FileAttributes attributes = File.GetAttributes(directoryPath);
                if ((attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    File.SetAttributes(directoryPath, attributes | FileAttributes.Hidden);
                    Console.WriteLine($"Verzeichnis wurde versteckt: {directoryPath}");
                }
            }
        }
        private DataTable LoadCsvToDataTable(string filePath)
        {
            DataTable dataTable = new DataTable();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",               // Setzt das Trennzeichen auf Semikolon
                HasHeaderRecord = true          // Gibt an, dass die erste Zeile Spaltennamen enthält
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                using (var dr = new CsvDataReader(csv))
                {
                    dataTable.Load(dr);         // Lädt die Daten in die DataTable
                }
            }

            return dataTable;
        }

        private List<string> GetCsvFilePaths(string directoryPath)
        {
            List<string> csvFilePaths = new List<string>();

            if (Directory.Exists(directoryPath)) // Überprüfen, ob das Verzeichnis existiert
            {
                // Sucht alle CSV-Dateien im Verzeichnis und allen Unterverzeichnissen
                string[] files = Directory.GetFiles(directoryPath, "*.csv", SearchOption.AllDirectories);

                csvFilePaths.AddRange(files);
            }
            else
            {
                Console.WriteLine($"Das Verzeichnis '{directoryPath}' existiert nicht.");
            }

            return csvFilePaths;
        }

        private void ExportToCsv(string connectionString, string sqlQuery, string csvFilePath)
        {
            using (OdbcConnection connection = new OdbcConnection(connectionString))
            {
                connection.Open();

                using (OdbcCommand command = new OdbcCommand(sqlQuery, connection))
                using (OdbcDataReader reader = command.ExecuteReader())
                using (StreamWriter writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
                {
                    // Schreibe die Header (Spaltennamen) in die CSV-Datei
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        writer.Write(reader.GetName(i));
                        if (i < reader.FieldCount - 1)
                        {
                            writer.Write(";");
                        }
                    }
                    writer.WriteLine();

                    // Schreibe die Daten in die CSV-Datei
                    StringBuilder sb = new StringBuilder();
                    while (reader.Read())
                    {
                        sb.Clear(); // Leere den StringBuilder für die neue Zeile
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            // Füge den Wert hinzu und schließe ihn bei Bedarf in Anführungszeichen ein
                            sb.Append(reader[i].ToString().Replace(";", ",")); // Escape Semikolon in Daten
                            if (i < reader.FieldCount - 1)
                            {
                                sb.Append(";");
                            }
                        }
                        writer.WriteLine(sb.ToString());
                    }
                }
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

    }
}
