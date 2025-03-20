using TShockAPI;

namespace Skynomi.PlaytimeReward
{
    public abstract class Commands
    {
        private static readonly Skynomi.Database.Database db = new();
        public static void Initialize()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command(Permissions.Playtime, Playtime, "playtime", "pt")
            {
                AllowServer = false,
                HelpText = "Shows how much playtime you have."
            });
            TShockAPI.Commands.ChatCommands.Add(new Command(Permissions.GetReward, GetReward, "getreward", "gr")
            {
                AllowServer = false,
                HelpText = "Claims your playtime reward."
            });
        }
        private static void GetReward(CommandArgs args)
        {
            var playtimeCache = Skynomi.Database.CacheManager.Cache.GetCache<int>("Playtime");
            int playtime = playtimeCache.GetValue(args.Player.Name);

            if (playtime >= 30)
            {
                playtimeCache.Update(args.Player.Name, playtime - 30);
                int reward = PlaytimeReward.config?.Reward ?? 0;
                db.AddBalance(args.Player.Name, db.GetBalance(args.Player.Name) + reward);
                args.Player.SendSuccessMessage($"You have claimed your playtime reward and received {Utils.Util.CurrencyFormat(reward)}!");
            }
            else
            {
                args.Player.SendErrorMessage("You need at least 30 mins of unused playtime to claim a reward.");
            }
        }

        private static void Playtime(CommandArgs args)
        {
            PlaytimeReward.UpdateTime();
            args.Player.SendInfoMessage($"You have {Skynomi.Database.CacheManager.Cache.GetCache<int>("Playtime").GetValue(args.Player.Name)} mins unused playtime.");
        }
    }
}