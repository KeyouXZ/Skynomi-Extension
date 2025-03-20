using Microsoft.Xna.Framework;
using TShockAPI;
using Terraria;

namespace Skynomi.AuctionSystem
{
    public abstract class Commands
    {
        public static void Initialize()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command(Permissions.List, AuctionList, "auctionlist", "aclist")
            {
                AllowServer = true,
                HelpText = "Lists all auctions"
            });
            TShockAPI.Commands.ChatCommands.Add(new Command(Permissions.Add, AuctionAdd, "auctionadd", "acadd")
            {
                AllowServer = false,
                HelpText = "Adds an auction"
            });
            TShockAPI.Commands.ChatCommands.Add(new Command(Permissions.Buy, AuctionBuy, "auctionbuy", "acbuy")
            {
                AllowServer = false,
                HelpText = "Buys an auction"
            });
            TShockAPI.Commands.ChatCommands.Add(new Command(Permissions.Delete, AuctionRemove, "auctiondel", "acdel")
            {
                AllowServer = false,
                HelpText = "Removes an auction"
            });
            TShockAPI.Commands.ChatCommands.Add(new Command(Permissions.AdminDelete, AdminAuctionRemove, "/auctiondel", "/acdel")
            {
                AllowServer = true,
                HelpText = "Admin Removes an auction"
            });
        }

        #region Auction List

        private static void AuctionList(CommandArgs args)
        {
            string target = args.Parameters.Count > 0 ? args.Parameters[0] : "";
            bool anyTarget = target != "";

            var listAuction = Database.listAuction(target, anyTarget);

            if (!listAuction.Any() && !anyTarget)
            {
                args.Player.SendInfoMessage("No auctions found.");
                return;
            }
            else if (!listAuction.Any() && anyTarget)
            {
                args.Player.SendErrorMessage("No auctions found for this player.");
                return;
            }

            string message = anyTarget ? $"{target}'s Auction List:" : "Auction List:";

            int counter = 1;
            if (anyTarget)
            {
                foreach (var aauction in listAuction)
                {
                    var auction = (dynamic)aauction;
                    message += $"\n{counter}. [i/s{auction.Amount}:{auction.ItemId}] ({auction.ItemId}) for {Utils.Util.CurrencyFormat((int)auction.Price)}";
                    counter++;
                }
            }
            else
            {
                Dictionary<string, int> itemCount = new Dictionary<string, int>();
                foreach (var playerName in listAuction.Select(auction => ((dynamic)auction).Username))
                {
                    if (!itemCount.ContainsKey((string)playerName))
                    {
                        itemCount[(string)playerName] = 1;
                    }
                    else
                    {
                        itemCount[(string)playerName] += 1;
                    }
                }

                foreach (var player in itemCount)
                {
                    message += $"\n{counter}. {player.Key} ({player.Value} item{((player.Value > 1) ? "s" : "")})";
                    counter++;
                }
            }

            args.Player.SendInfoMessage(message);
        }
        #endregion

        #region Auction Add

        private static void AuctionAdd(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /auctionadd <itemid/name> <price> [amount]");
                return;
            }

            if (!int.TryParse(args.Parameters[0], out var itemid))
            {
                itemid = TShock.Utils.GetItemByName(args.Parameters[0]).Where(x => x.Name.ToLower() == args.Parameters[0].ToLower()).Select(x => x.netID).FirstOrDefault();
                if (itemid == 0)
                {
                    args.Player.SendErrorMessage("Item not found!");
                    return;
                }
            }

            if (!int.TryParse(args.Parameters[1], out var price))
            {
                args.Player.SendErrorMessage("Price must be a number!");
                return;
            }

            if (price < 1)
            {
                args.Player.SendErrorMessage("Price must be at least 1!");
                return;
            }

            int amount = 1;
            if (args.Parameters.Count > 2)
            {
                if (!int.TryParse(args.Parameters[2], out amount))
                {
                    args.Player.SendErrorMessage("Amount must be a number!");
                    return;
                }
            }

            if (amount < 1)
            {
                args.Player.SendErrorMessage("Amount must be at least 1!");
                return;
            }

            int maxStack = TShock.Utils.GetItemById(itemid).maxStack;
            if (amount > maxStack)
            {
                args.Player.SendErrorMessage($"Amount must be less than or equal to {maxStack}!");
                return;
            }

            var isItemExists = TShock.Utils.GetItemById(itemid);
            if (isItemExists == null)
            {
                args.Player.SendErrorMessage("Item does not exist!");
                return;
            }

            int totalOwned = 0;
            foreach (var item in args.Player.TPlayer.inventory)
            {
                if (item.netID == itemid)
                {
                    totalOwned += item.stack;
                }
            }

            if (totalOwned == 0)
            {
                args.Player.SendErrorMessage("You do not own any of the item!");
                return;
            }

            if (totalOwned < amount)
            {
                args.Player.SendErrorMessage($"You do not have enough of the item!");
                return;
            }

            List<Database.Auction> data = Skynomi.Database.CacheManager.Cache.GetCache<Database.Auction>("Auctions").GetAllValues().Where(e => e.Username == args.Player.Name && e.ItemId == itemid).ToList();
            if (data.Count > 0)
            {
                args.Player.SendErrorMessage("You already have an auction with this item!");
                return;
            }

            if (Database.AddAuction(args.Player.Name, itemid, price, amount))
            {
                int remainingToRemove = amount;
                for (int i = 0; i < args.Player.TPlayer.inventory.Length; i++)
                {
                    if (args.Player.TPlayer.inventory[i].type == itemid)
                    {
                        if (args.Player.TPlayer.inventory[i].stack > remainingToRemove)
                        {
                            args.Player.TPlayer.inventory[i].stack -= remainingToRemove;
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, args.Player.Index, i);
                            break;
                        }
                        else
                        {
                            remainingToRemove -= args.Player.TPlayer.inventory[i].stack;
                            args.Player.TPlayer.inventory[i].netDefaults(0);
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, args.Player.Index, i);
                        }
                    }
                }

                string detail = $"[c/00FF00:== Auction Add Succesfull ==]\n" +
                                $"[c/0000FF:Item:] {TShock.Utils.GetItemById(itemid).Name} ([i/s{amount}:{itemid}])\n" +
                                $"[c/0000FF:Price:] {Utils.Util.CurrencyFormat(price)}\n" +
                                $"[c/0000FF:Amount:] {amount}";
                args.Player.SendMessage(detail, Color.White);
            }
            else
            {
                args.Player.SendErrorMessage("Failed to add auction!");
            }
        }
        #endregion

        #region Auction Buy

        private static void AuctionBuy(CommandArgs args)
        {
            try
            {
                if (args.Parameters.Count < 2)
                {
                    args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /auctionbuy <playername> <itemid/name> [amount]");
                    return;
                }

                string playername = args.Parameters[0];
                if (!int.TryParse(args.Parameters[1], out var itemid))
                {
                    itemid = TShock.Utils.GetItemByName(args.Parameters[1]).Where(x => x.Name.ToLower() == args.Parameters[1].ToLower()).Select(x => x.netID).FirstOrDefault();
                    if (itemid == 0)
                    {
                        args.Player.SendErrorMessage("Item not found!");
                        return;
                    }
                }

                int amount = 1;
                if (args.Parameters.Count > 2)
                {
                    if (!int.TryParse(args.Parameters[2], out amount))
                    {
                        args.Player.SendErrorMessage("Amount must be a number!");
                        return;
                    }
                }

                if (amount < 1)
                {
                    args.Player.SendErrorMessage("Amount must be at least 1!");
                    return;
                }

                var data = Database.listAuction(args.Player.Name);
                if (data.Count == 0)
                {
                    args.Player.SendErrorMessage("Auction not found!");
                    return;
                }

                int price = (int)((dynamic)data)[0].Price;
                int amountInAuction = (int)((dynamic)data)[0].Amount;

                if (amount > amountInAuction)
                {
                    args.Player.SendErrorMessage("Not enough items in the auction to fulfill the request!");
                    return;
                }

                int totalPrice = amount * price;
                if (Database.db.GetBalance(args.Player.Name) < totalPrice)
                {
                    args.Player.SendErrorMessage("You don't have enough money to buy this item! You need " + Utils.Util.CurrencyFormat(totalPrice));
                    return;
                }

                if (Database.UpdateAuction(playername, itemid, amountInAuction - amount))
                {
                    Database.db.AddBalance(playername, totalPrice);
                    Database.db.RemoveBalance(args.Player.Name, totalPrice);

                    args.Player.GiveItem(itemid, amount);

                    string detail = $"[c/00FF00:== Auction Buy Succesfull ==]\n" +
                                    $"[c/0000FF:From:] {playername}\n" +
                                    $"[c/0000FF:Item:] {TShock.Utils.GetItemById(itemid).Name} [i/s{amount}:{itemid}]\n" +
                                    $"[c/0000FF:Price:] {Utils.Util.CurrencyFormat(totalPrice)}";
                    args.Player.SendMessage(detail, Color.White);
                }
                else
                {
                    args.Player.SendErrorMessage("Failed to buy item!");
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
            }
        }
        #endregion

        #region Auction Remove

        private static void AuctionRemove(CommandArgs args)
        {
            try
            {
                if (args.Parameters.Count < 1)
                {
                    args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /auctiondel <itemid/name>");
                    return;
                }

                if (!int.TryParse(args.Parameters[0], out var itemid))
                {
                    itemid = TShock.Utils.GetItemByName(args.Parameters[0]).Where(x => x.Name.ToLower() == args.Parameters[0].ToLower()).Select(x => x.netID).FirstOrDefault();
                    if (itemid == 0)
                    {
                        args.Player.SendErrorMessage("Item not found!");
                        return;
                    }
                }

                var auctions = Database.listAuction(args.Player.Name, true);
                bool itemExists = false;
                int amount = 1;
                foreach (var auction in auctions)
                {
                    var auctionDict = (dynamic)auction;
                    if ((int)auctionDict.ItemId == itemid)
                    {
                        itemExists = true;
                        amount = (int)auctionDict.Amount;
                        break;
                    }
                }

                if (!itemExists)
                {
                    args.Player.SendErrorMessage("Item not found in your auction list!");
                    return;
                }

                if (Database.RemoveAuction(args.Player.Name, itemid))
                {
                    args.Player.SendSuccessMessage($"Successfully removed auction for {TShock.Utils.GetItemById(itemid).Name} ([i/s{amount}:{itemid}])");
                    args.Player.GiveItem(itemid, amount);
                }
                else
                {
                    args.Player.SendErrorMessage("Failed to remove auction!");
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
            }
        }
        #endregion

        #region Auction Remove Admin

        private static void AdminAuctionRemove(CommandArgs args)
        {
            try
            {
                if (args.Parameters.Count < 2)
                {
                    args.Player.SendErrorMessage("Invalid syntax! Proper syntax: //auctiondel <playername> <itemid/name>");
                    return;
                }

                string playername = args.Parameters[0];
                if (!int.TryParse(args.Parameters[1], out var itemid))
                {
                    itemid = TShock.Utils.GetItemByName(args.Parameters[1]).Where(x => x.Name.ToLower() == args.Parameters[1].ToLower()).Select(x => x.netID).FirstOrDefault();
                    if (itemid == 0)
                    {
                        args.Player.SendErrorMessage("Item not found!");
                        return;
                    }
                }

                var auctions = Database.listAuction(playername, true);
                bool itemExists = false;
                int amount = 1;
                foreach (var auction in auctions)
                {
                    var auctionDict = (dynamic)auction;
                    if ((int)auctionDict.ItemId == itemid)
                    {
                        itemExists = true;
                        amount = (int)auctionDict.Amount;
                        break;
                    }
                }

                if (!itemExists)
                {
                    args.Player.SendErrorMessage("Item not found in {0}'s auction list!", playername);
                    return;
                }

                if (Database.RemoveAuction(playername, itemid))
                {
                    args.Player.SendSuccessMessage($"Successfully removed auction for {TShock.Utils.GetItemById(itemid).Name} ([i/s{amount}:{itemid}]) from {playername}'s auction list.");
                }
                else
                {
                    args.Player.SendErrorMessage("Failed to remove auction!");
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
            }
        }
        #endregion
    }
}