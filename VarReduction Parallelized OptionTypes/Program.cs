using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace MyApp
{
    public class VanDerCorput   // Generates a sequence of Van der Corput values
    {
        public List<double> GenerateSequence(int n, int baseValue)
        {
            List<double> sequence = new List<double>();
            for (int i = 1; i <= n; i++)
            {
                double vdc = CalculateVanDerCorput(i, baseValue);
                sequence.Add(vdc);
            }

            return sequence;
        }

        private double CalculateVanDerCorput(int v, int baseValue)
        {
            List<int> binaryRepresentation = GetBaseXRepresentation(v, baseValue);
            double vdc = 0;
            for (int j = 0; j < binaryRepresentation.Count; j++)
            {
                vdc += binaryRepresentation[j] * Math.Pow(baseValue, -(j + 1));
            }
            return vdc;
        }

        private List<int> GetBaseXRepresentation(int v, int baseValue)
        {
            List<int> binary = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                binary.Add(v % baseValue);
                v = v / baseValue;
            }
            return binary;
        }
    }
    public class GausNormGenerator // Method to generate rvs from N(0,1) and retain them 
    {
        public double[] RetainedRandoms { get; set; }
        public static double[] NextGaussDouble(int n, bool applyVanderCorput, int b1, int b2)  // Static class (do not access this using instance)
        {
            int listlength = 2*n; 
            if (applyVanderCorput == true) {
                listlength = 2;
            } 

            double[] results = new double[listlength]; 

            if (applyVanderCorput == true) {
                VanDerCorput vdc = new VanDerCorput();
                Random rnd = new Random();
                int h = rnd.Next(1,256);
                List<double> sequence1 = vdc.GenerateSequence(h, b1);
                List<double> sequence2 = vdc.GenerateSequence(h, b2); 

                //Here we try to convert the values inside the list to normal vals 
                double u1 = sequence1[sequence1.Count - 1];
                double u2 = sequence2[sequence2.Count - 1];
                double z1 = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
                double z2 = Math.Sqrt(-2 * Math.Log(u1)) * Math.Sin(2 * Math.PI * u2);

                results[0] = z1;
                results[1] = z2;
                
            }

            else {
                Random rnd = new Random();
                //Retained values will be doubled as the PolarRejection algo generates a pair of rv for every loop
                for (int i=0; i < listlength; i+=2)
                { 
                    while (true)
                    {
                        // Generate two uniform random values on the unit interval
                        double x1 = rnd.NextDouble() * 2 - 1;
                        double x2 = rnd.NextDouble() * 2 - 1;

                        double w = x1 * x1 + x2 * x2;

                        if (w <= 1)
                        {
                            double c = Math.Sqrt(-2 * Math.Log(w) / w);
                            double z1 = c * x1;
                            double z2 = c * x2;
                            results[i] = z1;
                            results[i+1] = z2;           
                            break;
                        }
                    }
                }
            }
            return results;
        }
        public void GenerateRetainedGausDoubles(int n, bool applyVanderCorput,int b1, int b2)
        {
            
             RetainedRandoms = NextGaussDouble(n, applyVanderCorput, b1, b2);   
        }
    }

    public class StockPriceGenerator  // Created a new class just in case user is curious about the stock paths and want to graph it
    {
        static double BlackScholesDelta(double S, double t, double K, double T, double sig, double r, bool isCall)
        {
            double d1 = (Math.Log(S / K) + (r  + 0.5 * sig * sig) * (T - t)) / (sig * Math.Sqrt(T - t));
            if (isCall)
            {
                return NormalCdf(d1); // Call option delta
            }
            else
            {
                return NormalCdf(d1) - 1; // Put option delta
            }
        }
        static double NormalCdf(double x)
        {
            // CDF of standard normal distribution using Erf
            return 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
        }

        static double Erf(double z)
        {
            double p = 0.3275911;
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;

            int sign = z < 0.0 ? -1 : 1;

            double x = Math.Abs(z) / Math.Sqrt(2.0);
            double t = 1.0 / (1.0 + p * x);
            double erf = 1.0 - (((((a5 * t + a4) * t) + a3)
                * t + a2) * t + a1) * t * Math.Exp(-x * x);
            return 0.5 * (1.0 + sign * erf);
        }

        public (double[] path, double CV) GenerateGBMPath(double S0, double mu, double K, double sigma, double T, int steps, double[] normalRandoms, SimulationParams p, bool isCall)
        {
            double dt = T / steps;
            double[] W = new double[steps];
            for (int i = 0; i < steps; i++)
            {
                W[i] = normalRandoms[i] * Math.Sqrt(dt); 
            }
            double[] path = new double[steps+1];
            path[0] = S0;

            double CV = 0; // Control variate accumulation
            double erddt = Math.Exp(mu * dt); // Expected return adjustment for CV
           

            for (int i = 1; i <= steps; i++)
            {                   
                // Compute delta and control variate if enabled
                if (p.enableControlVariate)
                {
                    double t = (i) * dt;
                    double delta = BlackScholesDelta(path[i - 1], t, K, T, sigma, mu, isCall);
                    double Stn = path[i - 1] * Math.Exp((mu - 0.5 * sigma * sigma) * dt + sigma * W[i - 1]);
                    CV += delta * (Stn - path[i - 1] * erddt); // Control variate update
                }
                path[i] = path[i - 1] * Math.Exp((mu - 0.5 * sigma * sigma) * dt + sigma * W[i-1]);
            }
            return (path, CV);
        }

        public (double[] path, double CV2) GenerateGBMPathOpposite(double S0, double mu, double K, double sigma, double T, int steps, double[] normalRandoms, SimulationParams p, bool isCall)
        {
            double dt = T / steps;
            double[] W = new double[steps];
            for (int i = 0; i < steps; i++)
            {
                W[i] = normalRandoms[i] * Math.Sqrt(dt); 
            }
            double[] path = new double[steps+1];
            path[0] = S0;
            
            double CV2 = 0; // Control variate accumulation
            double erddt = Math.Exp(mu * dt); // Expected return adjustment for CV

            for (int i = 1; i <= steps; i++)
            {   
                // Compute delta and control variate if enabled
                if (p.enableControlVariate)
                {
                    double t = (i) * dt;
                    double delta = BlackScholesDelta(path[i - 1], t, K, T, sigma, mu, isCall);
                    double Stn = path[i - 1] * Math.Exp((mu - 0.5 * sigma * sigma) * dt - sigma * W[i - 1]);
                    CV2 += delta * (Stn - path[i - 1] * erddt); // Control variate update
                }

                path[i] = path[i - 1] * Math.Exp((mu - 0.5 * sigma * sigma) * dt - sigma * W[i-1]);
            }

            return (path, CV2);
        }

    }

    public class SimulationParams {
        public int Steps { get; set; }
        public int Simulations { get; set; }
        public bool IsAntithetic { get; set; }
        public bool IsVanderCorput { get; set; }
        public int Base { get; set; }
        public int Base2 { get; set; }
        public bool IsParallel { get; set; }
        public bool enableControlVariate { get; set; }

    }

    public class EvaluationResult{
        public double Price { get; set; }
        public double Delta { get; set; }
        public double Gamma { get; set; }
        public double Vega { get; set; }
        public double Theta { get; set; }
        public double Rho { get; set; }
        public double StandardErrors { get; set; }

    }

    public abstract class Option {
        public Underlying Underlying { get; set; }
        public DateTime expirationDate { get; set;}
        
        public abstract double CalculatePayoff(double[] path, double T, double r);
    }

    public class European : Option {
        public double Strike { get; set; }
        public bool IsCall { get; set;}

        public override double CalculatePayoff(double[] path, double T, double r)
        {
            double ST = path[^1];
            double payoff = (IsCall ? Math.Max(ST - Strike, 0) : Math.Max(Strike - ST, 0));
            return payoff * Math.Exp(-r * T);
        }

    }

    public class Asian : Option
    {
        public double Strike { get; set; }
        public bool IsCall { get; set;}

        public override double CalculatePayoff(double[] path, double T, double r)
        {
            double averagePrice = path.Average();
            double payoff = (IsCall ? Math.Max(averagePrice - Strike, 0) : Math.Max(Strike - averagePrice, 0));
            return payoff * Math.Exp(-r * T);
        }
    }
    public class LookbackOption : Option
    {
        public double Strike { get; set; }
        public bool IsCall { get; set; } // If true, it's a Call, otherwise it's a Put

        public override double CalculatePayoff(double[] path, double T, double r)
        {
            double maxPrice = path.Max();
            double minPrice = path.Min();
            
            double payoff = 0;
            if (IsCall)
            {
                // Fixed strike call
                payoff = Math.Max(maxPrice - Strike, 0);
            }
            else
            {
                // Fixed strike put
                payoff = Math.Max(Strike - minPrice, 0);
            }
            
            return payoff * Math.Exp(-r * T); // Discounted payoff
        }
    }

    public class BarrierOption : Option
    {
        public double Strike { get; set; }
        public double Barrier { get; set; }
        public BarrierType Type { get; set; } // DownAndOut, UpAndOut, DownAndIn, UpAndIn
        public bool IsCall { get; set; }

        // Calculate the payoff for a barrier option
        public override double CalculatePayoff(double[] path, double T, double r)
        {
            // Check if the path breaches the barrier
            bool breachedBarrier = false;
            double minPrice = path.Min();
            double maxPrice = path.Max();

            // Determine if the option has been knocked out or knocked in
            switch (Type)
            {
                case BarrierType.DownAndOut:
                    breachedBarrier = minPrice < Barrier;
                    break;
                case BarrierType.UpAndOut:
                    breachedBarrier = maxPrice > Barrier;
                    break;
                case BarrierType.DownAndIn:
                    breachedBarrier = minPrice < Barrier;
                    break;
                case BarrierType.UpAndIn:
                    breachedBarrier = maxPrice > Barrier;
                    break;
            }

            // For knockout options, if the barrier is breached, the payoff is 0
            if (breachedBarrier)
            {
                return 0;
            }

            // For knockin options, if the barrier is breached, calculate the payoff at expiration
            double finalPrice = path.Last(); // Final stock price at expiration
            double payoff = (IsCall ? Math.Max(finalPrice - Strike, 0) : Math.Max(Strike - finalPrice, 0));

            // Discount the payoff to present value
            return payoff * Math.Exp(-r * T);
        }
    }

    public enum BarrierType
    {
        DownAndOut,
        UpAndOut,
        DownAndIn,
        UpAndIn
    }

    public class RangeOption : Option
    {
        public bool IsCall { get; set; }
        // Constructor
        public RangeOption()
        {
            // Default values
            IsCall = true; // Range options are generally treated as calls
        }

        public override double CalculatePayoff(double[] path, double T, double r)
        {
            // Calculate the max and min values of the path
            double maxPrice = path.Max();
            double minPrice = path.Min();

            // Calculate the payoff based on the max and min values
            double payoff = maxPrice - minPrice;

            // Return the discounted payoff
            return payoff * Math.Exp(-r * T);
        }
    }

    public class DigitalOption : Option
    {
        public DigitalOption()
        {
            // Default payout, this will be overridden by user input
            Payout = 1.0;
        }

        public double Payout { get; set; }  // Payout value for the option (fixed)
        public bool IsCall { get; set; }    // True for Call, False for Put
        public double Strike { get; set; }

        public override double CalculatePayoff(double[] path, double T, double r)
        {
            double finalPrice = path.Last();

            double payoff = 0;

            if (IsCall && finalPrice >= Strike)
            {
                payoff = Payout;
            }
            else if (!IsCall && finalPrice <= Strike)
            {
                payoff = Payout;
            }

            // Return the discounted payoff
            return payoff * Math.Exp(-r * T);
        }
    }

    public class VolatilitySurface {
        public double Volatility { get; set; }
    }

    public class YieldCurve {
        public double GetTenorRate(double T){
            return 0.05;
        }
    }
    public class Underlying {
        public string Ticker { get; set; }
        public double LastPrice { get; set; }
        public VolatilitySurface surface { get; set; }
    }
    
    public static class Simulator
    {
        // Main evaluation method
        public static EvaluationResult Evaluate(Option o, YieldCurve c, SimulationParams p, StockPriceGenerator generator)
        {
            bool isCall = false;
            double K = 0;

            if (o is European european)
            {
                isCall = european.IsCall;
                K = european.Strike;
            }
            else if (o is Asian asian)
            {
                isCall = asian.IsCall;
                K = asian.Strike;  // Assuming Asian options also have a Strike property
            }
            else if (o is BarrierOption barrier)
            {
                isCall = barrier.IsCall;
                K = barrier.Strike;  // Assuming Asian options also have a Strike property
            }
            else if (o is LookbackOption lookback)
            {
                isCall = lookback.IsCall;
                K = lookback.Strike;  // Assuming Asian options also have a Strike property
            }
            else if (o is RangeOption range)
            {
                isCall = range.IsCall;
            }
            else if (o is DigitalOption digital)
            {
                isCall = digital.IsCall;
                K = digital.Strike;
            }
            else
            {
                throw new ArgumentException("Unsupported option type.");
            }
            double T = (o.expirationDate - DateTime.Today).Days / 365d;
            double S0 = o.Underlying.LastPrice;
            double sigma = o.Underlying.surface.Volatility;
            double r = c.GetTenorRate(T);

            var payoffBags = new ConcurrentBag<double>[8];
            for (int i = 0; i < payoffBags.Length; i++)
            {
                payoffBags[i] = new ConcurrentBag<double>();
            }

            if (p.IsParallel)
            {
                Parallel.For(0, p.Simulations, i =>
                {
                    var normalValues = GenerateNormalValues(p);
                    var (paths, CVs) = GeneratePaths(S0, r, K, sigma, T, p, generator, normalValues, isCall);
                    
                    // For antithetic variates
                    var (pathsAntithetic, CV2s) = p.IsAntithetic ? GenerateAntitheticPaths(S0, r, K, sigma, T, p, generator, normalValues, isCall): (null, null);

                    AddPayoffs(o, paths, pathsAntithetic, K, T, r, payoffBags, CVs, CV2s);
                });
            }
            else
            {
                for (int i = 0; i < p.Simulations; i++)
                {
                    var normalValues = GenerateNormalValues(p);
                    var (paths, CVs) = GeneratePaths(S0, r, K, sigma, T, p, generator, normalValues, isCall);

                    var (pathsAntithetic, CV2s) = p.IsAntithetic ? GenerateAntitheticPaths(S0, r, K, sigma, T, p, generator, normalValues, isCall): (null, null);

                    AddPayoffs(o, paths, pathsAntithetic, K, T, r, payoffBags, CVs, CV2s);
                }
            }

            // Placeholder: Combine the results from `payoffBags` to return an EvaluationResult
            return ProcessResults(payoffBags);
        }

        // Helper method for generating normal random values
        private static double[] GenerateNormalValues(SimulationParams p)
        {
            int steps = p.Steps / 2;
            if (p.IsVanderCorput) steps = 2;

            var normalValues = new GausNormGenerator();
            normalValues.GenerateRetainedGausDoubles(steps, p.IsVanderCorput, p.Base, p.Base2);
            return normalValues.RetainedRandoms;
        }

        // Helper method to generate an array of paths based on initial parameters
        private static (double[][] paths, double[] CVs) GeneratePaths(double S0, double r, double K, double sigma, double T, SimulationParams p, StockPriceGenerator generator, double[] normalRandoms, bool isCall)
        {
            int numPaths = 8; 
            double[][] paths = new double[numPaths][];
            double[] CVs = new double[numPaths];

            for (int i = 0; i < numPaths; i++)
            {
                // Call GenerateGBMPath for each path and control variate
                var (path, CV) = i switch
                {
                    0 => generator.GenerateGBMPath(S0, r, K, sigma, T, p.Steps, normalRandoms, p, isCall),
                    1 => generator.GenerateGBMPath(S0 + 1, r, K, sigma, T, p.Steps, normalRandoms, p, isCall),
                    2 => generator.GenerateGBMPath(S0 - 1, r, K, sigma, T, p.Steps, normalRandoms, p, isCall),
                    3 => generator.GenerateGBMPath(S0, r, K, sigma + 0.1, T, p.Steps, normalRandoms, p, isCall),
                    4 => generator.GenerateGBMPath(S0, r, K, sigma - 0.1, T, p.Steps, normalRandoms, p, isCall),
                    5 => generator.GenerateGBMPath(S0, r, K, sigma, T + 0.1, p.Steps, normalRandoms, p, isCall),
                    6 => generator.GenerateGBMPath(S0, r + 0.001, K, sigma, T, p.Steps, normalRandoms, p, isCall),
                    7 => generator.GenerateGBMPath(S0, r - 0.001, K, sigma, T, p.Steps, normalRandoms, p, isCall),
                    _ => throw new ArgumentOutOfRangeException()
                };

                paths[i] = path;
                CVs[i] = CV; // Store the CV for this path
            }

            return (paths, CVs);
        }

        private static (double[][] paths, double[] CVs) GenerateAntitheticPaths(double S0, double r, double K, double sigma, double T, SimulationParams p, StockPriceGenerator generator, double[] normalRandoms, bool isCall)
        {
            int numPaths = 8; 
            double[][] paths = new double[numPaths][];
            double[] CVs = new double[numPaths];

            for (int i = 0; i < numPaths; i++)
            {
                // Call GenerateGBMPathOpposite for each antithetic path and control variate
                var (path, CV) = i switch
                {
                    0 => generator.GenerateGBMPathOpposite(S0, r, K, sigma, T, p.Steps, normalRandoms, p, isCall),
                    1 => generator.GenerateGBMPathOpposite(S0 + 1, r, K, sigma, T, p.Steps, normalRandoms, p, isCall),
                    2 => generator.GenerateGBMPathOpposite(S0 - 1, r, K, sigma, T, p.Steps, normalRandoms, p, isCall),
                    3 => generator.GenerateGBMPathOpposite(S0, r, K, sigma + 0.1, T, p.Steps, normalRandoms, p, isCall),
                    4 => generator.GenerateGBMPathOpposite(S0, r, K, sigma - 0.1, T, p.Steps, normalRandoms, p, isCall),
                    5 => generator.GenerateGBMPathOpposite(S0, r, K, sigma, T + 0.1, p.Steps, normalRandoms, p, isCall),
                    6 => generator.GenerateGBMPathOpposite(S0, r + 0.001, K, sigma, T, p.Steps, normalRandoms, p, isCall),
                    7 => generator.GenerateGBMPathOpposite(S0, r - 0.001, K, sigma, T, p.Steps, normalRandoms, p, isCall),
                    _ => throw new ArgumentOutOfRangeException()
                };

                paths[i] = path;
                CVs[i] = CV; // Store the CV for this antithetic path
            }

            return (paths, CVs);
        }


        private static void AddPayoffs(Option o, double[][] paths, double[][] antitheticPaths, double K, double T, double r, ConcurrentBag<double>[] payoffBags, double[] CVs, double[] CV2s)
        {
            double beta1 = -1;

            for (int j = 0; j < paths.Length; j++)
            {
                double payoff;

                if (antitheticPaths != null && CVs != null)
                {
                    payoff = 0.5 * ((o.CalculatePayoff(paths[j], T, r) + beta1 * CVs[j]) + (o.CalculatePayoff(antitheticPaths[j], T, r)+ beta1 *  CV2s[j]));
                }  
                else if (antitheticPaths != null)
                {
                    payoff = 0.5 * (o.CalculatePayoff(paths[j],T,r) + o.CalculatePayoff(antitheticPaths[j], T, r));
                }
                else if (CVs != null)
                {
                    payoff = o.CalculatePayoff(paths[j],T,r) + beta1 * CVs[j];
                }
                else
                {
                    payoff = o.CalculatePayoff(paths[j],T,r); // No variance reduction
                }
    
                payoffBags[j].Add(payoff);

            }
        }

        private static EvaluationResult ProcessResults(ConcurrentBag<double>[] payoffBags)
        {
            EvaluationResult result = new EvaluationResult();

            // Retrieve each payoff list from the bags
            var payoffValues = payoffBags[0].ToList();
            var payoffValues1 = payoffBags[1].ToList();
            var payoffValues2 = payoffBags[2].ToList();
            var payoffValues3 = payoffBags[3].ToList();
            var payoffValues4 = payoffBags[4].ToList();
            var payoffValues5 = payoffBags[5].ToList();
            var payoffValues6 = payoffBags[6].ToList();
            var payoffValues7 = payoffBags[7].ToList();

            // Calculate Greeks and option price
            result.Price = payoffValues.Average();
            result.Delta = (payoffValues1.Average() - payoffValues2.Average()) / 2; // dS = 1
            result.Gamma = payoffValues1.Average() - 2 * payoffValues.Average() + payoffValues2.Average(); // dS = 1
            result.Vega = (payoffValues3.Average() - payoffValues4.Average()) / 0.2; // dSigma = 0.1
            result.Theta = -(payoffValues5.Average() - payoffValues.Average()) / 0.1; // dT = 0.1
            result.Rho = (payoffValues6.Average() - payoffValues7.Average()) / 0.002; // dR = 0.001

            // Calculate standard error of the option price
            double priceMean = result.Price;
            double sumOfSquares = payoffValues.Sum(x => Math.Pow(x - priceMean, 2));
            result.StandardErrors = Math.Sqrt(sumOfSquares / (payoffValues.Count - 1)) / Math.Sqrt(payoffValues.Count);

            return result;
        }
    }

    class Program
    {
         static void Main(string[] args)
        {
            // Create instances
            VolatilitySurface v = new VolatilitySurface();
            YieldCurve c = new YieldCurve();
            StockPriceGenerator s = new StockPriceGenerator();
            Underlying u = new Underlying();
            Option option = null;
            SimulationParams p = new SimulationParams();

            // Prompt user for input
            Console.WriteLine("Enter Ticker:");
            u.Ticker = Console.ReadLine();
             
            Console.WriteLine("Enter Last Price:");
            u.LastPrice = double.Parse(Console.ReadLine());

            u.surface = v;

            Console.WriteLine("Select Option Type: ");
            Console.WriteLine("1. European");
            Console.WriteLine("2. Asian");
            Console.WriteLine("3. Barrier");
            Console.WriteLine("4. Lookback");
            Console.WriteLine("5. Range");
            Console.WriteLine("6. Digital");

            int optionChoice = int.Parse(Console.ReadLine());

            switch (optionChoice)
            {
                case 1:  // European Option
                    option = new European();
                    Console.WriteLine("Enter Strike:");
                    ((European)option).Strike = double.Parse(Console.ReadLine());

                    Console.WriteLine("Is this a Call option? (true/false):");
                    ((European)option).IsCall = bool.Parse(Console.ReadLine());
                    break;

                case 2:  // Asian Option
                    option = new Asian();  // Assuming Asian doesn't need a strike, but you could modify if needed
                    Console.WriteLine("Enter Strike:");
                    ((Asian)option).Strike = double.Parse(Console.ReadLine());  // Optional for Asian options, if needed

                    Console.WriteLine("Is this a Call option? (true/false):");
                    ((Asian)option).IsCall = bool.Parse(Console.ReadLine());
                    break;
                
                case 3: // Barrier Option
                    option = new BarrierOption();
                    Console.WriteLine("Enter Strike:");
                    ((BarrierOption)option).Strike = double.Parse(Console.ReadLine());

                    Console.WriteLine("Enter Barrier Level:");
                    ((BarrierOption)option).Barrier = double.Parse(Console.ReadLine());

                    Console.WriteLine("Choose Barrier Type: (1) Down-and-Out (2) Up-and-Out (3) Down-and-In (4) Up-and-In");
                    int barrierTypeChoice = int.Parse(Console.ReadLine());
                    ((BarrierOption)option).Type = (BarrierType)(barrierTypeChoice - 1); // Adjust for enum

                    Console.WriteLine("Is it a Call Option? (true/false):");
                    ((BarrierOption)option).IsCall = bool.Parse(Console.ReadLine());
                    break;
                case 4:
                    // Lookback Option
                    option = new LookbackOption();
                    Console.WriteLine("Enter Strike:");
                    ((LookbackOption)option).Strike = double.Parse(Console.ReadLine());

                    Console.WriteLine("Is it a Call Option? (true/false):");
                    ((LookbackOption)option).IsCall = bool.Parse(Console.ReadLine());; // true for Call, false for Put
                    break;
                 
                case 5:
                    // Lookback Option
                    option = new RangeOption();
                    break;
                
                case 6:
                    // Lookback Option
                    option = new DigitalOption();
                    Console.WriteLine("Enter Strike:");
                    ((DigitalOption)option).Strike = double.Parse(Console.ReadLine());

                    Console.WriteLine("Is it a Call Option? (true/false):");
                    ((DigitalOption)option).IsCall = bool.Parse(Console.ReadLine());; // true for Call, false for Put

                    Console.WriteLine("Enter Payout (e.g. 1 for full payout):");
                    ((DigitalOption)option).Payout = double.Parse(Console.ReadLine()); 
                    break;
                
                default:
                    Console.WriteLine("Invalid option selected.");
                    return;  // Exit the program if invalid option
            }


            Console.WriteLine("Enter Volatility:");
            v.Volatility  = double.Parse(Console.ReadLine());
            
            Console.WriteLine("Enter Expiration Date (YYYY-MM-DD):");
            option.expirationDate = DateTime.Parse(Console.ReadLine());
            
            option.Underlying = u;

            Console.WriteLine("Is it an Van der Corput? (true/false):");
            p.IsVanderCorput = bool.Parse(Console.ReadLine());

            if (p.IsVanderCorput == true) {
                Console.WriteLine("Enter Base1:");
                p.Base = int.Parse(Console.ReadLine());

                Console.WriteLine("Enter Base2:");
                p.Base2 = int.Parse(Console.ReadLine());

            }
            if (p.IsVanderCorput == false) {
                Console.WriteLine("Is it an Antithetic? (true/false):");
                p.IsAntithetic = bool.Parse(Console.ReadLine());

                Console.WriteLine("Enter Number of Steps:");
                p.Steps = int.Parse(Console.ReadLine());
            }


            Console.WriteLine("Enter Number of Simulations:");
            p.Simulations = int.Parse(Console.ReadLine());

            Console.WriteLine("Enable parallelization? (true/false):");
            p.IsParallel = bool.Parse(Console.ReadLine());

            Console.WriteLine("Enable Control Variate? (true/false):");
            p.enableControlVariate = bool.Parse(Console.ReadLine());

           // Evaluate
            var result = Simulator.Evaluate(option, c, p, s);

            // Output with description
            Console.WriteLine($"Price: {result.Price}");
            if (p.IsVanderCorput == false) {
                Console.WriteLine($"Standard Error: {result.StandardErrors}");
            }
            Console.WriteLine($"Delta: {result.Delta}");
            Console.WriteLine($"Gamma: {result.Gamma}");
            Console.WriteLine($"Vega: {result.Vega}");
            Console.WriteLine($"Theta: {result.Theta}");
            Console.WriteLine($"Rho: {result.Rho}");
            
        }
        
    }
}
