# Skynomi.Auction

## Description

Skynomi.Auction is an extension for Skynomi that provides an auction feature within the game. This extension allows players to create and manage item auctions in the game.

## Features

- **Auction Listing**: Players can list all available auctions or filter auctions by player name.
- **Auction Creation**: Players can create new auctions by specifying the item Id or item name, price, and amount.
- **Auction Purchase**: Players can buy items from auctions by specifying the seller's name, item Id or item name, and amount.
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
- **Usage**: `/auctionadd <itemid/name> <price> [amount]`
- **Example**: `/auctionadd 123 1000 5` or `/auctionadd Zenith 500 1`
- **Permission**: `auction.add`

### Auction Buy

- **Command**: `/auctionbuy` or `/acbuy`
- **Description**: Buys an item from an auction.
- **Usage**: `/auctionbuy <playername> <itemid/name> [amount]`
- **Example**: `/auctionbuy JohnDoe 123 2` or `/auctionbuy JohnDoe Zenith 5`
- **Permission**: `auction.buy`

### Auction Remove

- **Command**: `/auctiondel` or `/acdel`
- **Description**: Removes an auction.
- **Usage**: `/auctiondel <itemid/name>`
- **Example**: `/auctiondel 123` or `/auctiondel Zenith`
- **Permission**: `auction.remove`

### Admin Auction Remove

- **Command**: `//auctiondel` or `//acdel`
- **Description**: Admin command to remove an auction from any player.
- **Usage**: `//auctiondel <playername> <itemid/name>`
- **Example**: `//auctiondel JohnDoe 123` or `//auctiondel JohnDoe Zenith`
- **Permission**: `auction.admin.remove`
