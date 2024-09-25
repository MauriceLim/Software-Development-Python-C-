using System;

class MonteCarloOptionPricing
{
    public class Config
    {
        public bool Antithetic { get; set; }
        public bool ControlVariate { get; set; }
    }

    static void Main(string[] args)
    {
        // User input for option parameters
        Console.WriteLine("Enter Strike Price (K): ");
        double K = Convert.ToDouble(Console.ReadLine());

        Console.WriteLine("Enter Time to Maturity in years (T): ");
        double T = Convert.ToDouble(Console.ReadLine());

        Console.WriteLine("Enter Current Stock Price (S): ");
        double S = Convert.ToDouble(Console.ReadLine());

        Console.WriteLine("Enter Volatility (sig): ");
        double sig = Convert.ToDouble(Console.ReadLine());

        Console.WriteLine("Enter Risk-free Rate (r): ");
        double r = Convert.ToDouble(Console.ReadLine());

        Console.WriteLine("Enter Dividend Yield (div): ");
        double div = Convert.ToDouble(Console.ReadLine());

        Console.WriteLine("Enter Number of Time Steps (N): ");
        int N = Convert.ToInt32(Console.ReadLine());

        Console.WriteLine("Enter Number of Simulations (M): ");
        int M = Convert.ToInt32(Console.ReadLine());

        // User input for variance reduction techniques
        Console.WriteLine("Enable Antithetic Variance Reduction? (true/false): ");
        bool antithetic = Convert.ToBoolean(Console.ReadLine());

        Console.WriteLine("Enable Control Variate Variance Reduction? (true/false): ");
        bool controlVariate = Convert.ToBoolean(Console.ReadLine());

        // Config for enabling variance reduction techniques
        Config config = new Config
        {
            Antithetic = antithetic,
            ControlVariate = controlVariate
        };

        double callValue = MonteCarloEuropeanCallOption(K, T, S, sig, r, div, N, M, config);
        Console.WriteLine($"Call Option Value: {callValue}");
    }

    static double MonteCarloEuropeanCallOption(double K, double T, double S, double sig, double r, double div, int N, int M, Config config)
    {
        double dt = T / N;
        double nuat = (r - div - 0.5 * sig * sig) * dt;
        double sigst = sig * Math.Sqrt(dt);
        double erddt = Math.Exp((r - div) * dt);
        double beta1 = -1;
        double sum_CT = 0;
        double sum_CT2 = 0;

        Random rand = new Random();

        for (int j = 0; j < M; j++) // for each simulation
        {
            double St = S;
            double St2 = S;
            double CV = 0;
            double CV2 = 0;

            for (int i = 0; i < N; i++) // for each time step
            {
                double t = (i - 1) * dt;
                double delta = BlackScholesDelta(St, t, K, T, sig, r, div);
                double delta2 = BlackScholesDelta(St2, t, K, T, sig, r, div);
                double z = NormalSample(rand);

                double Stn = St * Math.Exp(nuat + sigst * z);
                double Stn2 = St2 * Math.Exp(nuat + sigst * -z); // Antithetic path

                if (config.ControlVariate)
                {
                    CV += delta * (Stn - St * erddt);
                    CV2 += delta2 * (Stn2 - St2 * erddt); // Antithetic control variate
                }

                St = Stn;
                St2 = Stn2;
            }

            double CT;
            if (config.Antithetic && config.ControlVariate)
            {
                CT = 0.5 * ((Math.Max(0, St - K) + beta1 * CV) + (Math.Max(0, St2 - K) + beta1 * CV2));
            }
            else if (config.Antithetic)
            {
                CT = 0.5 * (Math.Max(0, St - K) + Math.Max(0, St2 - K));
            }
            else if (config.ControlVariate)
            {
                CT = Math.Max(0, St - K) + beta1 * CV;
            }
            else
            {
                CT = Math.Max(0, St - K); // No variance reduction
            }

            sum_CT += CT;
            sum_CT2 += CT * CT;
        }

        double callValue = sum_CT / M * Math.Exp(-r * T);
        double SD = Math.Sqrt((sum_CT2 - sum_CT * sum_CT / M) * Math.Exp(-2 * r * T) / (M - 1));
        double SE = SD / Math.Sqrt(M);

        Console.WriteLine($"Standard Deviation: {SD}");
        Console.WriteLine($"Standard Error: {SE}");
        return callValue;
    }

    static double BlackScholesDelta(double S, double t, double K, double T, double sig, double r, double div)
    {
        // CDF of standard normal distribution
        double d1 = (Math.Log(S / K) + (r - div + 0.5 * sig * sig) * (T - t)) / (sig * Math.Sqrt(T - t));
        return NormalCdf(d1);
    }

    static double NormalSample(Random rand)
    {
        // Generate a standard normal random variable using Box-Muller transform
        double u1 = rand.NextDouble();
        double u2 = rand.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
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
}
