using SAM.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.XPath;

namespace Steam_Achievement_Abuser_Auto
{
    class Program
    {
        // Default pause between games in milliseconds
        private static int pausebetweenabuse = 5000;
        private static Client _SteamClient = null;
        private static List<GameInfo> _Games = new List<GameInfo>();

        static void Main()
        {
            Console.SetWindowSize(140, 36);
            Console.Title = "Steam Achievement Abuser Enhanced | Breno Farias da Silva";
            Console.WriteLine("Starting Steam Achievement Abuser Enhanced");
            Console.WriteLine("GitHub Repository: https://github.com/BrenoFariasdaSilva/Steam-Achievement-Abuser-Enhanced");
            Console.WriteLine();
            try
            {
                _SteamClient = new Client();
                if (_SteamClient.Initialize(0) == false)
                    return;
            }
            catch (DllNotFoundException)
            {
                throw;
            }

            Console.WriteLine();
            AddGames();
            Console.WriteLine($"Found {_Games.Count()} games. Running automatically...");
            Console.WriteLine();
            StartAbuse();
        }

        static void StartAbuse()
        {
            Console.WriteLine("Starting abuse (auto)...");
            int i = 1;
            foreach (var Game in _Games)
            {
                ProcessStartInfo ps = new ProcessStartInfo("Steam Achievement Abuser App.exe", Game.Id.ToString());
                ps.CreateNoWindow = true;
                ps.UseShellExecute = false;
                Console.WriteLine($"{i}/{_Games.Count()} | {Game.Name}");
                using (Process p = Process.Start(ps))
                    p.WaitForExit();
                i++;
                Thread.Sleep(pausebetweenabuse);
            }
            Console.WriteLine("Done (auto)!");
        }

        static void AddGames()
        {
            Console.WriteLine("Downloading base...");
            var pairs = new List<KeyValuePair<uint, string>>();
            byte[] bytes;
            using (var downloader = new WebClient())
            {
                bytes = downloader.DownloadData(new Uri(string.Format("http://gib.me/sam/games.xml")));
            }
            using (var stream = new MemoryStream(bytes, false))
            {
                var document = new XPathDocument(stream);
                var navigator = document.CreateNavigator();
                var nodes = navigator.Select("/games/game");
                while (nodes.MoveNext())
                {
                    string type = nodes.Current.GetAttribute("type", "");
                    if (type == string.Empty)
                    {
                        type = "normal";
                    }
                    pairs.Add(new KeyValuePair<uint, string>((uint)nodes.Current.ValueAsLong, type));
                }
                foreach (var kv in pairs)
                {
                    AddGame(kv.Key, kv.Value);
                }
            }
        }

        private static void AddGame(uint id, string type)
        {
            if (_Games.Any(i => i.Id == id))
                return;

            if (!_SteamClient.SteamApps003.IsSubscribedApp(id))
                return;

            var info = new GameInfo(id, type);
            info.Name = _SteamClient.SteamApps001.GetAppData(info.Id, "name");
            if (info.Type == "demo" || info.Type == "mod" || info.Type == "junk")
                return;
            _Games.Add(info);
        }
    }
    internal class GameInfo
    {
        private string _Name;
        public uint Id;
        public string Type;
        public string Name
        {
            get { return _Name; }
            set { _Name = value ?? "App " + this.Id.ToString(CultureInfo.InvariantCulture); }
        }
        public GameInfo(uint id, string type)
        {
            this.Id = id;
            this.Type = type;
            this.Name = null;
        }
    }
}
