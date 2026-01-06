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

namespace Steam_Achievement_Abuser_Multiple_Runs
{
    class Program
    {
        // Pause between games (ms)
        private static int pausebetweenabuse = 5000;
        // Run/cycle counter (starts at 0, incremented before each cycle)
        private static int runNumber = 0;
        private static Client _SteamClient = null;
        private static List<GameInfo> _Games = new List<GameInfo>();

        static void Main()
        {
            Console.SetWindowSize(140, 36);
            Console.Title = "Steam Achievement Abuser Enhanced | Breno Farias da Silva";
            W("Starting Steam Achievement Abuser Enhanced");
            W("GitHub Repository: https://github.com/BrenoFariasdaSilva/Steam-Achievement-Abuser-Enhanced");
            W();
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

            // Continuous loop: run StartAbuse, wait 1 hour, repeat; keep console open
            while (true)
            {
                try
                {
                    // increment the run counter and announce the cycle
                    runNumber++;
                    W($"Run {runNumber}: starting cycle...");

                    _Games.Clear();
                    AddGames();
                    _Games = _Games.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ToList();
                    W($"Found {_Games.Count} games. Running automatically...");
                    
                    // Estimate total time: keep each game open for pausebetweenabuse ms, then pausebetweenabuse ms gap
                    double estimatedHoursMulti = (_Games.Count * 2.0 * pausebetweenabuse) / 3600000.0;
                    W($"Estimated total time to process {_Games.Count} games: {estimatedHoursMulti:F2} hours (based on {pausebetweenabuse/1000.0:F1}s open + {pausebetweenabuse/1000.0:F1}s gap per game)");
                    W();
                    StartAbuse();
                }
                catch (Exception ex)
                {
                    W($"Unexpected error: {ex.Message}");
                }

                W("Cycle complete. Waiting 1 hour before next run...");
                Thread.Sleep(TimeSpan.FromHours(1));
            }
        }

        static void StartAbuse()
        {
            W($"Starting abuse (multiple runs) - Run {runNumber}...");
            int i = 1;
            foreach (var Game in _Games)
            {
                ProcessStartInfo ps = new ProcessStartInfo("Steam Achievement Abuser App.exe", Game.Id.ToString());
                ps.CreateNoWindow = true;
                ps.UseShellExecute = false;
                W($"{i}/{_Games.Count()} | {Game.Name}");
                using (Process p = Process.Start(ps))
                {
                    // Ensure the process remains "open" for at least `pausebetweenabuse` ms.
                    var sw = Stopwatch.StartNew();
                    bool exited = p.WaitForExit(pausebetweenabuse);
                    if (!exited)
                    {
                        try
                        {
                            if (p.CloseMainWindow())
                            {
                                if (!p.WaitForExit(1000))
                                    p.Kill();
                            }
                            else
                            {
                                p.Kill();
                            }
                        }
                        catch
                        {
                            try { p.Kill(); } catch { }
                        }
                        p.WaitForExit();
                    }
                    else
                    {
                        var elapsed = (int)sw.ElapsedMilliseconds;
                        if (elapsed < pausebetweenabuse)
                            Thread.Sleep(pausebetweenabuse - elapsed);
                    }
                }
                i++;
                // Wait another `pausebetweenabuse` ms between closing and next launch
                Thread.Sleep(pausebetweenabuse);
            }
            W("Done for this cycle.");
        }

        static void AddGames()
        {
            W("Downloading base...");
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

        private static string ToTitle(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());
        }

        private static void W()
        {
            Console.WriteLine();
        }

        private static void W(string s)
        {
            Console.WriteLine(ToTitle(s));
        }

        private static void W(string format, params object[] args)
        {
            string s;
            try { s = string.Format(format, args); }
            catch { s = format; }
            Console.WriteLine(ToTitle(s));
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
