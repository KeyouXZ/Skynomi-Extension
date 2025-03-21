using Skynomi.Utils;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Skynomi.Web;
public class Web : Loader.ISkynomiExtension, Loader.ISkynomiExtensionReloadable, Loader.ISkynomiExtensionPostInit, Loader.ISkynomiExtensionDisposable
{
    public string Name => "Web";
    public string Description => "Web Server";
    public Version Version => new Version(1, 0, 0);
    public string Author => "Keyou";
    public static Config config = new();
    public static string? hostPort;
    public static List<string> onlinePlayer = new();
    private int lastOnlinePlayer;

    public static event Action<int, int>? OnOnlinePlayerChange;

    public void Initialize()
    {
        config = Config.Read();
        hostPort = config.UsingHTTPS ? $"https://+:{config.Port}/" : $"http://+:{config.Port}/";
    }

    public void PostInitialize(EventArgs args)
    {
        PlayerData.CheckRanks();

        WebServer.Start();
        ServerApi.Hooks.NetGreetPlayer.Register(Loader.GetPlugin(), PlayerJoin);
        ServerApi.Hooks.ServerLeave.Register(Loader.GetPlugin(), PlayerLeave);

        #region OnlinePlayer
        OnOnlinePlayerChange += (op, lop) =>
        {
            if (op.GetType() != typeof(int) || lop.GetType() != typeof(int)) return;
            if (op != lop && op != -1)
            {
                WebServer.SendWsData("online-player", op);
                WebServer.SendWsData(data: PlayerData.GetData());
                lastOnlinePlayer = op;
            }
        };
        #endregion


        #region PlayerData
        var allCache = Database.CacheManager.GetAllCacheKeys();
        var enumerable = allCache.ToList();
        if (enumerable.Contains("Balance"))
        {
            var balCache = Database.CacheManager.Cache.GetCache<long>("Balance");
            balCache.Events.OnUpdate += (_, _) => WebServer.SendWsData(data: PlayerData.GetData());
            balCache.Events.OnAdd += (_, _) => WebServer.SendWsData(data: PlayerData.GetData());
            balCache.Events.OnDelete += (_) => WebServer.SendWsData(data: PlayerData.GetData());
        }
        #endregion

        #region PlayerRank
        if (enumerable.Contains("Ranks"))
        {
            var rankCache = Database.CacheManager.Cache.GetCache<dynamic>("Ranks");
            rankCache.Events.OnUpdate += (_, _) => WebServer.SendWsData(data: PlayerData.GetData());
            rankCache.Events.OnAdd += (_, _) => WebServer.SendWsData(data: PlayerData.GetData());
            rankCache.Events.OnDelete += (_) => WebServer.SendWsData(data: PlayerData.GetData());
        }
        #endregion

        #region Auction
        if (enumerable.Contains("Auctions"))
        {
            var auctionCache = Database.CacheManager.Cache.GetCache<dynamic>("Auctions");
            auctionCache.Events.OnAdd += (_, _) => WebServer.SendWsData(data: PlayerData.GetAuctionData());
            auctionCache.Events.OnUpdate += (_, _) => WebServer.SendWsData(data: PlayerData.GetAuctionData());
            auctionCache.Events.OnDelete += (_) => WebServer.SendWsData(data: PlayerData.GetAuctionData());
        }
        #endregion
    }

    public void Reload(ReloadEventArgs args)
    {
        PlayerData.CheckRanks();
        config = Config.Read();
        
        try
        {
            WebServer.Stop(true);
            WebServer.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void Dispose()
    {
        WebServer.Stop();
    }

    private void PlayerJoin(GreetPlayerEventArgs args)
    {
        onlinePlayer.Add(TShock.Players[args.Who].Name);
        OnOnlinePlayerChange?.Invoke(onlinePlayer.Count, lastOnlinePlayer);
    }

    private void PlayerLeave(LeaveEventArgs args)
    {
        onlinePlayer.Remove(TShock.Players[args.Who].Name);
        OnOnlinePlayerChange?.Invoke(onlinePlayer.Count, lastOnlinePlayer);
    }
}