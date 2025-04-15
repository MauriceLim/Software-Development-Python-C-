using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonteCarloSimulatorAPI.Data;
using MonteCarloSimulatorAPI.DataModels;

namespace MonteCarloSimulatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnderlyingsController : ControllerBase
    {
        private readonly FinancialDbContext _context;

        public UnderlyingsController(FinancialDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUnderlyings()
        {
            var underlyings = await _context.Underlyings
                .Include(u => u.Market)
                .Select(u => new
                {
                    u.UnderlyingID,
                    u.Symbol,
                    u.Name,
                    u.MarketID,
                    Market = u.Market != null ? new
                    {
                        u.Market.MarketID,
                        u.Market.Name
                    } : null
                })
                .ToListAsync();

            return Ok(underlyings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUnderlying(int id)
        {
            var underlying = await _context.Underlyings
                .Include(u => u.Market)
                .Select(u => new
                {
                    u.UnderlyingID,
                    u.Symbol,
                    u.Name,
                    u.MarketID,
                    Market = u.Market != null ? new
                    {
                        u.Market.MarketID,
                        u.Market.Name
                    } : null
                })
                .FirstOrDefaultAsync(u => u.UnderlyingID == id);

            if (underlying == null)
            {
                return NotFound(new { Error = "Underlying not found." });
            }

            return Ok(underlying);
        }

        [HttpPost]
        public async Task<ActionResult<Underlying>> CreateUnderlying(Underlying underlying)
        {
            var marketExists = await _context.Markets.AnyAsync(m => m.MarketID == underlying.MarketID);
            if (!marketExists)
            {
                return BadRequest(new { Error = "Invalid MarketID. The Market does not exist." });
            }

            _context.Underlyings.Add(underlying);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUnderlying), new { id = underlying.UnderlyingID }, underlying);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUnderlying(int id, Underlying underlying)
        {
            if (id != underlying.UnderlyingID)
            {
                return BadRequest(new { Error = "UnderlyingID mismatch." });
            }

            var marketExists = await _context.Markets.AnyAsync(m => m.MarketID == underlying.MarketID);
            if (!marketExists)
            {
                return BadRequest(new { Error = "Invalid MarketID. The Market does not exist." });
            }

            _context.Entry(underlying).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Underlyings.Any(u => u.UnderlyingID == id))
                {
                    return NotFound(new { Error = "Underlying not found." });
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnderlying(int id)
        {
            var underlying = await _context.Underlyings.FindAsync(id);

            if (underlying == null)
            {
                return NotFound(new { Error = "Underlying not found." });
            }

            _context.Underlyings.Remove(underlying);
            await _context.SaveChangesAsync();

            // Reset the sequence dynamically
            var resetSequenceQuery = @"
                SELECT setval(pg_get_serial_sequence('""Underlyings""', 'UnderlyingID'), 
                            COALESCE((SELECT MAX(""UnderlyingID"") FROM ""Underlyings""), 1));
            ";
            await _context.Database.ExecuteSqlRawAsync(resetSequenceQuery);

            return NoContent();
        }
    }
}
