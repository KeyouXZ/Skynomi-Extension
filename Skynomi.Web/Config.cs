using Newtonsoft.Json;
using TShockAPI;

namespace Skynomi.Web
{
    public class Config
    {
        [JsonProperty("Port")]
        public int Port { get; set; } = 8080;
        [JsonProperty("Enable Reverse Proxy")]
        public bool EnableReverseProxy { get; set; }
        [JsonProperty("Using HTTPS")]
        public bool UsingHTTPS { get; set; }
        [JsonProperty("Debug Logs")]
        public bool DebugLogs { get; set; }

        public static Config Read()
        {
            string directoryPath = Path.Combine(TShock.SavePath, "Skynomi");
            string configPath = Path.Combine(directoryPath, "WebServer.json");
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