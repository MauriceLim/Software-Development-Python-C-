using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonteCarloSimulatorAPI.Data;
using MonteCarloSimulatorAPI.DataModels;

namespace MonteCarloSimulatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangesController : ControllerBase
    {
        private readonly FinancialDbContext _context;

        public ExchangesController(FinancialDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Exchange>>> GetExchanges()
        {
            return await _context.Exchanges.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Exchange>> GetExchange(int id)
        {
            var exchange = await _context.Exchanges.FindAsync(id);
            if (exchange == null)
            {
                return NotFound();
            }
            return exchange;
        }

        [HttpPost]
        public async Task<ActionResult<Exchange>> CreateExchange(Exchange exchange)
        {
            // Validate fields
            if (string.IsNullOrWhiteSpace(exchange.Name) ||
                string.IsNullOrWhiteSpace(exchange.Country) ||
                string.IsNullOrWhiteSpace(exchange.Currency))
            {
                return BadRequest(new { Error = "Name, Country, and Currency are required." });
            }

            _context.Exchanges.Add(exchange);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExchange), new { id = exchange.ExchangeID }, exchange);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExchange(int id, Exchange exchange)
        {
            if (id != exchange.ExchangeID)
            {
                return BadRequest(new { Error = "Exchange ID mismatch." });
            }

            if (string.IsNullOrWhiteSpace(exchange.Name) || 
                string.IsNullOrWhiteSpace(exchange.Country) || 
                string.IsNullOrWhiteSpace(exchange.Currency))
            {
                return BadRequest(new { Error = "All fields are required." });
            }

            _context.Entry(exchange).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Exchanges.Any(e => e.ExchangeID == id))
                {
                    return NotFound(new { Error = "Exchange not found." });
                }
                throw;
            }

            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExchange(int id)
        {
            var exchange = await _context.Exchanges.FindAsync(id);

            if (exchange == null)
            {
                return NotFound(new { Error = "Exchange not found." });
            }

            _context.Exchanges.Remove(exchange);
            await _context.SaveChangesAsync();

            // Reset the sequence dynamically
            var resetSequenceQuery = @"
                SELECT setval(pg_get_serial_sequence('""Exchanges""', 'ExchangeID'), 
                            COALESCE((SELECT MAX(""ExchangeID"") FROM ""Exchanges""), 1));
            ";

            await _context.Database.ExecuteSqlRawAsync(resetSequenceQuery);

            return NoContent();
        }

    }
}
