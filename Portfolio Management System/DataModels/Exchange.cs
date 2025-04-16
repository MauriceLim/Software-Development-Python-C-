namespace MonteCarloSimulatorAPI.DataModels
{
    public class Exchange
    {
        public int ExchangeID { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }

        // Navigation property
        public ICollection<Market>? Markets { get; set; }
    }
}
