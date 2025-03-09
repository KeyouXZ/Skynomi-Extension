using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Cms;
using TShockAPI;

namespace Skynomi.PlaytimeReward
{
    public class Database
    {
        public static Skynomi.Database.Database db;
        private static string _databaseType = Skynomi.Database.Database._databaseType;

        public static async Task Initialize()
        {
            db = new Skynomi.Database.Database();

            if (_databaseType == "mysql")
            {
                await db.CustomVoidAsync(@"
                    CREATE TABLE IF NOT EXISTS PRewards (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        Username VARCHAR(255) UNIQUE NOT NULL,
                        Time INT NOT NULL
                    )
                ");
            }
            else if (_databaseType == "sqlite")
            {
                await db.CustomVoidAsync(@"
                    CREATE TABLE IF NOT EXISTS PRewards (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT UNIQUE NOT NULL,
                        Time INTEGER NOT NULL
                    )
                ");
            }
        }

        public static async Task CreatePlayer(string username)
        {
            if (_databaseType == "mysql")
            {
                await db.CustomVoidAsync("INSERT IGNORE INTO PRewards (Username, Time) VALUES (@username, 0)", new { username });
            }
            else if (_databaseType == "sqlite")
            {
                await db.CustomVoidAsync("INSERT OR IGNORE INTO PRewards (Username, Time) VALUES (@username, 0)", new { username });
            }
        }

        public static async Task<int> GetPlaytime(string username)
        {
            dynamic time = new object[] { };
            if (_databaseType == "mysql")
            {
                time = await db.CustomVoidAsync("SELECT Time FROM PRewards WHERE Username = @username", new { username }, true);
            }
            else if (_databaseType == "sqlite")
            {
                time = await db.CustomVoidAsync("SELECT Time FROM PRewards WHERE Username = @username", new { username }, true);
            }

            if (time.Count == 0)
            {
                return 0;
            }

            return (int)time[0]["Time"] | 0;
        }

        public static async Task SaveData(string username, int time)
        {
            if (_databaseType == "mysql")
            {
                await db.CustomVoidAsync("UPDATE PRewards SET Time = @time WHERE Username = @username", new { time, username });
            }
            else if (_databaseType == "sqlite")
            {
                await db.CustomVoidAsync("UPDATE PRewards SET Time = @time WHERE Username = @username", new { time, username });
            }
        }
    }
}