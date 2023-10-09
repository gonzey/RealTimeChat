using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json; // install newtonsoft NuGet package also
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, List<byte[]>> FileChunks = new ConcurrentDictionary<string, List<byte[]>>();

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

    public async Task SendMultimediaToRoom(string roomName, string user, string base64String, string fileType)
    {
        await Clients.Group(roomName).SendAsync("ReceiveMultimedia", user, base64String, fileType);
    }




    public async Task UploadFileChunk(string roomName, string user, string chunkStr, string fileType, string fileId)
    {
            byte[] chunk = ConvertToByteArray(chunkStr); 

    //        if (fileType == "testType")
    // {
    //     Console.WriteLine($"Received test array: {string.Join(", ", chunk)}");  // Should print "1, 2, 3, 4, 5"
    // }
            if (!FileChunks.ContainsKey(fileId))
            {
                FileChunks[fileId] = new List<byte[]>();
            }
            FileChunks[fileId].Add(chunk);
            await Clients.Group(roomName).SendAsync("ReceiveFileChunk", user, chunk.Length, fileType);
    }

    public async Task FinalizeFileUpload(string roomName, string user, string fileType, string fileId)
    {
        if (FileChunks.TryGetValue(fileId, out List<byte[]> chunks))
        {
            // Combine all the chunks
            var fullFile = CombineChunks(chunks);

            // Convert to Base64
            var base64String = Convert.ToBase64String(fullFile);

            // Remove the chunks from the dictionary
            FileChunks.TryRemove(fileId, out _);

            // Send the complete file back to the client
            await Clients.Group(roomName).SendAsync("ReceiveMultimedia", user, base64String, fileType);
        }
    }

    private byte[] CombineChunks(List<byte[]> chunks)
    {
        var totalLength = 0;
        foreach (var chunk in chunks)
        {
            totalLength += chunk.Length;
        }

        var fullFile = new byte[totalLength];
        var offset = 0;

        foreach (var chunk in chunks)
        {
            Buffer.BlockCopy(chunk, 0, fullFile, offset, chunk.Length);
            offset += chunk.Length;
        }

        return fullFile;
    }

       private byte[] ConvertToByteArray(string str)
    {
        return  System.Text.Encoding.UTF8.GetBytes(str);
    }


}
