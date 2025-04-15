namespace MonteCarloSimulatorAPI.DataModels
{
    public class Market
    {
        public int MarketID { get; set; }
        public int ExchangeID { get; set; }
        public string Name { get; set; }

        // Navigation properties
        public Exchange? Exchange { get; set; }
        public ICollection<Underlying>? Underlyings { get; set; }
    }
}
