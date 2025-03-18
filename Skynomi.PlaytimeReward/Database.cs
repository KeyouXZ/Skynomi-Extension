namespace Skynomi.PlaytimeReward
{
    public class Database
    {
        public static Skynomi.Database.Database? db;
        private static string _databaseType = Skynomi.Database.Database._databaseType;

        public static void Initialize()
        {
            db = new Skynomi.Database.Database();

            if (_databaseType == "mysql")
            {
                db.CustomVoid(@"
                    CREATE TABLE IF NOT EXISTS PRewards (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        Username VARCHAR(255) UNIQUE NOT NULL,
                        Time INT NOT NULL
                    )
                ");
            }
            else if (_databaseType == "sqlite")
            {
                db.CustomVoid(@"
                    CREATE TABLE IF NOT EXISTS PRewards (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT UNIQUE NOT NULL,
                        Time INTEGER NOT NULL
                    )
                ");
            }

            PlaytimeInitialize();
        }

        public static void PlaytimeInitialize()
        {
            var cache = Skynomi.Database.CacheManager.Cache.GetCache<int>("Playtime");
            cache.MysqlQuery = "SELECT Username AS 'Key', Time AS 'Value' FROM PRewards";
            cache.SqliteQuery = cache.MysqlQuery;
            cache.SaveMysqlQuery = "INSERT INTO PRewards (Username, Time) VALUES (@key, @value) ON DUPLICATE KEY UPDATE Time = @value";
            cache.SaveSqliteQuery = "INSERT INTO PRewards (Username, Time) VALUES (@key, @value) ON CONFLICT (Username) DO UPDATE SET Time = @value";
            cache.Init();
        }

        public static void CreatePlayer(string username)
        {
            if (!Skynomi.Database.CacheManager.Cache.GetCache<int>("Playtime").TryGetValue(username, out int _))
            {
                Skynomi.Database.CacheManager.Cache.GetCache<int>("Playtime").Update(username, 0);
            }
        }
    }
}