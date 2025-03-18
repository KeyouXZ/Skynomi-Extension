# Skynomi.Auction

## Description

Skynomi.Auction is an extension for Skynomi that provides an auction feature within the game. This extension allows players to create and manage item auctions in the game.

## Features

- **Auction Listing**: Players can list all available auctions or filter auctions by player name.
- **Auction Creation**: Players can create new auctions by specifying the item ID, price, and amount.
- **Auction Purchase**: Players can buy items from auctions by specifying the seller's name, item ID, and amount.
- **Auction Removal**: Players can remove their own auctions, and administrators can remove auctions from any player.

## Commands

### Auction List

- **Command**: `/auctionlist` or `/aclist`
- **Description**: Lists all auctions or auctions by a specific player.
- **Usage**: `/auctionlist [playername]`
- **Example**: `/auctionlist` or `/auctionlist JohnDoe`
- **Permission**: `auction.list`

### Auction Add

- **Command**: `/auctionadd` or `/acadd`
- **Description**: Adds a new auction.
- **Usage**: `/auctionadd <itemid> <price> [amount]`
- **Example**: `/auctionadd 123 1000 5`
- **Permission**: `auction.add`

### Auction Buy

- **Command**: `/auctionbuy` or `/acbuy`
- **Description**: Buys an item from an auction.
- **Usage**: `/auctionbuy <playername> <itemid> [amount]`
- **Example**: `/auctionbuy JohnDoe 123 2`
- **Permission**: `auction.buy`

### Auction Remove

- **Command**: `/auctiondel` or `/acdel`
- **Description**: Removes an auction.
- **Usage**: `/auctiondel <itemid>`
- **Example**: `/auctiondel 123`
- **Permission**: `auction.remove`

### Admin Auction Remove

- **Command**: `//auctiondel` or `//acdel`
- **Description**: Admin command to remove an auction from any player.
- **Usage**: `//auctiondel <playername> <itemid>`
- **Example**: `//auctiondel JohnDoe 123`
- **Permission**: `auction.admin.remove`
