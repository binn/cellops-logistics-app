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
            var lots = await _ctx.Lots
                .Include(x => x.Assignments)
                .Include(x => x.Notes)
                .Include(x => x.Tasks)
                .Include(x => x.Audits)
                .Where(x => x.Timestamp >= week).ToListAsync();

            var totalQuantity = lots.Sum(x => x.Count);
            var averageQuantity = lots.Average(x => x.Count);
            //var averageGB = lots.Average(x => x.GB);

            var completionTimes = lots.Where(x => x.Tasks.All(y => y.Completed))
                .ToDictionary(x => x.LotNo, y => y.Timestamp - y.Tasks.OrderByDescending(t => t.CompletedAt).First().CompletedAt);

            var averageCompletionTime = completionTimes.Average(x => x.Value!.Value.TotalSeconds);

            var lotDeliveryTimes = lots.Where(x => x.Archived)
                .ToDictionary(x => x.LotNo, y => y.Timestamp - y.ArchivedAt);

            var averageDeliveryTime = lotDeliveryTimes.Average(e => e.Value!.Value.TotalSeconds);


            return Ok();
        }
    }
}
