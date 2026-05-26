using System.IO;

namespace LoL_Queue_Assistant.Services
{
    public class ClientDetectionService
    {
        public bool IsClientOpen()
        {
            return File.Exists(@"C:\Riot Games\League of Legends\lockfile");
        }
    }
}