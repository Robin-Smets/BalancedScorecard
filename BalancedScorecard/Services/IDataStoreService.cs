using System.Data;

namespace BalancedScorecard.Services
{
    public interface IDataStoreService
    {
        DateTime? FromDateFilter { get; set; }
        DateTime? UntilDateFilter { get; set; }
        DataTableCollection DataTables { get; }

        Task UpdateDataStore();
        Task LoadData();
        Task EncryptDataStore(string key);
        Task DecryptDataStore(string key);

        void EnsureDirectoryExistsAndHidden(string directoryPath);
    }
}
