using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoL_Queue_Assistant.Services
{
    public class LeagueEventServices
    {
        private ClientWebSocket socket = new();

        private string ReadLockfile()
        {
            using FileStream stream = new FileStream(@"C:\Riot Games\League of Legends\lockfile",
                FileMode.Open, FileAccess. Read, FileShare.ReadWrite);
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        private async Task SubscribeToEvent()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("[5, \"OnJsonApiEvent\"]");
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        public async Task connect()
        {
            if (socket.State == WebSocketState.Open ||
                socket.State == WebSocketState.Connecting)
                return;
            string lockfile = ReadLockfile();
            string[] parts = lockfile.Split(':');
            string port = parts[2];
            string password = parts[3];
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"riot:{password}"));
            socket.Options.SetRequestHeader("Authorization", $"Basic {auth}");
            socket.Options.RemoteCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
            Uri uri = new Uri($"wss://127.0.0.1:{port}");
            await socket.ConnectAsync(uri, CancellationToken.None);
            await SubscribeToEvent();
            _ = Listen_event();
        }
        public async Task Listen_event()
        {
            byte[] buffer = new byte[8192];

            while (socket.State == WebSocketState.Open) {
                WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                File.AppendAllText("events.log", message + "\n");
            }
        }
    }
}