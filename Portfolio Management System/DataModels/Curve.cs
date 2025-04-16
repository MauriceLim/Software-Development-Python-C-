namespace MonteCarloSimulatorAPI.DataModels
{
    public class Curve
    {
        public int CurveID { get; set; }
        public string Name { get; set; } // e.g., "Yield Curve", "Volatility Surface"
        public string Description { get; set; }

        // Navigation property
        public ICollection<Rate>? Rates { get; set; }
    }
}
