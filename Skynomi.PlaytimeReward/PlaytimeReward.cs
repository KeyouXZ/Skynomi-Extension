using TShockAPI;
using System.Reflection;
using Skynomi.Utils;
using TerrariaApi.Server;
using TShockAPI.Hooks;

namespace Skynomi.PlaytimeReward
{
    public class PlaytimeReward : Loader.ISkynomiExtension, Loader.ISkynomiExtensionDisposable, Loader.ISkynomiExtensionReloadable
    {
        public string Name => "Playtime Reward";
        public string Description => "Playtime Reward extension for Skynomi";
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public Version Version => new Version(Assembly.GetExecutingAssembly().GetName().Version.Major, Assembly.GetExecutingAssembly().GetName().Version.Minor, Assembly.GetExecutingAssembly().GetName().Version.Build);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        public string Author => "Keyou";

        private static DateTime lastTime = DateTime.UtcNow;
        private static readonly List<string> onlinePlayers = new List<string>();
        public static Config? config;

        public void Initialize()
        {
            ServerApi.Hooks.ServerJoin.Register(Loader.GetPlugin(), PlayerJoin);
            ServerApi.Hooks.ServerLeave.Register(Loader.GetPlugin(), PlayerLeave);

            Database.Initialize();
            Commands.Initialize();

            config = Config.Read();

            Save();
        }

        public void Reload(ReloadEventArgs args)
        {
            config = Config.Read();
            Save();
        }

        private CancellationTokenSource? cts;

        private void Save()
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

        private void PlayerJoin(JoinEventArgs args)
        {
            UpdateTime();
            Database.CreatePlayer(TShock.Players[args.Who].Name);
            onlinePlayers.Add(TShock.Players[args.Who].Name);
        }

        private void PlayerLeave(LeaveEventArgs args)
        {
            UpdateTime();
            var player = TShock.Players[args.Who];
            if (player != null)
            {
                onlinePlayers.Remove(player.Name);
            }
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
