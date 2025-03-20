using Skynomi.Utils;
using TShockAPI;
using Terraria;
using System.Reflection;

namespace Skynomi.AuctionSystem
{
    public class Auction : Loader.ISkynomiExtension, Loader.ISkynomiExtensionPostInit
    {
        public string Name => "Auction";
        public string Description => "Auction extension for Skynomi.";
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public Version Version => new Version(Assembly.GetExecutingAssembly().GetName().Version.Major, Assembly.GetExecutingAssembly().GetName().Version.Minor, Assembly.GetExecutingAssembly().GetName().Version.Build);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        public string Author => "Keyou";

        public void Initialize()
        {
            if (!Main.ServerSideCharacter)
            {
                return;
            }
            Database.Initialize();
            Commands.Initialize();
        }

        public void PostInitialize(EventArgs args)
        {
            if (Main.ServerSideCharacter) return;
            TShock.Log.ConsoleError($"{Messages.Name} Auction has been disabled because Server Side Character is not enabled.");
        }
    }
}
