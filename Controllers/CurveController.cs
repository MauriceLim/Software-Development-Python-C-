using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonteCarloSimulatorAPI.Data;
using MonteCarloSimulatorAPI.DataModels;

namespace MonteCarloSimulatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurvesController : ControllerBase
    {
        private readonly FinancialDbContext _context;

        public CurvesController(FinancialDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Curve>>> GetCurves()
        {
            return await _context.Curves.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Curve>> GetCurve(int id)
        {
            var curve = await _context.Curves.FindAsync(id);

            if (curve == null)
            {
                return NotFound(new { Error = "Curve not found." });
            }

            return curve;
        }

        [HttpPost]
        public async Task<ActionResult<Curve>> CreateCurve(Curve curve)
        {
            _context.Curves.Add(curve);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCurve), new { id = curve.CurveID }, curve);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurve(int id, Curve curve)
        {
            if (id != curve.CurveID)
            {
                return BadRequest(new { Error = "CurveID mismatch." });
            }

            _context.Entry(curve).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Curves.Any(c => c.CurveID == id))
                {
                    return NotFound(new { Error = "Curve not found." });
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurve(int id)
        {
            var curve = await _context.Curves.FindAsync(id);

            if (curve == null)
            {
                return NotFound(new { Error = "Curve not found." });
            }

            _context.Curves.Remove(curve);
            await _context.SaveChangesAsync();

            // Reset the sequence dynamically
            var resetSequenceQuery = @"
                SELECT setval(pg_get_serial_sequence('""Curves""', 'CurveID'), 
                            COALESCE((SELECT MAX(""CurveID"") FROM ""Curves""), 1));
            ";
            await _context.Database.ExecuteSqlRawAsync(resetSequenceQuery);

            return NoContent();
        }
    }
}
