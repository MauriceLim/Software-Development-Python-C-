using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonteCarloSimulatorAPI.Data;
using MonteCarloSimulatorAPI.DataModels;

namespace MonteCarloSimulatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricesController : ControllerBase
    {
        private readonly FinancialDbContext _context;

        public PricesController(FinancialDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPrices()
        {
            var prices = await _context.Prices
                .Include(p => p.Underlying)
                .Select(p => new
                {
                    p.PriceID,
                    p.Date,
                    p.OpenPrice,
                    p.ClosePrice,
                    p.HighPrice,
                    p.LowPrice,
                    p.Volume,
                    p.UnderlyingID,
                    Underlying = p.Underlying != null ? new
                    {
                        p.Underlying.UnderlyingID,
                        p.Underlying.Name,
                        p.Underlying.Symbol
                    } : null
                })
                .ToListAsync();

            return Ok(prices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPrice(int id)
        {
            var price = await _context.Prices
                .Include(p => p.Underlying)
                .Select(p => new
                {
                    p.PriceID,
                    p.Date,
                    p.OpenPrice,
                    p.ClosePrice,
                    p.HighPrice,
                    p.LowPrice,
                    p.Volume,
                    p.UnderlyingID,
                    Underlying = p.Underlying != null ? new
                    {
                        p.Underlying.UnderlyingID,
                        p.Underlying.Name,
                        p.Underlying.Symbol
                    } : null
                })
                .FirstOrDefaultAsync(p => p.PriceID == id);

            if (price == null)
            {
                return NotFound(new { Error = "Price not found." });
            }

            return Ok(price);
        }

        [HttpPost]
        public async Task<ActionResult<Price>> CreatePrice(Price price)
        {
            price.Date = price.Date.ToUniversalTime(); // Convert to UTC before saving
            _context.Prices.Add(price);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPrice), new { id = price.PriceID }, price);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrice(int id, Price price)
        {
            if (id != price.PriceID)
            {
                return BadRequest(new { Error = "PriceID mismatch." });
            }

            price.Date = price.Date.ToUniversalTime(); // Convert to UTC before updating
            _context.Entry(price).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Prices.Any(p => p.PriceID == id))
                {
                    return NotFound(new { Error = "Price not found." });
                }
                throw;
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrice(int id)
        {
            var price = await _context.Prices.FindAsync(id);

            if (price == null)
            {
                return NotFound(new { Error = "Price not found." });
            }

            _context.Prices.Remove(price);
            await _context.SaveChangesAsync();

            // Reset the sequence dynamically
            var resetSequenceQuery = @"
                SELECT setval(pg_get_serial_sequence('""Prices""', 'PriceID'), 
                            COALESCE((SELECT MAX(""PriceID"") FROM ""Prices""), 1));
            ";
            await _context.Database.ExecuteSqlRawAsync(resetSequenceQuery);

            return NoContent();
        }
    }
}
