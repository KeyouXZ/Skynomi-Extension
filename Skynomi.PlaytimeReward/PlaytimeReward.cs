using TShockAPI;
using System.Reflection;
using Skynomi.Utils;
using TerrariaApi.Server;

namespace Skynomi.PlaytimeReward
{
    public class PlaytimeReward : Loader.ISkynomiExtension, Loader.ISkynomiExtensionDisposable
    {
        public string Name => "Playtime Reward";
        public string Description => "Playtime Reward extension for Skynomi";
        public Version Version => new Version(Assembly.GetExecutingAssembly().GetName().Version.Major, Assembly.GetExecutingAssembly().GetName().Version.Minor, Assembly.GetExecutingAssembly().GetName().Version.Build);
        public string Author => "Keyou";

        public static DateTime lastTime = DateTime.UtcNow;
        public static List<string> onlinePlayers = new List<string>();

        public void Initialize()
        {
            ServerApi.Hooks.ServerJoin.Register(Loader.GetPlugin(), PlayerJoin);
            ServerApi.Hooks.ServerLeave.Register(Loader.GetPlugin(), PlayerLeave);

            Skynomi.PlaytimeReward.Database.Initialize();
            Skynomi.PlaytimeReward.Commands.Initialize();

            Save();
        }

        private CancellationTokenSource? cts;

        public void Save()
        {
            cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), cts.Token);
                    UpdateTime();
                }
            }, cts.Token);
        }

        public void PlayerJoin(JoinEventArgs args)
        {
            UpdateTime();
            Skynomi.PlaytimeReward.Database.CreatePlayer(TShock.Players[args.Who].Name);
            onlinePlayers.Add(TShock.Players[args.Who].Name);
        }

        public void PlayerLeave(LeaveEventArgs args)
        {
            UpdateTime();
            onlinePlayers.Remove(TShock.Players[args.Who].Name);
        }

        public static void UpdateTime()
        {
            if ((DateTime.UtcNow - lastTime).TotalMinutes < 1)
            {
                return;
            }
            foreach (string plr in onlinePlayers)
            {            
                var PlaytimeCache = Skynomi.Database.CacheManager.Cache.GetCache<int>("Playtime");
                PlaytimeCache.Update(plr, PlaytimeCache.GetValue(plr) + (int)(DateTime.UtcNow - lastTime).TotalMinutes);
            }
            lastTime = DateTime.UtcNow;
        }

        public void Dispose() {
            cts?.Cancel();
            UpdateTime();
        }
    }
}
