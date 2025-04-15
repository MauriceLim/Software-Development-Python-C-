using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonteCarloSimulatorAPI.Data;
using MonteCarloSimulatorAPI.DataModels;

namespace MonteCarloSimulatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatesController : ControllerBase
    {
        private readonly FinancialDbContext _context;

        public RatesController(FinancialDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRates()
        {
            var rates = await _context.Rates
                .Include(r => r.Curve)
                .Select(r => new
                {
                    r.RateID,
                    r.Tenor,
                    r.Value,
                    r.CurveID,
                    Curve = r.Curve != null ? new
                    {
                        r.Curve.CurveID,
                        r.Curve.Name,
                        r.Curve.Description
                    } : null
                })
                .ToListAsync();

            return Ok(rates);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRate(int id)
        {
            var rate = await _context.Rates
                .Include(r => r.Curve)
                .Select(r => new
                {
                    r.RateID,
                    r.Tenor,
                    r.Value,
                    r.CurveID,
                    Curve = r.Curve != null ? new
                    {
                        r.Curve.CurveID,
                        r.Curve.Name,
                        r.Curve.Description
                    } : null
                })
                .FirstOrDefaultAsync(r => r.RateID == id);

            if (rate == null)
            {
                return NotFound(new { Error = "Rate not found." });
            }

            return Ok(rate);
        }

        [HttpPost]
        public async Task<ActionResult<Rate>> CreateRate(Rate rate)
        {
            var curveExists = await _context.Curves.AnyAsync(c => c.CurveID == rate.CurveID);
            if (!curveExists)
            {
                return BadRequest(new { Error = "Invalid CurveID. The Curve does not exist." });
            }

            _context.Rates.Add(rate);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRate), new { id = rate.RateID }, rate);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRate(int id, Rate rate)
        {
            if (id != rate.RateID)
            {
                return BadRequest(new { Error = "Rate ID mismatch." });
            }

            _context.Entry(rate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Rates.Any(r => r.RateID == id))
                {
                    return NotFound(new { Error = "Rate not found." });
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRate(int id)
        {
            var rate = await _context.Rates.FindAsync(id);
            if (rate == null)
            {
                return NotFound(new { Error = "Rate not found." });
            }

            _context.Rates.Remove(rate);
            await _context.SaveChangesAsync();

            // Reset sequence dynamically
            var resetSequenceQuery = @"
                SELECT setval(pg_get_serial_sequence('""Rates""', 'RateID'), 
                            COALESCE((SELECT MAX(""RateID"") FROM ""Rates""), 1));
            ";
            await _context.Database.ExecuteSqlRawAsync(resetSequenceQuery);

            return NoContent();
        }
    }
}
