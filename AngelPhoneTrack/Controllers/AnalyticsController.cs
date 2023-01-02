using AngelPhoneTrack.Data;
using AngelPhoneTrack.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AngelPhoneTrack.Controllers
{
    [ApiController]
    [AngelAuthorized]
    [Route("/analytics")]
    public class AnalyticsController : AngelControllerBase
    {
        private readonly AngelContext _ctx;
        
        public AnalyticsController(AngelContext ctx) => _ctx = ctx;
       
        [HttpGet("lots-weekly")]
        public async Task<IActionResult> GetTurnoverTimeForLotsAsync()
        {
            var week = DateTimeOffset.UtcNow.AddDays(-7);
            var lots = await _ctx.Lots.Where(x => x.Timestamp > week).ToListAsync();

            var timeAverage = lots.Where(x => x.Archived).Select(x =>  x.ArchivedAt!.Value - x.Timestamp);
            return Ok(new { total = lots.Count(), incomplete = lots.Count(x => !x.Archived), averageSeconds = timeAverage.Average(e => e.TotalSeconds) });
        }
    }
}
