using AngelPhoneTrack.Data;
using AngelPhoneTrack.Filters;
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AngelPhoneTrack.Controllers
{
    [ApiController]
    [Route("/lots")]
    public class LotGeneratorController : AngelControllerBase
    {
        private readonly AngelContext _ctx;
        private ILocalUtilityReportingService? _rs;

        public LotGeneratorController(AngelContext ctx) =>
            _ctx = ctx;

        [HttpGet("{id}/report")]
        public async Task<IActionResult> GetReportAsync(Guid id, [FromQuery] string name)
        {
            var lot = await _ctx.Lots
                .Include(x => x.Audits)
                .Include(x => x.Assignments)
                    .ThenInclude(x => x.Department)
                .Include(x => x.Tasks)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (lot == null)
                return NotFound();

            var data = new
            {
                lot.Id,
                lot.LotNo,
                CreatedAt = lot.Timestamp,
                lot.Audits.FirstOrDefault(x => x.Type == "LOT_CREATED")!.CreatedBy,
                Assignments = lot.Assignments.Select(x => new
                {
                    x.Department.Name,
                    x.Count
                }),
                Tasks = lot.Tasks.Select(x => new
                {
                    x.Name,
                    x.Completed,
                    x.Category
                }),
                lot.Priority,
                lot.Expiration,
                lot.Grade,
                lot.Model,
                lot.Count,
                PrintedBy = Employee?.Name ?? name ?? "Angel Cellular LLC",
                DueSoon = DateTimeOffset.UtcNow >= lot.Expiration.AddHours(-1) && DateTimeOffset.UtcNow < lot.Expiration,
                Late = lot.Expiration <= DateTimeOffset.UtcNow
            };
            try
            {

                var dataSerialized = JsonSerializer.Serialize(data, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                _rs ??= new LocalReporting().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();

                Console.WriteLine(dataSerialized);
                var rr = new RenderRequest()
                {
                    Template = new Template()
                    {
                        Recipe = Recipe.ChromePdf,
                        Engine = Engine.None,
                        Chrome = new Chrome()
                        {
                            Url = "https://angelpt-reports-rqed7.ondigitalocean.app/?incoming=" + dataSerialized,
                            WaitForJS = true,
                            WaitForNetworkIddle = true,
                            MarginBottom = "20",
                            MarginTop = "20",
                            MarginLeft = "50",
                            MarginRight = "50"
                        },
                        Content = ""
                    },
                    Options = new RenderOptions()
                    {
                        Timeout = 4000
                    }
                };

                var report = await _rs.RenderAsync(rr);
                return File(report.Content, report.Meta.ContentType);
            }
            catch(Exception exception)
            {
                return Content(exception.ToString(), "text/plain");
            }
        }
    }
}
