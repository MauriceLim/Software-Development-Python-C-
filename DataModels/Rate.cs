namespace MonteCarloSimulatorAPI.DataModels
{
    public class Rate
    {
        public int RateID { get; set; }
        public int CurveID { get; set; }
        public double Tenor { get; set; } // e.g., "1Y", "5Y"
        public double Value { get; set; } // e.g., interest rate

        // Navigation property
        public Curve? Curve { get; set; }
    }
}
