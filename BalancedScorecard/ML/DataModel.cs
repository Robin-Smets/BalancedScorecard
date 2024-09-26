namespace BalancedScorecard.ML.DataModel
{
    public class OrderVolumeModel
    {
        public float OrderDateCalenderWeek { get; set; }
        public float OrderDateMonth { get; set; }
        public float OrderDateQuarter { get; set; }
        public float OrderDateYear { get; set; }
        public string TimeUnitCalenderWeek { get; set; }
        public string TimeUnitMonth { get; set; }
        public string TimeUnitQuarter { get; set; }
        public DateTime OrderDate {  get; set; }
        public float SalesOrderID { get; set; }
        public float CustomerID { get; set; }
        public string CustomerName { get; set; }
        public float TerritoryID { get; set; }
        public string TerritoryName { get; set; }
        public float SalesPersonID { get; set; }
        public string SalesPersonName { get; set; }
        public float ProductID { get; set; }
        public string ProductName { get; set; }
        public float OrderVolume { get; set; }
        public float TotalOrderVolume { get; set; }
        public float OrderVolumePercentage { get; set; }
    }
}
