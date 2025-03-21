using TShockAPI;

namespace Skynomi.AuctionSystem
{
    public abstract class Database
    {
        public static Skynomi.Database.Database? db;
        private static readonly string _databaseType = Skynomi.Database.Database._databaseType;

        public class Auction
        {
            public string? Username { get; set; }
            public int ItemId { get; set; }
            public int Price { get; set; }
            public int Amount { get; set; }
        }

        public static void Initialize()
        {
            db = new Skynomi.Database.Database();

            db.CustomVoid(_databaseType == "mysql"
                ? @"
                    CREATE TABLE IF NOT EXISTS Auctions (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        UUID VARCHAR(255) NOT NULL UNIQUE,
                        Username VARCHAR(255) NOT NULL,
                        ItemId INT NOT NULL,
                        Price INT NOT NULL,
                        Amount INT NOT NULL
                    )
                "
                : @"
                    CREATE TABLE IF NOT EXISTS Auctions (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UUID VARCHAR(255) NOT NULL UNIQUE,
                        Username VARCHAR(255) NOT NULL,
                        ItemId INTEGER NOT NULL,
                        Price INTEGER NOT NULL,
                        Amount INTEGER NOT NULL
                    )
                ");

            var auctionCache = Skynomi.Database.CacheManager.Cache.GetCache<Auction>("Auctions");
            auctionCache.MysqlQuery =
                "SELECT UUID AS 'Key', JSON_OBJECT('Username', Username, 'ItemId', ItemId, 'Price', Price, 'Amount', Amount) AS 'Value' FROM Auctions";
            auctionCache.SqliteQuery = auctionCache.MysqlQuery;
            auctionCache.SaveMysqlQuery =
                "INSERT INTO Auctions (UUID, Username, ItemId, Price, Amount) VALUES (@key, @value_Username, @value_ItemId, @value_Price, @value_Amount)";
            auctionCache.SaveSqliteQuery = auctionCache.SaveMysqlQuery;

            auctionCache.Init();
        }

        public static bool AddAuction(string username, int itemId, int price, int amount)
        {
            try
            {
                Skynomi.Database.CacheManager.Cache.GetCache<Auction>("Auctions").Update(username + "_" + itemId,
                    new Auction { Username = username, ItemId = itemId, Price = price, Amount = amount });
                return true;
            }
            catch (Exception ex)
            {
                Utils.Log.Error(ex.ToString());
                return false;
            }
        }

        public static bool RemoveAuction(string username, int itemId)
        {
            try
            {
                Skynomi.Database.CacheManager.Cache.GetCache<Auction>("Auctions").DeleteValue(username + "_" + itemId);
                return true;
            }
            catch (Exception ex)
            {
                Utils.Log.Error(ex.ToString());
                return false;
            }
        }

        public static bool UpdateAuction(string playername, int itemId, int amount)
        {
            try
            {
                if (amount <= 0)
                    return RemoveAuction(playername, itemId);

                Skynomi.Database.CacheManager.Cache.GetCache<Auction>("Auctions").Modify(playername + "_" + itemId, e =>
                {
                    e.Amount = amount;
                    return e;
                });
                return true;
            }
            catch (Exception ex)
            {
                Utils.Log.Error(ex.ToString());
                return false;
            }
        }

        public static List<object> listAuction(string playername, bool username = false)
        {
            var auctions = Skynomi.Database.CacheManager.Cache.GetCache<Auction>("Auctions").GetAllValues();

            return username
                ? auctions.Where(e => e.Username == playername && e.Amount > 0)
                    .Select(e => new { e.ItemId, e.Price, e.Amount }).Cast<object>().ToList()
                : auctions.Where(e => e.Amount > 0).Select(e => new { e.Username, e.ItemId, e.Price, e.Amount })
                    .Cast<object>().ToList();
        }
    }
}