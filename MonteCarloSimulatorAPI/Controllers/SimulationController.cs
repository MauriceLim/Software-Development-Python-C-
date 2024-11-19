using Microsoft.AspNetCore.Mvc;
using MonteCarloSimulatorAPI.Models;
using System;
using System.Collections.Generic;

namespace MonteCarloSimulatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulationController : ControllerBase
    {
        [HttpPost("evaluate-option")]
        public ActionResult<EvaluationResult> EvaluateOption([FromBody] OptionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Create underlying asset
                var underlying = new Underlying
                {
                    Ticker = request.Ticker,
                    LastPrice = request.LastPrice,
                    surface = new VolatilitySurface { Volatility = request.Volatility }
                };

                // Create the option object based on request
                Option option = request.OptionTypeCode switch
                {
                    1 => new European
                    {
                        Strike = request.StrikePrice,
                        IsCall = request.IsCall,
                        expirationDate = request.ExpirationDate,
                        Underlying = underlying
                    },
                    2 => new Asian
                    {
                        Strike = request.StrikePrice,
                        IsCall = request.IsCall,
                        expirationDate = request.ExpirationDate,
                        Underlying = underlying
                    },
                    3 => new LookbackOption
                    {
                        Strike = request.StrikePrice,
                        IsCall = request.IsCall,
                        expirationDate = request.ExpirationDate,
                        Underlying = underlying
                    },
                    4 => new DigitalOption
                    {
                        Strike = request.StrikePrice,
                        IsCall = request.IsCall,
                        Payout = request.Payout,
                        expirationDate = request.ExpirationDate,
                        Underlying = underlying
                    },
                    5 => new RangeOption
                    {
                        IsCall = request.IsCall,
                        expirationDate = request.ExpirationDate,
                        Underlying = underlying
                    },
                    6 => new BarrierOption
                    {
                        Strike = request.StrikePrice,
                        Barrier = request.BarrierLevel,
                        Type = request.BarrierTypeCode switch
                        {
                            1 => BarrierType.DownAndOut,
                            2 => BarrierType.UpAndOut,
                            3 => BarrierType.DownAndIn,
                            4 => BarrierType.UpAndIn,
                            _ => throw new ArgumentException("Invalid Barrier Type Code. Use 1=DownAndOut, 2=UpAndOut, 3=DownAndIn, 4=UpAndIn.")
                        },
                        IsCall = request.IsCall,
                        expirationDate = request.ExpirationDate,
                        Underlying = underlying
                    },
                    _ => throw new ArgumentException("Invalid Option Type Code. Use 1=European, 2=Asian, 3=Lookback, 4=Digital, 5=Range, 6=Barrier.")
                };

                // Create simulation parameters
                var simulationParams = new SimulationParams
                {
                    Steps = request.Steps,
                    Simulations = request.Simulations,
                    IsAntithetic = request.UseAntithetic,
                    IsVanderCorput = request.UseVanderCorput,
                    Base = request.Base1,
                    Base2 = request.Base2,
                    IsParallel = request.UseParallelization,
                    enableControlVariate = request.UseControlVariate
                };

                // Instantiate necessary objects
                var stockPriceGenerator = new StockPriceGenerator();
                var yieldCurve = new YieldCurve();

                // Evaluate the option
                var result = Simulator.Evaluate(option, yieldCurve, simulationParams, stockPriceGenerator);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("types")]
        public ActionResult GetOptionAndBarrierTypes()
        {
            return Ok(new
            {
                OptionTypes = new Dictionary<int, string>
                {
                    { 1, "European" },
                    { 2, "Asian" },
                    { 3, "Lookback" },
                    { 4, "Digital" },
                    { 5, "Range" },
                    { 6, "Barrier" }
                },
                BarrierTypes = new Dictionary<int, string>
                {
                    { 1, "DownAndOut" },
                    { 2, "UpAndOut" },
                    { 3, "DownAndIn" },
                    { 4, "UpAndIn" }
                }
            });
        }
    }

    // DTO for incoming requests
    public class OptionRequest
    {
        public string Ticker { get; set; }
        public double LastPrice { get; set; }
        public double Volatility { get; set; }
        public int OptionTypeCode { get; set; } // 1=European, 2=Asian, etc.
        public double StrikePrice { get; set; }
        public bool IsCall { get; set; }
        public double Payout { get; set; } // For Digital Options
        public DateTime ExpirationDate { get; set; }
        public double BarrierLevel { get; set; } // For Barrier Options
        public int BarrierTypeCode { get; set; } // 1=DownAndOut, 2=UpAndOut, etc.
        public int Steps { get; set; }
        public int Simulations { get; set; }
        public bool UseAntithetic { get; set; }
        public bool UseVanderCorput { get; set; }
        public int Base1 { get; set; }
        public int Base2 { get; set; }
        public bool UseParallelization { get; set; }
        public bool UseControlVariate { get; set; }
    }
    
}
