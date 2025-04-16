using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonteCarloSimulatorAPI.Data;
using MonteCarloSimulatorAPI.DataModels;

namespace MonteCarloSimulatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DerivativesController : ControllerBase
    {
        private readonly FinancialDbContext _context;

        public DerivativesController(FinancialDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetDerivatives()
        {
            var derivatives = await _context.Derivatives
                .Include(d => d.Underlying)
                .Select(d => new
                {
                    d.DerivativeID,
                    d.Symbol,
                    d.Type,
                    d.StrikePrice,
                    d.ExpirationDate,
                    d.IsCall,
                    d.BarrierLevel,
                    d.BarrierType,
                    d.Payout,
                    d.UnderlyingID,
                    Underlying = d.Underlying != null ? new
                    {
                        d.Underlying.UnderlyingID,
                        d.Underlying.Name,
                        d.Underlying.Symbol
                    } : null
                })
                .ToListAsync();

            return Ok(derivatives);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetDerivative(int id)
        {
            var derivative = await _context.Derivatives
                .Include(d => d.Underlying)
                .Select(d => new
                {
                    d.DerivativeID,
                    d.Symbol,
                    d.Type,
                    d.StrikePrice,
                    d.ExpirationDate,
                    d.IsCall,
                    d.BarrierLevel,
                    d.BarrierType,
                    d.Payout,
                    d.UnderlyingID,
                    Underlying = d.Underlying != null ? new
                    {
                        d.Underlying.UnderlyingID,
                        d.Underlying.Name,
                        d.Underlying.Symbol
                    } : null
                })
                .FirstOrDefaultAsync(d => d.DerivativeID == id);

            if (derivative == null)
            {
                return NotFound(new { Error = "Derivative not found." });
            }

            return Ok(derivative);
        }

        [HttpPost]
       public async Task<ActionResult<Derivative>> CreateDerivative(Derivative derivative)
        {
            if (derivative.ExpirationDate.Kind == DateTimeKind.Unspecified)
            {
                derivative.ExpirationDate = DateTime.SpecifyKind(derivative.ExpirationDate, DateTimeKind.Utc);
            }

            var underlyingExists = await _context.Underlyings.AnyAsync(u => u.UnderlyingID == derivative.UnderlyingID);
            if (!underlyingExists)
            {
                return BadRequest(new { Error = "Invalid UnderlyingID. The underlying does not exist." });
            }

            _context.Derivatives.Add(derivative);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDerivative), new { id = derivative.DerivativeID }, derivative);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDerivative(int id, Derivative derivative)
        {
            if (id != derivative.DerivativeID)
            {
                return BadRequest(new { Error = "DerivativeID mismatch." });
            }

            bool isTraded = await _context.Trades.AnyAsync(t => t.DerivativeID == id);
            if (isTraded)
            {
                return BadRequest(new { Error = "Cannot modify a derivative that has already been traded." });
            }

            var underlyingExists = await _context.Underlyings.AnyAsync(u => u.UnderlyingID == derivative.UnderlyingID);
            if (!underlyingExists)
            {
                return BadRequest(new { Error = "Invalid UnderlyingID. The underlying does not exist." });
            }

            derivative.ExpirationDate = derivative.ExpirationDate.ToUniversalTime(); // Convert to UTC before updating
            _context.Entry(derivative).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Derivatives.Any(d => d.DerivativeID == id))
                {
                    return NotFound(new { Error = "Derivative not found." });
                }
                throw;
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDerivative(int id)
        {
            var derivative = await _context.Derivatives.FindAsync(id);

            if (derivative == null)
            {
                return NotFound(new { Error = "Derivative not found." });
            }

            _context.Derivatives.Remove(derivative);
            await _context.SaveChangesAsync();

            // Reset the sequence dynamically
            var resetSequenceQuery = @"
                SELECT setval(pg_get_serial_sequence('""Derivatives""', 'DerivativeID'), 
                            COALESCE((SELECT MAX(""DerivativeID"") FROM ""Derivatives""), 1));
            ";
            await _context.Database.ExecuteSqlRawAsync(resetSequenceQuery);

            return NoContent();
        }
    }
}
