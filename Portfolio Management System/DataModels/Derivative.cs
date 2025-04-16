using System.Text.Json.Serialization; // For JsonIgnore

namespace MonteCarloSimulatorAPI.DataModels
{
    public class Derivative
    {
        public int DerivativeID { get; set; }
        public int UnderlyingID { get; set; }
        public string Symbol { get; set; } // Unique identifier for the derivative
        public string Type { get; set; } // e.g., "European", "Asian", "Barrier", "Digital"
        public double StrikePrice { get; set; }
        public DateTime ExpirationDate { get; set; }

        // Optional properties for specific types
        public bool? IsCall { get; set; } // True for call, false for put
        public double? BarrierLevel { get; set; }
        public string? BarrierType { get; set; } // e.g., "DownAndOut", "UpAndOut"
        public double? Payout { get; set; } // Used for digital options

        // Navigation properties
        public Underlying? Underlying { get; set; }

        [JsonIgnore] // Prevent circular reference during serialization

        public ICollection<Trade>? Trades { get; set; } // Allow trading derivatives
    }
}
