using AngelPhoneTrack.Data;
using Microsoft.AspNetCore.Mvc;

namespace AngelPhoneTrack.Controllers
{
    [Route("/v1")]
    public class AngelControllerBase : ControllerBase
    {
        public Employee? Employee { get; set; }
    }
}
