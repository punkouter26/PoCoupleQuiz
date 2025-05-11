using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PoCoupleQuiz.Web.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinRoom(string roomId, string playerName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("PlayerJoined", playerName);
        }

        public async Task LeaveRoom(string roomId, string playerName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("PlayerLeft", playerName);
        }

        public async Task SubmitAnswer(string roomId, string playerName, int questionId, string answer)
        {
            await Clients.Group(roomId).SendAsync("AnswerSubmitted", playerName, questionId, answer);
        }

        public async Task StartGame(string roomId)
        {
            await Clients.Group(roomId).SendAsync("GameStarted");
        }

        public async Task EndGame(string roomId)
        {
            await Clients.Group(roomId).SendAsync("GameEnded");
        }
    }
} 