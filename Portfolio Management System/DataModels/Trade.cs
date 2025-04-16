using System.Text.Json.Serialization; // For JsonIgnore

namespace MonteCarloSimulatorAPI.DataModels
{
    public class Trade
    {
        public int TradeID { get; set; } // Primary Key
        public int? DerivativeID { get; set; } // Nullable Foreign Key for Derivatives
        public int? UnderlyingID { get; set; } // Nullable Foreign Key for Underlyings
        public int Quantity { get; set; }
        public double TradePrice { get; set; } // Price on Trade Date
        public DateTime TradeDate { get; set; }

        public double CurrentPrice { get; set; } // Nullable field for today's price

        public double MarketValue { get; set; } // Explicitly mapped column

        // Greeks
        public double Delta { get; set; } = 0;
        public double Gamma { get; set; } = 0;
        public double Vega { get; set; } = 0;
        public double Theta { get; set; } = 0;

        // Navigation Properties        
        [JsonIgnore] // Prevent circular reference during serialization
        public Derivative? Derivative { get; set; }
        public Underlying? Underlying { get; set; }
    }
}
