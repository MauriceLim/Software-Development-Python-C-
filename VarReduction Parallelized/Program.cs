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
        public double[] GenerateGBMPath(double S0, double mu, double sigma, double T, int steps, double[] normalRandoms)
        {
            double dt = T / steps;
            double[] W = new double[steps];
            for (int i = 0; i < steps; i++)
            {
                W[i] = normalRandoms[i] * Math.Sqrt(dt); 
            }
            double[] path = new double[steps+1];
            path[0] = S0;

            for (int i = 1; i <= steps; i++)
            {   
                path[i] = path[i - 1] * Math.Exp((mu - 0.5 * sigma * sigma) * dt + sigma * W[i-1]);
            }

            return path;
        }

        public double[] GenerateGBMPathOpposite(double S0, double mu, double sigma, double T, int steps, double[] normalRandoms)
        {
            double dt = T / steps;
            double[] W = new double[steps];
            for (int i = 0; i < steps; i++)
            {
                W[i] = normalRandoms[i] * Math.Sqrt(dt); 
            }
            double[] path = new double[steps+1];
            path[0] = S0;

            for (int i = 1; i <= steps; i++)
            {   
                path[i] = path[i - 1] * Math.Exp((mu - 0.5 * sigma * sigma) * dt - sigma * W[i-1]);
            }

            return path;
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
    }

    public class EvaluationResult{
        public double Price { get; set; }
        public double Delta { get; set; }
        public double Gamma { get; set; }
        public double Vega { get; set; }
        public double Theta { get; set; }
        public double Rho { get; set; }
        public double StandardErrors { get; set; }

        public double PriceAnti { get; set; }
        public double DeltaAnti { get; set; }
        public double GammaAnti { get; set; }
        public double VegaAnti { get; set; }
        public double ThetaAnti { get; set; }
        public double RhoAnti { get; set; }
        public double StandardErrorsAnti { get; set; }

    }

    public abstract class Option {
        public Underlying Underlying { get; set; }
        public DateTime expirationDate { get; set;}
    }

    public class European : Option {
        public double Strike { get; set; }
        public bool IsCall { get; set;}

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
    
    public static class Simulator {
        public static EvaluationResult Evaluate(Option o, YieldCurve c, SimulationParams p, StockPriceGenerator generator) {
            var K = ((European)o).Strike; 
            var T = (o.expirationDate - DateTime.Today).Days / 365d;
            var S0 = o.Underlying.LastPrice;
            var sigma = o.Underlying.surface.Volatility;
            var r = c.GetTenorRate(T);
            Stopwatch timer = Stopwatch.StartNew();

            // Use ConcurrentBag to store results in a thread-safe manner
            ConcurrentBag<double> payoff_values = new ConcurrentBag<double>();
            ConcurrentBag<double> payoff_values1 = new ConcurrentBag<double>();
            ConcurrentBag<double> payoff_values2 = new ConcurrentBag<double>();
            ConcurrentBag<double> payoff_values3 = new ConcurrentBag<double>();
            ConcurrentBag<double> payoff_values4 = new ConcurrentBag<double>();
            ConcurrentBag<double> payoff_values5 = new ConcurrentBag<double>();
            ConcurrentBag<double> payoff_values6 = new ConcurrentBag<double>();
            ConcurrentBag<double> payoff_values7 = new ConcurrentBag<double>();

            if (p.IsParallel) {
                Parallel.For(0, p.Simulations, i => {
                    GausNormGenerator normalvalues = new GausNormGenerator();
                    int steps = p.Steps / 2;
                    if (p.IsVanderCorput) steps = 2;

                    normalvalues.GenerateRetainedGausDoubles(steps, p.IsVanderCorput, p.Base, p.Base2);
                    double[] normalRandoms = normalvalues.RetainedRandoms;

                    // Generate paths
                    double[] path = generator.GenerateGBMPath(S0, r, sigma, T, steps, normalRandoms);
                    double[] path1 = generator.GenerateGBMPath(S0 + 1, r, sigma, T, steps, normalRandoms);
                    double[] path2 = generator.GenerateGBMPath(S0 - 1, r, sigma, T, steps, normalRandoms);
                    double[] path3 = generator.GenerateGBMPath(S0, r, sigma + 0.1, T, steps, normalRandoms);
                    double[] path4 = generator.GenerateGBMPath(S0, r, sigma - 0.1, T, steps, normalRandoms);
                    double[] path5 = generator.GenerateGBMPath(S0, r, sigma, T + 0.1, steps, normalRandoms);
                    double[] path6 = generator.GenerateGBMPath(S0, r + 0.001, sigma, T, steps, normalRandoms);
                    double[] path7 = generator.GenerateGBMPath(S0, r - 0.001, sigma, T, steps, normalRandoms);
                    
                    var ST = path[^1];
                    var ST1 = path1[^1];
                    var ST2 = path2[^1];
                    var ST3 = path3[^1];
                    var ST4 = path4[^1];
                    var ST5 = path5[^1];
                    var ST6 = path6[^1];
                    var ST7 = path7[^1];

                    if (p.IsAntithetic) {
                        // Generate antithetic paths
                        double[] pathanti = generator.GenerateGBMPathOpposite(S0, r, sigma, T, steps, normalRandoms); 
                        double[] path1anti = generator.GenerateGBMPathOpposite(S0 + 1, r, sigma, T, steps, normalRandoms);
                        double[] path2anti = generator.GenerateGBMPathOpposite(S0 - 1, r, sigma, T, steps, normalRandoms);
                        double[] path3anti = generator.GenerateGBMPathOpposite(S0, r, sigma + 0.1, T, steps, normalRandoms);
                        double[] path4anti = generator.GenerateGBMPathOpposite(S0, r, sigma - 0.1, T, steps, normalRandoms);
                        double[] path5anti = generator.GenerateGBMPathOpposite(S0, r, sigma, T + 0.1, steps, normalRandoms);
                        double[] path6anti = generator.GenerateGBMPathOpposite(S0, r + 0.001, sigma, T, steps, normalRandoms);
                        double[] path7anti = generator.GenerateGBMPathOpposite(S0, r - 0.001, sigma, T, steps, normalRandoms);
                        
                        var STanti = pathanti[^1];
                        var ST1anti = path1anti[^1];
                        var ST2anti = path2anti[^1];
                        var ST3anti = path3anti[^1];
                        var ST4anti = path4anti[^1];
                        var ST5anti = path5anti[^1];
                        var ST6anti = path6anti[^1];
                        var ST7anti = path7anti[^1];

                        // Calculate and add the average payoff for the original and antithetic paths
                        if (((European)o).IsCall) {
                            payoff_values.Add((Math.Max(ST - K, 0) + Math.Max(STanti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values1.Add((Math.Max(ST1 - K, 0) + Math.Max(ST1anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values2.Add((Math.Max(ST2 - K, 0) + Math.Max(ST2anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values3.Add((Math.Max(ST3 - K, 0) + Math.Max(ST3anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values4.Add((Math.Max(ST4 - K, 0) + Math.Max(ST4anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values5.Add((Math.Max(ST5 - K, 0) + Math.Max(ST5anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values6.Add((Math.Max(ST6 - K, 0) + Math.Max(ST6anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values7.Add((Math.Max(ST7 - K, 0) + Math.Max(ST7anti - K, 0)) / 2 * Math.Exp(-r * T));
                        } else {
                            payoff_values.Add((Math.Max(K - ST, 0) + Math.Max(K - STanti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values1.Add((Math.Max(K - ST1, 0) + Math.Max(K - ST1anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values2.Add((Math.Max(K - ST2, 0) + Math.Max(K - ST2anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values3.Add((Math.Max(K - ST3, 0) + Math.Max(K - ST3anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values4.Add((Math.Max(K - ST4, 0) + Math.Max(K - ST4anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values5.Add((Math.Max(K - ST5, 0) + Math.Max(K - ST5anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values6.Add((Math.Max(K - ST6, 0) + Math.Max(K - ST6anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values7.Add((Math.Max(K - ST7, 0) + Math.Max(K - ST7anti, 0)) / 2 * Math.Exp(-r * T));
                        }
                    } 
                    else 
                    {
                        if (((European)o).IsCall) {
                            payoff_values.Add(Math.Max(ST - K, 0) * Math.Exp(-r * T));
                            payoff_values1.Add(Math.Max(ST1 - K, 0) * Math.Exp(-r * T));
                            payoff_values2.Add(Math.Max(ST2 - K, 0) * Math.Exp(-r * T));
                            payoff_values3.Add(Math.Max(ST3 - K, 0) * Math.Exp(-r * T));
                            payoff_values4.Add(Math.Max(ST4 - K, 0) * Math.Exp(-r * T));
                            payoff_values5.Add(Math.Max(ST5 - K, 0) * Math.Exp(-r * T));
                            payoff_values6.Add(Math.Max(ST6 - K, 0) * Math.Exp(-r * T));
                            payoff_values7.Add(Math.Max(ST7 - K, 0) * Math.Exp(-r * T));
                        } 
                        else
                        {
                            payoff_values.Add(Math.Max(K - ST, 0) * Math.Exp(-r * T));
                            payoff_values1.Add(Math.Max(K - ST1, 0) * Math.Exp(-r * T));
                            payoff_values2.Add(Math.Max(K - ST2, 0) * Math.Exp(-r * T));
                            payoff_values3.Add(Math.Max(K - ST3, 0) * Math.Exp(-r * T));
                            payoff_values4.Add(Math.Max(K - ST4, 0) * Math.Exp(-r * T));
                            payoff_values5.Add(Math.Max(K - ST5, 0) * Math.Exp(-r * T));
                            payoff_values6.Add(Math.Max(K - ST6, 0) * Math.Exp(-r * T));
                            payoff_values7.Add(Math.Max(K - ST7, 0) * Math.Exp(-r * T));
                        }
                    } 
                });

            } else {
                for (int i = 0; i < p.Simulations; i++) {
                    GausNormGenerator normalvalues = new GausNormGenerator();
                    int steps = p.Steps / 2;
                    if (p.IsVanderCorput) steps = 2;

                    normalvalues.GenerateRetainedGausDoubles(steps, p.IsVanderCorput, p.Base, p.Base2);
                    double[] normalRandoms = normalvalues.RetainedRandoms;

                    // Generate paths
                    double[] path = generator.GenerateGBMPath(S0, r, sigma, T, steps, normalRandoms);
                    double[] path1 = generator.GenerateGBMPath(S0 + 1, r, sigma, T, steps, normalRandoms);
                    double[] path2 = generator.GenerateGBMPath(S0 - 1, r, sigma, T, steps, normalRandoms);
                    double[] path3 = generator.GenerateGBMPath(S0, r, sigma + 0.1, T, steps, normalRandoms);
                    double[] path4 = generator.GenerateGBMPath(S0, r, sigma - 0.1, T, steps, normalRandoms);
                    double[] path5 = generator.GenerateGBMPath(S0, r, sigma, T + 0.1, steps, normalRandoms);
                    double[] path6 = generator.GenerateGBMPath(S0, r + 0.001, sigma, T, steps, normalRandoms);
                    double[] path7 = generator.GenerateGBMPath(S0, r - 0.001, sigma, T, steps, normalRandoms);

                    var ST = path[^1];
                    var ST1 = path1[^1];
                    var ST2 = path2[^1];
                    var ST3 = path3[^1];
                    var ST4 = path4[^1];
                    var ST5 = path5[^1];
                    var ST6 = path6[^1];
                    var ST7 = path7[^1];

                    if (p.IsAntithetic) {
                        // Generate antithetic paths
                        double[] pathanti = generator.GenerateGBMPathOpposite(S0, r, sigma, T, steps, normalRandoms); 
                        double[] path1anti = generator.GenerateGBMPathOpposite(S0 + 1, r, sigma, T, steps, normalRandoms);
                        double[] path2anti = generator.GenerateGBMPathOpposite(S0 - 1, r, sigma, T, steps, normalRandoms);
                        double[] path3anti = generator.GenerateGBMPathOpposite(S0, r, sigma + 0.1, T, steps, normalRandoms);
                        double[] path4anti = generator.GenerateGBMPathOpposite(S0, r, sigma - 0.1, T, steps, normalRandoms);
                        double[] path5anti = generator.GenerateGBMPathOpposite(S0, r, sigma, T + 0.1, steps, normalRandoms);
                        double[] path6anti = generator.GenerateGBMPathOpposite(S0, r + 0.001, sigma, T, steps, normalRandoms);
                        double[] path7anti = generator.GenerateGBMPathOpposite(S0, r - 0.001, sigma, T, steps, normalRandoms);

                        var STanti = pathanti[^1];
                        var ST1anti = path1anti[^1];
                        var ST2anti = path2anti[^1];
                        var ST3anti = path3anti[^1];
                        var ST4anti = path4anti[^1];
                        var ST5anti = path5anti[^1];
                        var ST6anti = path6anti[^1];
                        var ST7anti = path7anti[^1];

                        if (((European)o).IsCall) {
                            payoff_values.Add((Math.Max(ST - K, 0) + Math.Max(STanti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values1.Add((Math.Max(ST1 - K, 0) + Math.Max(ST1anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values2.Add((Math.Max(ST2 - K, 0) + Math.Max(ST2anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values3.Add((Math.Max(ST3 - K, 0) + Math.Max(ST3anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values4.Add((Math.Max(ST4 - K, 0) + Math.Max(ST4anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values5.Add((Math.Max(ST5 - K, 0) + Math.Max(ST5anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values6.Add((Math.Max(ST6 - K, 0) + Math.Max(ST6anti - K, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values7.Add((Math.Max(ST7 - K, 0) + Math.Max(ST7anti - K, 0)) / 2 * Math.Exp(-r * T));
                        } else {
                            payoff_values.Add((Math.Max(K - ST, 0) + Math.Max(K - STanti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values1.Add((Math.Max(K - ST1, 0) + Math.Max(K - ST1anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values2.Add((Math.Max(K - ST2, 0) + Math.Max(K - ST2anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values3.Add((Math.Max(K - ST3, 0) + Math.Max(K - ST3anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values4.Add((Math.Max(K - ST4, 0) + Math.Max(K - ST4anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values5.Add((Math.Max(K - ST5, 0) + Math.Max(K - ST5anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values6.Add((Math.Max(K - ST6, 0) + Math.Max(K - ST6anti, 0)) / 2 * Math.Exp(-r * T));
                            payoff_values7.Add((Math.Max(K - ST7, 0) + Math.Max(K - ST7anti, 0)) / 2 * Math.Exp(-r * T));
                        }
                    } else {
                        if (((European)o).IsCall) {
                            payoff_values.Add(Math.Max(ST - K, 0) * Math.Exp(-r * T));
                            payoff_values1.Add(Math.Max(ST1 - K, 0) * Math.Exp(-r * T));
                            payoff_values2.Add(Math.Max(ST2 - K, 0) * Math.Exp(-r * T));
                            payoff_values3.Add(Math.Max(ST3 - K, 0) * Math.Exp(-r * T));
                            payoff_values4.Add(Math.Max(ST4 - K, 0) * Math.Exp(-r * T));
                            payoff_values5.Add(Math.Max(ST5 - K, 0) * Math.Exp(-r * T));
                            payoff_values6.Add(Math.Max(ST6 - K, 0) * Math.Exp(-r * T));
                            payoff_values7.Add(Math.Max(ST7 - K, 0) * Math.Exp(-r * T));
                        } else {
                            payoff_values.Add(Math.Max(K - ST, 0) * Math.Exp(-r * T));
                            payoff_values1.Add(Math.Max(K - ST1, 0) * Math.Exp(-r * T));
                            payoff_values2.Add(Math.Max(K - ST2, 0) * Math.Exp(-r * T));
                            payoff_values3.Add(Math.Max(K - ST3, 0) * Math.Exp(-r * T));
                            payoff_values4.Add(Math.Max(K - ST4, 0) * Math.Exp(-r * T));
                            payoff_values5.Add(Math.Max(K - ST5, 0) * Math.Exp(-r * T));
                            payoff_values6.Add(Math.Max(K - ST6, 0) * Math.Exp(-r * T));
                            payoff_values7.Add(Math.Max(K - ST7, 0) * Math.Exp(-r * T));
                        }
                    }
                }

            }

            timer.Stop();
            Console.WriteLine($"Execution Time: {timer.ElapsedMilliseconds} ms");


            EvaluationResult result = new EvaluationResult();
            result.Price = payoff_values.Average(); // This is the Option Price
            result.Delta = (payoff_values1.Average() -  payoff_values2.Average())/2; //dS = 1
            result.Gamma = payoff_values1.Average() - 2*payoff_values.Average() + payoff_values2.Average(); // dS = 1
            result.Vega = (payoff_values3.Average() -  payoff_values4.Average())/ 0.2;  //dSigma = 0.1
            result.Theta = -(payoff_values5.Average() -  payoff_values.Average())/ 0.1;  //dT= 0.1
            result.Rho = (payoff_values6.Average() -  payoff_values7.Average())/ 0.002;  //r = 0.001
            
            // Calculate the sum of squares of differences between each payoff value and the mean
            double sumOfSquares = payoff_values.Sum(x => Math.Pow(x - payoff_values.Average(), 2));
            double standardError = Math.Sqrt(sumOfSquares / (payoff_values.Count - 1)) / Math.Sqrt(payoff_values.Count);
            result.StandardErrors = standardError;

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
            European e = new European();
            SimulationParams p = new SimulationParams();

            // Prompt user for input
            Console.WriteLine("Enter Ticker:");
            u.Ticker = Console.ReadLine();
             
            Console.WriteLine("Enter Last Price:");
            u.LastPrice = double.Parse(Console.ReadLine());

            u.surface = v;

            Console.WriteLine("Enter Strike:");
            e.Strike = double.Parse(Console.ReadLine());

            Console.WriteLine("Enter Volatility:");
            v.Volatility  = double.Parse(Console.ReadLine());
            
            Console.WriteLine("Enter Expiration Date (YYYY-MM-DD):");
            e.expirationDate = DateTime.Parse(Console.ReadLine());

            Console.WriteLine("Is it a call option? (true/false):");
            e.IsCall = bool.Parse(Console.ReadLine());
            
            e.Underlying = u;

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

           // Evaluate
            var result = Simulator.Evaluate(e, c, p, s);

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
