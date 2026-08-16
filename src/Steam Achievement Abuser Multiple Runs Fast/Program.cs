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

namespace Steam_Achievement_Abuser_Multiple_Runs_Fast
{
    class Program
    {
        private const int PauseBetweenAbuse = 5000;
        private const int ConcurrentGameLimit = 30;

        private static int runNumber = 0;
        private static Client _SteamClient = null;
        private static List<GameInfo> _Games = new List<GameInfo>();

        static void Main()
        {
            Console.SetWindowSize(140, 36);
            Console.Title = "Steam Achievement Abuser Enhanced Fast | Breno Farias da Silva";
            W("Starting Steam Achievement Abuser Enhanced");
            W("Fast multiple runs mode: up to 30 games at once");
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

            while (true)
            {
                try
                {
                    runNumber++;
                    W($"Run {runNumber}: starting fast cycle...");

                    _Games.Clear();
                    AddGames();
                    _Games = _Games.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ToList();

                    int batchCount = GetBatchCount(_Games.Count);
                    TimeSpan estimatedTime = TimeSpan.FromMilliseconds(batchCount * 2.0 * PauseBetweenAbuse);
                    W($"Found {_Games.Count} games. Running automatically in fast parallel mode...");
                    W($"Concurrency limit: {ConcurrentGameLimit} games");
                    W($"Batches this cycle: {batchCount}");
                    W($"Estimated cycle processing time: {estimatedTime.TotalMinutes:F1} minutes ({estimatedTime.TotalHours:F2} hours), based on {PauseBetweenAbuse / 1000.0:F1}s open + {PauseBetweenAbuse / 1000.0:F1}s cooldown per batch");
                    W();
                    StartAbuse();
                }
                catch (Exception ex)
                {
                    W($"Unexpected error: {ex.Message}");
                }

                W("Fast cycle complete. Waiting 1 hour before next run...");
                Thread.Sleep(TimeSpan.FromHours(1));
            }
        }

        static void StartAbuse()
        {
            W($"Starting abuse (multiple runs fast) - Run {runNumber}...");
            if (_Games.Count == 0)
            {
                W("No eligible games found for this cycle.");
                return;
            }

            int batchCount = GetBatchCount(_Games.Count);
            for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                List<GameInfo> batch = _Games
                    .Skip(batchIndex * ConcurrentGameLimit)
                    .Take(ConcurrentGameLimit)
                    .ToList();

                W($"Batch {batchIndex + 1}/{batchCount}: launching {batch.Count} games...");
                ProcessBatch(batch, batchIndex * ConcurrentGameLimit);
                W($"Batch {batchIndex + 1}/{batchCount}: cooldown...");
                Thread.Sleep(PauseBetweenAbuse);
            }

            W("Done for this fast cycle.");
        }

        static void ProcessBatch(List<GameInfo> batch, int gameOffset)
        {
            var running = new List<RunningGame>();
            var batchOpenTime = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < batch.Count; i++)
                {
                    GameInfo game = batch[i];
                    W($"{gameOffset + i + 1}/{_Games.Count} | {game.Name}");
                    RunningGame started = StartGame(game);
                    if (started != null)
                        running.Add(started);
                }
            }
            finally
            {
                if (running.Count > 0)
                    WaitForOpenWindow(batchOpenTime);

                foreach (var item in running)
                {
                    CloseGame(item);
                }
            }
        }

        static void WaitForOpenWindow(Stopwatch openTime)
        {
            int remaining = PauseBetweenAbuse - (int)openTime.ElapsedMilliseconds;
            if (remaining > 0)
                Thread.Sleep(remaining);
        }

        static RunningGame StartGame(GameInfo game)
        {
            try
            {
                var ps = new ProcessStartInfo("Steam Achievement Abuser App.exe", game.Id.ToString(CultureInfo.InvariantCulture));
                ps.CreateNoWindow = true;
                ps.UseShellExecute = false;

                Process process = Process.Start(ps);
                if (process == null)
                {
                    W($"Failed to start helper for {game.Id} ({game.Name}): process was not created.");
                    return null;
                }

                return new RunningGame(game, process);
            }
            catch (Exception ex)
            {
                W($"Failed to start helper for {game.Id} ({game.Name}): {ex.Message}");
                return null;
            }
        }

        static void CloseGame(RunningGame item)
        {
            try
            {
                int remaining = PauseBetweenAbuse - (int)item.OpenTime.ElapsedMilliseconds;
                if (remaining > 0 && item.Process.WaitForExit(remaining))
                    return;

                if (HasExited(item.Process))
                    return;

                W($"Closing helper for {item.Game.Id} ({item.Game.Name})...");
                try
                {
                    if (item.Process.CloseMainWindow())
                    {
                        if (item.Process.WaitForExit(1000))
                            return;
                    }

                    KillProcess(item);
                }
                catch (Exception ex)
                {
                    W($"Error while closing helper for {item.Game.Id} ({item.Game.Name}): {ex.Message}");
                    KillProcess(item);
                }

                WaitForExit(item);
            }
            catch (Exception ex)
            {
                W($"Unexpected cleanup error for {item.Game.Id} ({item.Game.Name}): {ex.Message}");
            }
            finally
            {
                item.Process.Dispose();
            }
        }

        static void KillProcess(RunningGame item)
        {
            try
            {
                if (!HasExited(item.Process))
                    item.Process.Kill();
            }
            catch (Exception ex)
            {
                W($"Failed to kill helper for {item.Game.Id} ({item.Game.Name}): {ex.Message}");
            }
        }

        static void WaitForExit(RunningGame item)
        {
            try
            {
                if (!HasExited(item.Process))
                {
                    W($"Waiting for helper to exit before continuing: {item.Game.Id} ({item.Game.Name})");
                    item.Process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                W($"Failed while waiting for helper for {item.Game.Id} ({item.Game.Name}): {ex.Message}");
            }
        }

        static bool HasExited(Process process)
        {
            try
            {
                return process.HasExited;
            }
            catch
            {
                return true;
            }
        }

        static int GetBatchCount(int gameCount)
        {
            if (gameCount == 0)
                return 0;

            return (gameCount + ConcurrentGameLimit - 1) / ConcurrentGameLimit;
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

    internal class RunningGame
    {
        public GameInfo Game;
        public Process Process;
        public Stopwatch OpenTime;

        public RunningGame(GameInfo game, Process process)
        {
            this.Game = game;
            this.Process = process;
            this.OpenTime = Stopwatch.StartNew();
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
