using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonteCarloSimulatorAPI.Data;
using MonteCarloSimulatorAPI.DataModels;

namespace MonteCarloSimulatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketsController : ControllerBase
    {
        private readonly FinancialDbContext _context;

        public MarketsController(FinancialDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetMarkets()
        {
            var markets = await _context.Markets
                .Include(m => m.Exchange) // Include the related Exchange
                .Select(m => new
                {
                    m.MarketID,
                    m.Name,
                    m.ExchangeID,
                    Exchange = m.Exchange != null ? new
                    {
                        m.Exchange.ExchangeID,
                        m.Exchange.Name,
                        m.Exchange.Country,
                        m.Exchange.Currency
                    } : null
                })
                .ToListAsync();

            return Ok(markets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetMarket(int id)
        {
            var market = await _context.Markets
                .Include(m => m.Exchange) // Include the related Exchange
                .Select(m => new
                {
                    m.MarketID,
                    m.Name,
                    m.ExchangeID,
                    Exchange = m.Exchange != null ? new
                    {
                        m.Exchange.ExchangeID,
                        m.Exchange.Name,
                        m.Exchange.Country,
                        m.Exchange.Currency
                    } : null
                })
                .FirstOrDefaultAsync(m => m.MarketID == id);

            if (market == null)
            {
                return NotFound();
            }

            return Ok(market);
        }



        [HttpPost]
        public async Task<ActionResult<Market>> CreateMarket(Market market)
        {
            // Validate that the ExchangeID exists
            var exchangeExists = await _context.Exchanges.AnyAsync(e => e.ExchangeID == market.ExchangeID);
            if (!exchangeExists)
            {
                return BadRequest(new { Error = "Invalid ExchangeID. The Exchange does not exist." });
            }

            // Proceed with saving the market
            _context.Markets.Add(market);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMarket), new { id = market.MarketID }, market);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMarket(int id, Market market)
        {
            if (id != market.MarketID)
            {
                return BadRequest();
            }

            _context.Entry(market).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MarketExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarket(int id)
        {
            var market = await _context.Markets.FindAsync(id);

            if (market == null)
            {
                return NotFound(new { Error = "Market not found." });
            }

            _context.Markets.Remove(market);
            await _context.SaveChangesAsync();

            // Reset the sequence dynamically
            var resetSequenceQuery = @"
                SELECT setval(pg_get_serial_sequence('""Markets""', 'MarketID'), 
                            COALESCE((SELECT MAX(""MarketID"") FROM ""Markets""), 1));
            ";

            await _context.Database.ExecuteSqlRawAsync(resetSequenceQuery);

            return NoContent();
        }

        private bool MarketExists(int id)
        {
            return _context.Markets.Any(e => e.MarketID == id);
        }
    }
}
