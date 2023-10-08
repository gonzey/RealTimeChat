using Microsoft.AspNetCore.SignalR;

// public class ChatHub : Hub
// {
//     public async Task SendMessage(string user, string message)
//     {
//         await Clients.All.SendAsync("ReceiveMessage", user, message);
//     }
// }

public class ChatHub : Hub
{
    public async Task JoinRoom(string roomName, string user)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("ReceiveMessage", Context.ConnectionId, "Server", $"{user} has joined the room.");
    }

    public async Task LeaveRoom(string roomName, string user)
    {
        await Clients.Group(roomName).SendAsync("ReceiveMessage", Context.ConnectionId, "Server", $"{user} has left the room.");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
    }

    public async Task SendMessageToRoom(string roomName, string user, string message)
    {
        await Clients.Group(roomName).SendAsync("ReceiveMessage", Context.ConnectionId, user, message);
    }
}
