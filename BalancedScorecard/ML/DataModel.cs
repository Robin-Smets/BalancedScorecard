namespace BalancedScorecard.ML.DataModel
{
    public class OrderVolumeModel
    {
        public int OrderDateCalenderWeek { get; set; }
        public int OrderDateMonth { get; set; }
        public int OrderDateQuarter { get; set; }
        public int OrderDateYear { get; set; }
        public string TimeUnitCalenderWeek { get; set; }
        public string TimeUnitMonth { get; set; }
        public string TimeUnitQuarter { get; set; }
        public DateTime OrderDate {  get; set; }
        public int SalesOrderID { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public int TerritoryID { get; set; }
        public string TerritoryName { get; set; }
        public int SalesPersonID { get; set; }
        public string SalesPersonName { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public float OrderVolume { get; set; }
        public float TotalOrderVolume { get; set; }
        public float OrderVolumePercentage { get; set; }
    }
}
