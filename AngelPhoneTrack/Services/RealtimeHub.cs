using Microsoft.AspNetCore.SignalR;

namespace AngelPhoneTrack.Services
{
    public class RealtimeHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var deptName = Context.GetHttpContext()?.Request.Query["department"];
            await RegisterSelf(deptName!);

            Console.WriteLine("New client connected as department: " + deptName);
            await base.OnConnectedAsync();
        }

        public async Task RegisterSelf(string department)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, department.ToUpper());
        }
    }
}
