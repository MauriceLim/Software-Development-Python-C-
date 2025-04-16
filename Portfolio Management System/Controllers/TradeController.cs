using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonteCarloSimulatorAPI.Data;
using MonteCarloSimulatorAPI.DataModels;
using MonteCarloSimulatorAPI.Models; // Import models and simulator classes
using System.Net.Http.Json;
using System.Text.Json;


namespace MonteCarloSimulatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradesController : ControllerBase
    {
        private readonly FinancialDbContext _context;

        public TradesController(FinancialDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrades()
        {
            var trades = await _context.Trades
                .Include(t => t.Derivative)
                .Include(t => t.Underlying)
                .Select(t => new
                {
                    t.TradeID,
                    t.DerivativeID,
                    Derivative = t.Derivative != null ? new
                    {
                        t.Derivative.Symbol,
                        t.Derivative.Type,
                        t.Derivative.StrikePrice,
                        t.Derivative.ExpirationDate,
                        t.Derivative.IsCall
                    } : null,
                    t.UnderlyingID,
                    Underlying = t.Underlying != null ? new
                    {
                        t.Underlying.Symbol,
                        t.Underlying.Name
                    } : null,
                    t.Quantity,
                    t.TradePrice,
                    t.TradeDate,
                    t.CurrentPrice,
                    t.MarketValue,
                    t.Delta,
                    t.Gamma,
                    t.Vega,
                    t.Theta
                })
                .ToListAsync();

            return Ok(trades);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetSummary()
        {
            var summary = await _context.Trades
                .Where(t => t.DerivativeID.HasValue) // Only include derivatives
                .GroupBy(t => 1) // Group all trades together
                .Select(g => new
                {
                    TotalMarketValue = g.Sum(t => t.MarketValue),
                    TotalDelta = Math.Round(g.Sum(t => t.Delta), 2),
                    TotalGamma = g.Sum(t => t.Gamma), // No rounding for Gamma
                    TotalVega = Math.Round(g.Sum(t => t.Vega), 2),
                    TotalTheta = Math.Round(g.Sum(t => t.Theta), 2)
                })
                .FirstOrDefaultAsync();

            return Ok(summary ?? new
            {
                TotalMarketValue = 0.0,
                TotalDelta = 0.0,
                TotalGamma = 0.0,
                TotalVega = 0.0,
                TotalTheta = 0.0
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTrade(int id)
        {
            var trade = await _context.Trades
                .Include(t => t.Derivative)
                .Include(t => t.Underlying)
                .Select(t => new
                {
                    t.TradeID,
                    t.DerivativeID,
                    Derivative = t.Derivative != null ? new
                    {
                        t.Derivative.Symbol,
                        t.Derivative.Type,
                        t.Derivative.StrikePrice,
                        t.Derivative.ExpirationDate,
                        t.Derivative.IsCall
                    } : null,
                    t.UnderlyingID,
                    Underlying = t.Underlying != null ? new
                    {
                        t.Underlying.Symbol,
                        t.Underlying.Name
                    } : null,
                    t.Quantity,
                    t.TradePrice,
                    t.TradeDate,
                    t.CurrentPrice,
                    t.MarketValue,
                    t.Delta,
                    t.Gamma,
                    t.Vega,
                    t.Theta
                })
                .FirstOrDefaultAsync(t => t.TradeID == id);

            if (trade == null)
            {
                return NotFound(new { Error = "Trade not found." });
            }

            return Ok(trade);
        }

        [HttpPost]
        public async Task<ActionResult<Trade>> CreateTrade(Trade trade)
        {
            try
            {
                // If no derivative is given, treat it as an underlying trade
                if (!trade.DerivativeID.HasValue)
                {
                    trade.CurrentPrice = trade.UnderlyingID.HasValue
                        ? await _context.Prices
                            .Where(p => p.UnderlyingID == trade.UnderlyingID)
                            .OrderByDescending(p => p.Date)
                            .Select(p => p.ClosePrice)
                            .FirstOrDefaultAsync()
                        : 0;
                    
                    Console.WriteLine($"[LOG] Fetched Current Price: {trade.CurrentPrice}");

                    if (trade.CurrentPrice == 0 && trade.UnderlyingID.HasValue)
                    {
                        return BadRequest(new { Error = "No price data available for the selected underlying." });
                    }

                    trade.Delta = trade.Quantity;
                    trade.Gamma = 0;
                    trade.Vega = 0;
                    trade.Theta = 0;
                }
                else
                {
                    // Derivative logic
                    var derivative = await _context.Derivatives.FindAsync(trade.DerivativeID);
                    if (derivative == null)
                    {
                        return BadRequest(new { Error = "Derivative not found." });
                    }
                    
                    Console.WriteLine($"[LOG] Fetched Derivative: {JsonSerializer.Serialize(derivative)}");

                    // Fetch latest underlying price data
                    var latestPriceData = await _context.Prices
                        .Where(p => p.UnderlyingID == derivative.UnderlyingID)
                        .OrderByDescending(p => p.Date)
                        .FirstOrDefaultAsync();

                    Console.WriteLine($"[LOG] Latest Price Data: {JsonSerializer.Serialize(latestPriceData)}");

                    if (latestPriceData == null || latestPriceData.ClosePrice == 0)
                    {
                        return BadRequest(new { Error = "No price data available for the underlying linked to this derivative." });
                    }

                    // Time to expiration
                    var timeToExpiration = (derivative.ExpirationDate - latestPriceData.Date).TotalDays / 365.0;
                    Console.WriteLine($"[LOG] Time to Expiration: {timeToExpiration}");

                    // Risk-free rate
                    var riskFreeRate = await _context.Rates
                        .Where(r => r.Tenor >= timeToExpiration)
                        .OrderBy(r => r.Tenor)
                        .Select(r => r.Value)
                        .FirstOrDefaultAsync();
                    
                    Console.WriteLine($"[LOG] Risk-Free Rate: {riskFreeRate}");

                    if (riskFreeRate == 0)
                    {
                        return BadRequest(new { Error = "No rate data available for the given time to expiration." });
                    }

                    // Assume 50% volatility
                    double assumedVolatility = 0.5;
                    Console.WriteLine($"[LOG] Assumed Volatility: {assumedVolatility}");


                    // Map the derivative type string to option type code
                    // Adjust this mapping based on your naming conventions
                    int optionTypeCode = derivative.Type switch
                    {
                        "European" => 1,
                        "Asian" => 2,
                        "Lookback" => 3,
                        "Digital" => 4,
                        "Range" => 5,
                        "Barrier" => 6,
                        _ => throw new Exception("Unknown derivative type.")
                    };

                    bool isCall = derivative.IsCall ?? false; // Default to false if null

                    // Build the option and simulation parameters
                    var underlying = new MonteCarloSimulatorAPI.Models.Underlying
                    {
                        Ticker = derivative.Symbol,
                        LastPrice = latestPriceData.ClosePrice,
                        surface = new VolatilitySurface { Volatility = assumedVolatility }
                    };
                    
                    Console.WriteLine($"[LOG] Underlying Price: {underlying.LastPrice}");


                    // Create the appropriate Option object based on optionTypeCode
                    Option option = optionTypeCode switch
                    {
                        1 => new European { Strike = derivative.StrikePrice, IsCall = isCall, Underlying = underlying, expirationDate = derivative.ExpirationDate },
                        2 => new Asian { Strike = derivative.StrikePrice, IsCall = isCall, Underlying = underlying, expirationDate = derivative.ExpirationDate },
                        3 => new LookbackOption { Strike = derivative.StrikePrice, IsCall = isCall, Underlying = underlying, expirationDate = derivative.ExpirationDate },
                        4 => new DigitalOption { Strike = derivative.StrikePrice, IsCall = isCall, Underlying = underlying, expirationDate = derivative.ExpirationDate, Payout = derivative.Payout ?? 1.0 }, 
                        5 => new RangeOption { IsCall = true, Underlying = underlying, expirationDate = derivative.ExpirationDate }, // Range as call
                        6 => new BarrierOption { 
                            Strike = derivative.StrikePrice, 
                            IsCall = isCall, 
                            Underlying = underlying, 
                            expirationDate = derivative.ExpirationDate,
                            Barrier = derivative.BarrierLevel ?? 100.0, // Use database value or default to 100
                            Type = Enum.TryParse<BarrierType>(derivative.BarrierType, out var parsedBarrierType) ? parsedBarrierType : BarrierType.DownAndOut
                        },
                        _ => throw new Exception("Option type code not supported.")
                    };

                    Console.WriteLine($"[LOG] Constructed Option: {JsonSerializer.Serialize(option)}");


                    var simulationParams = new SimulationParams
                    {
                        Steps = 100,
                        Simulations = 10000,
                        IsAntithetic = true,
                        IsVanderCorput = false,
                        Base = 2,
                        Base2 = 3,
                        IsParallel = true,
                        enableControlVariate = true,
                        // If Barrier is used, set these accordingly
                        IsBarrierOption = (optionTypeCode == 6),
                        BarrierLevel = derivative.BarrierLevel ?? 100.0, 
                        BarrierType = Enum.TryParse<BarrierType>(derivative.BarrierType, out var barrierType) ? barrierType : BarrierType.DownAndOut
                    };

                    var yieldCurve = new YieldCurve(riskFreeRate);
                    var stockPriceGenerator = new StockPriceGenerator();

                    // Evaluate option directly using the simulator
                    var result = Simulator.Evaluate(option, yieldCurve, simulationParams, stockPriceGenerator);

                    // Assign computed values
                    trade.CurrentPrice = result.Price;
                    trade.Delta = result.Delta*trade.Quantity;
                    trade.Gamma = result.Gamma*trade.Quantity;
                    trade.Vega = result.Vega*trade.Quantity;
                    trade.Theta = result.Theta*trade.Quantity;

                }

                // MarketValue calculation
                trade.MarketValue = trade.Quantity * (trade.CurrentPrice - trade.TradePrice);
                trade.TradeDate = trade.TradeDate.ToUniversalTime();


                _context.Trades.Add(trade);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTrade), new { id = trade.TradeID }, trade);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while creating the trade.", Details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrade(int id, Trade trade)
        {
            if (id != trade.TradeID)
            {
                return BadRequest(new { Error = "TradeID mismatch." });
            }

            if (!trade.DerivativeID.HasValue)
            {
                var latestPrice = await _context.Prices
                    .Where(p => p.UnderlyingID == trade.UnderlyingID)
                    .OrderByDescending(p => p.Date)
                    .Select(p => p.ClosePrice)
                    .FirstOrDefaultAsync();

                if (latestPrice == 0)
                {
                    return BadRequest(new { Error = "No price data available for the selected underlying." });
                }

                trade.CurrentPrice = latestPrice;
                trade.Delta = trade.Quantity;
                trade.Gamma = 0;
                trade.Vega = 0;
                trade.Theta = 0;
            }
            else
            {
                var derivative = await _context.Derivatives.FindAsync(trade.DerivativeID);
                if (derivative == null)
                {
                    return BadRequest(new { Error = "Derivative not found." });
                }

                var latestPriceData = await _context.Prices
                    .Where(p => p.UnderlyingID == derivative.UnderlyingID)
                    .OrderByDescending(p => p.Date)
                    .FirstOrDefaultAsync();

                if (latestPriceData == null || latestPriceData.ClosePrice == 0)
                {
                    return BadRequest(new { Error = "No price data available for the underlying linked to this derivative." });
                }

                var timeToExpiration = (derivative.ExpirationDate - latestPriceData.Date).TotalDays / 365.0;

                var riskFreeRate = await _context.Rates
                    .Where(r => r.Tenor >= timeToExpiration)
                    .OrderBy(r => r.Tenor)
                    .Select(r => r.Value)
                    .FirstOrDefaultAsync();

                if (riskFreeRate == 0)
                {
                    return BadRequest(new { Error = "No rate data available for the given time to expiration." });
                }

                double assumedVolatility = 0.5;

                int optionTypeCode = derivative.Type switch
                {
                    "European" => 1,
                    "Asian" => 2,
                    "Lookback" => 3,
                    "Digital" => 4,
                    "Range" => 5,
                    "Barrier" => 6,
                    _ => throw new Exception("Unknown derivative type.")
                };

                bool isCall = derivative.IsCall ?? false;

                var underlying = new MonteCarloSimulatorAPI.Models.Underlying
                {
                    Ticker = derivative.Symbol,
                    LastPrice = latestPriceData.ClosePrice,
                    surface = new VolatilitySurface { Volatility = assumedVolatility }
                };

                Option option = optionTypeCode switch
                {
                    1 => new European { Strike = derivative.StrikePrice, IsCall = isCall, Underlying = underlying, expirationDate = derivative.ExpirationDate },
                    2 => new Asian { Strike = derivative.StrikePrice, IsCall = isCall, Underlying = underlying, expirationDate = derivative.ExpirationDate },
                    3 => new LookbackOption { Strike = derivative.StrikePrice, IsCall = isCall, Underlying = underlying, expirationDate = derivative.ExpirationDate },
                    4 => new DigitalOption { Strike = derivative.StrikePrice, IsCall = isCall, Underlying = underlying, expirationDate = derivative.ExpirationDate, Payout = derivative.Payout ?? 1.0 },
                    5 => new RangeOption { IsCall = true, Underlying = underlying, expirationDate = derivative.ExpirationDate },
                    6 => new BarrierOption
                    {
                        Strike = derivative.StrikePrice,
                        IsCall = isCall,
                        Underlying = underlying,
                        expirationDate = derivative.ExpirationDate,
                        Barrier = derivative.BarrierLevel ?? 100.0,
                        Type = Enum.TryParse<BarrierType>(derivative.BarrierType, out var parsedBarrierType) ? parsedBarrierType : BarrierType.DownAndOut
                    },
                    _ => throw new Exception("Option type code not supported.")
                };

                var simulationParams = new SimulationParams
                {
                    Steps = 100,
                    Simulations = 10000,
                    IsAntithetic = true,
                    IsVanderCorput = false,
                    Base = 2,
                    Base2 = 3,
                    IsParallel = true,
                    enableControlVariate = true,
                    IsBarrierOption = (optionTypeCode == 6),
                    BarrierLevel = derivative.BarrierLevel ?? 100.0,
                    BarrierType = Enum.TryParse<BarrierType>(derivative.BarrierType, out var barrierType) ? barrierType : BarrierType.DownAndOut
                };

                var yieldCurve = new YieldCurve(riskFreeRate);
                var stockPriceGenerator = new StockPriceGenerator();

                var result = Simulator.Evaluate(option, yieldCurve, simulationParams, stockPriceGenerator);

                trade.CurrentPrice = result.Price;
                trade.Delta = result.Delta*trade.Quantity;
                trade.Gamma = result.Gamma*trade.Quantity;
                trade.Vega = result.Vega*trade.Quantity;
                trade.Theta = result.Theta*trade.Quantity;
            }

            trade.MarketValue = trade.Quantity * (trade.CurrentPrice - trade.TradePrice);
            trade.TradeDate = trade.TradeDate.ToUniversalTime();

            _context.Entry(trade).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Trades.Any(t => t.TradeID == id))
                {
                    return NotFound(new { Error = "Trade not found." });
                }
                throw;
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrade(int id)
        {
            var trade = await _context.Trades.FindAsync(id);

            if (trade == null)
            {
                return NotFound(new { Error = "Trade not found." });
            }

            _context.Trades.Remove(trade);
            await _context.SaveChangesAsync();

            // Reset sequence dynamically
            var resetSequenceQuery = @"
                SELECT setval(pg_get_serial_sequence('""Trades""', 'TradeID'), 
                            COALESCE((SELECT MAX(""TradeID"") FROM ""Trades""), 1));
            ";
            await _context.Database.ExecuteSqlRawAsync(resetSequenceQuery);

            return NoContent();
        }
    }
}
