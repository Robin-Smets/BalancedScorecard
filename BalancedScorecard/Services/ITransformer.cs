using System.Data;

namespace BalancedScorecard.Services
{
    public interface ITransformer
    {
        List<string> GetTopTenIDs(string featureColumn);
        DataTable FilterDataTableByTopTenIDs(DataTable originalData,
                                                    List<string> topSalesPersons,
                                                    List<string> topCustomers,
                                                    List<string> topProducts,
                                                    List<string> topTerritories);

        DataTable CreateAverageOrderVolumeMatrix(DataTable originalData);
    }
}
