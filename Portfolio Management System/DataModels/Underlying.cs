namespace MonteCarloSimulatorAPI.DataModels
{
    public class Underlying
    {
        public int UnderlyingID { get; set; }
        public int MarketID { get; set; }
        public string Symbol { get; set; } // Unique identifier for the stock
        public string Name { get; set; }

        // Navigation property
        public Market? Market { get; set; }
        public ICollection<Price>? Prices { get; set; }
        public ICollection<Trade>? Trades { get; set; } // Allow trading underlyings
        public ICollection<Derivative>? Derivatives { get; set; }
    }
}
