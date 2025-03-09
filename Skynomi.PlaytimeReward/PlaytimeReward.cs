using TShockAPI;
using Skynomi.Utils;
using TerrariaApi.Server;

namespace Skynomi.PlaytimeReward
{
    public class PlaytimeReward : Loader.ISkynomiExtension, Loader.ISkynomiExtensionDisposable
    {
        public string Name => "Playtime Reward";
        public string Description => "Playtime Reward extension for Skynomi";
        public string Author => "Keyou";
        public string Version => "1.0.0";

        public static DateTime lastTime = DateTime.UtcNow;
        public static Dictionary<string, int> onlinePlayers = new();

        public void Initialize()
        {
            ServerApi.Hooks.ServerJoin.Register(Loader.GetPlugin(), PlayerJoin);
            ServerApi.Hooks.ServerLeave.Register(Loader.GetPlugin(), PlayerLeave);

            Skynomi.PlaytimeReward.Database.Initialize().GetAwaiter();
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
                    UpdateTime(1);
                }
            }, cts.Token);
        }

        public void PlayerJoin(JoinEventArgs args)
        {
            UpdateTime();
            Skynomi.PlaytimeReward.Database.CreatePlayer(TShock.Players[args.Who].Name).GetAwaiter();
            try
            {
                onlinePlayers.Add(TShock.Players[args.Who].Name, Skynomi.PlaytimeReward.Database.GetPlaytime(TShock.Players[args.Who].Name).GetAwaiter().GetResult());
            }
            catch (NullReferenceException)
            {
                Skynomi.PlaytimeReward.Database.CreatePlayer(TShock.Players[args.Who].Name).GetAwaiter();
                onlinePlayers.Add(TShock.Players[args.Who].Name, 0);
            }
        }

        public void PlayerLeave(LeaveEventArgs args)
        {
            UpdateTime();
            onlinePlayers.Remove(TShock.Players[args.Who].Name);
        }

        public static void UpdateTime(int status = 0)
        {
            if ((DateTime.UtcNow - lastTime).TotalMinutes < 1)
            {
                return;
            }
            foreach (var plr in onlinePlayers)
            {
                onlinePlayers[plr.Key] += (int)(DateTime.UtcNow - lastTime).TotalMinutes;
                if (status == 1) Skynomi.PlaytimeReward.Database.SaveData(plr.Key, onlinePlayers[plr.Key]).GetAwaiter();
            }
            lastTime = DateTime.UtcNow;
        }

        public void Dispose() {
            cts?.Cancel();
            UpdateTime(1);
        }
    }
}
