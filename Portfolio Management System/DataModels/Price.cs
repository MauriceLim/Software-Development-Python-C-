namespace MonteCarloSimulatorAPI.DataModels
{
    public class Price
    {
        public int PriceID { get; set; }
        public int UnderlyingID { get; set; }
        public DateTime Date { get; set; }
        public double OpenPrice { get; set; }
        public double ClosePrice { get; set; }
        public double HighPrice { get; set; }
        public double LowPrice { get; set; }
        public long Volume { get; set; }

        // Navigation property
        public Underlying? Underlying { get; set; }
    }
}
