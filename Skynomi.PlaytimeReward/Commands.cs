using TShockAPI;

namespace Skynomi.PlaytimeReward
{
    public class Commands
    {
        public static void Initialize()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command(Permissions.Playtime, Playtime, "playtime", "pt")
            {
                AllowServer = false,
                HelpText = "Shows how much playtime you have."
            });
        }

        private static void Playtime(CommandArgs args)
        {
            PlaytimeReward.UpdateTime();
            args.Player.SendInfoMessage($"You have {PlaytimeReward.onlinePlayers[args.Player.Name]} mins unused playtime.");
        }
    }
}