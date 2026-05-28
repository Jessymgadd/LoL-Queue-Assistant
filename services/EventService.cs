using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using LoL_Queue_Assistant.Models;
using System.Linq;

    namespace LoL_Queue_Assistant.Services
    {
        public class LeagueEventServices
        {
            private ClientWebSocket socket = new();

            private string password = "";
            private string port = "";
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
        public async Task connect(int to_ban, int to_pick)
        {
            if (socket.State == WebSocketState.Open ||
                socket.State == WebSocketState.Connecting)
                return;
            string lockfile = ReadLockfile();
            string[] parts = lockfile.Split(':');
            port = parts[2];
            password = parts[3];
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"riot:{password}"));
            socket.Options.SetRequestHeader("Authorization", $"Basic {auth}");
            socket.Options.RemoteCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
            Uri uri = new Uri($"wss://127.0.0.1:{port}");
            await socket.ConnectAsync(uri, CancellationToken.None);
            await SubscribeToEvent();
            _ = Listen_event(to_ban, to_pick);
        }

        private HttpClient CreateClient()
        {
            HttpClientHandler handler = new();

            handler.ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true;

            HttpClient client = new HttpClient(handler);

            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"riot:{password}"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            return client;
        }

        private async Task Auto_Ban(int to_ban)
        {
            string json = $"{{\"championId\":{to_ban},\"completed\":true}}";
            StringContent content = new(json, Encoding.UTF8, "application/json");
            using HttpClient client = CreateClient();
            await client.PatchAsync($"https://127.0.0.1:{port}/lol-champ-select/v1/session/actions/0", content);
        }
        private async Task Auto_Pick(int to_pick)
        {
            string json = $"{{\"championId\":{to_pick},\"completed\":true}}";
            StringContent content = new(json, Encoding.UTF8, "application/json");
            using HttpClient client = CreateClient();
            await client.PatchAsync($"https://127.0.0.1:{port}/lol-champ-select/v1/session/actions/1", content);
        }
        public async Task Listen_event(int to_ban, int to_pick)
        {
            byte[] buffer = new byte[8192];

            while (socket.State == WebSocketState.Open) {
                WebSocketReceiveResult result = await socket.ReceiveAsync(
                    buffer, CancellationToken.None);
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (message.Contains("/lol-matchmaking/v1/ready-check") &&
                    message.Contains("\"playerResponse\":\"None\"")) {
                    using HttpClient client = CreateClient();
                    await client.PostAsync($"https://127.0.0.1:{port}/lol-matchmaking/v1/ready-check/accept", null);
                }

                if (message.Contains("/lol-champ-select/v1/session") &&
                    message.Contains("\"type\":\"ban\"") &&
                    message.Contains("\"isInProgress\":true")) {
                    await Auto_Ban(to_ban);
                }

                if (message.Contains("/lol-champ-select/v1/session") &&
                    message.Contains("\"type\":\"pick\"") && 
                    message.Contains("\"isInProgress\":true")) {
                    await Auto_Pick(to_pick);
                }
                System.Diagnostics.Debug.WriteLine(message);
                System.Diagnostics.Trace.WriteLine(message);
                File.AppendAllText("events.log", message + "\n");
            }
        }   
    }
}