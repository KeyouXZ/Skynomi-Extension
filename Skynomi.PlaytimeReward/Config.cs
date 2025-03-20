using Newtonsoft.Json;
using TShockAPI;

namespace Skynomi.PlaytimeReward
{
    public class Config
    {
        [JsonProperty("Balance Reward For 30 Minutes")]
        public int Reward { get; set; } = 3000;

        public static Config Read()
        {
            string directoryPath = Path.Combine(TShock.SavePath, "Skynomi");
            string configPath = Path.Combine(directoryPath, "PlaytimeReward.json");
            Directory.CreateDirectory(directoryPath);

            try
            {
                var defaultConfig = new Config();
                if (!File.Exists(configPath))
                {
                    File.WriteAllText(configPath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
                }
                var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath)) ?? new Config();

                return config;
            }

            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
                return new Config();
            }
        }
    }
}