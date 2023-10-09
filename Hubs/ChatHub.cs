using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> ConnectionToRoomMap = new ConcurrentDictionary<string, string>();
    private static readonly ConcurrentDictionary<string, List<string>> RoomToUsersMap = new ConcurrentDictionary<string, List<string>>();

    public async Task JoinRoom(string roomName, string user)
    {
        Console.WriteLine($"Joining room {roomName} for user {user}");
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Caller.SendAsync("JoinedRoom", roomName);
        await Clients.Group(roomName).SendAsync("ReceiveMessage", Context.ConnectionId, "Server", $"{user} has joined the room.");
        ConnectionToRoomMap[Context.ConnectionId] = roomName;
        RoomToUsersMap.GetOrAdd(roomName, new List<string>()).Add(user);
        await Clients.Group(roomName).SendAsync("UpdateUserList", RoomToUsersMap[roomName]);

    }
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (ConnectionToRoomMap.TryRemove(Context.ConnectionId, out string roomName))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
            // Assuming you also have a way to map the connection ID back to the user name
            // For now, let's just say "A user" has left
            RoomToUsersMap[roomName].Remove("A user"); // Replace "A user" with actual username if possible
            await Clients.Group(roomName).SendAsync("UpdateUserList", RoomToUsersMap[roomName]);
            await Clients.Group(roomName).SendAsync("ReceiveMessage", Context.ConnectionId, "Server", "A user has left the room.");
        }
        await base.OnDisconnectedAsync(exception);
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

    public async Task SendTorrentDetailsToRoom(string roomName, string user, string fileName, long fileSize, string infoHash)
    {
        Console.WriteLine($"Announcing new torrent in room {roomName} from user {user} with infoHash {infoHash}");
        await Clients.Group(roomName).SendAsync("ReceiveTorrentDetails", user, fileName, fileSize, infoHash);
    }

}
