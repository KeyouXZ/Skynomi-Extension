using Newtonsoft.Json;
using TShockAPI;

namespace Skynomi.Web
{
    public abstract class PlayerData
    {
        public class Data
        {
            public class TRank {
                public int Rank { get; set; }
                public string Name { get; set; } = "Unknown";
            }
            public string Username { get; set; } = "Unknown";
            public string Device { get; set; } = "Unknown";
            public long Balance { get; set; }
            public TRank? Rank { get; set; }
        }

        private static List<string> rankAvailable = new();

        public static void CheckRanks()
        {
            string directoryPath = Path.Combine(TShock.SavePath, "Skynomi");
            string ranksPath = Path.Combine(directoryPath, "Rank.json");

            if (!File.Exists(ranksPath))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(ranksPath);
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (jsonData == null || !jsonData.TryGetValue("Ranks", out var value)) return;
                var ranks = JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString() ?? string.Empty);
                rankAvailable = ranks?.Keys.ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                Utils.Log.Error($"Failed to read Rank.json: {ex}");
            }
        }

        public static object GetData()
        {
            var balCache = Database.CacheManager.Cache.GetCache<long>("Balance");

            dynamic? rankCache = null;
            bool isRankAvailable = Database.CacheManager.GetAllCacheKeys().Contains("Ranks");
            if (isRankAvailable)
            {
                rankCache = Database.CacheManager.Cache.GetCache<dynamic>("Ranks");
            }

            var data = new List<Data>();

            foreach (var player in Web.onlinePlayer)
            {
                long balance = balCache.GetValue(player);
                var rank = isRankAvailable ? rankCache?.GetValue(player).Rank : -1;
                string rankName = isRankAvailable ? (rankAvailable.Count() == 1 ? "N/A" : (rank <= -1 || rank == 0 ? "N/A" : rankAvailable[(int)rank! - 1])) : "N/A";

                var players = TSPlayer.FindByNameOrID(player);
#pragma warning disable CS8604 // Possible null reference argument.
                data.Add(new Data
                {
                    Username = player,
                    Device = Utils.Util.GetPlatform(players.FirstOrDefault(p => p.Name.Equals(player, StringComparison.OrdinalIgnoreCase)) ?? players.FirstOrDefault()),
                    Balance = balance,
                    Rank = new Data.TRank { Rank = rank, Name = rankName }
                });
#pragma warning restore CS8604 // Possible null reference argument.
            }

            var fullData = new
            {
                eventType = "player-data",
                data
            };

            return fullData;
        }

        public static object GetAuctionData()
        {
            dynamic[] auctions = Database.CacheManager.Cache.GetCache<dynamic>("Auctions").GetAllValues().Select(e => new { e.Username, e.ItemId, ItemName = TShock.Utils.GetItemById((int)e.ItemId)?.Name ?? "Unknown", e.Price, e.Amount }).Cast<object>().ToArray();

            var fullData = new
            {
                eventType = "auction-data",
                data = auctions
            };

            return fullData;
        }
    }
}
