using System.IO;
using System.Net.WebSockets;
using System.Text;


namespace LoL_Queue_Assistant.Services
{
    public class LeagueEventServices {
        private ClientWebSocket socket = new();

        private string ReadLockfile()
{
            using FileStream stream = new FileStream(@"C:\Riot Games\League of Legends\lockfile",
            FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        public async Task connect()
        {
            string lockfile = ReadLockfile();
            string[] parts = lockfile.Split(':');
            string port = parts[2];
            string password = parts[3];
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"riot:{password}"));
            socket.Options.SetRequestHeader("Authorization", $"Basic {auth}");
            socket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            Uri uri = new Uri($"wss://127.0.0.1:{port}");
            await socket.ConnectAsync(uri, CancellationToken.None);
        }
    }
}
