# Documentation for Skynomi.Web Extension

This document provides information on how to use the **Skynomi.Web** extension. Follow these instructions to configure and leverage this extension, which integrates with the **Skynomi** plugin for TShock, providing real-time web server and data-sharing capabilities.

---

## Overview

The Skynomi.Web extension enhances the **Skynomi** plugin for TShock by including a built-in web server. This extension enables real-time interaction with server data, such as:

- Player data (balances, ranks)
- Auction data
- Online players' details and counts

---

## Installation

### Prerequisites
1. **TShock Terraria Server**
   - Ensure TShock is installed and running.
   - Install the **Skynomi** plugin, which is required for this extension.
   
2. **Skynomi**
   - Skynomi must be installed as a dependency to provide the base functionality required by this extension.

### Adding the Extension
1. Place the `Skynomi.Web.dll` file within the `/ServerPlugins` folder of your TShock installation.
2. Restart the Terraria server.

---

## Configuration

Skynomi.Web reads its configuration from its configuration file. These settings control how the built-in web server operates, including details such as ports, HTTPS functionality, and reverse proxy options.

### Key Configuration Fields
- **Port:** The port number the web server should use.
- **UsingHTTPS:** Boolean indicating whether HTTPS should be enabled for secure communication.
- **EnableReverseProxy:** Boolean indicating support for reverse proxies.
- **Debug Logs:** Boolean indicating debug web server logs

### Example Configuration
1. Locate or create the configuration file (e.g., `./ServerPlugins/Skynomi.Web/config.json`).
2. Customize the file based on your needs. Example configuration:

```json
{
  "Port": 8080,
  "EnableReverseProxy": false,
  "UsingHTTPS": false,
  "Debug Logs": false
}
```

### Applying Changes
- After editing the configuration file, use the TShock command `/reload` to refresh the plugin.

---

## Initialization

Upon TShock server startup, Skynomi.Web automatically performs the following tasks:
- Reads the configuration file.
- Sets up the web server to handle incoming requests.
- Registers hooks to monitor player events (like joins and leaves).
- Integrates with **Skynomi's caching system** for data synchronization.

### Key Events and Actions
- **OnOnlinePlayerChange:** 
  - Triggered when the number of online players changes, automatically broadcasting updates to the web server and connected clients.
- **Cache Updates:** 
  - Listens for changes in Skynomi data (e.g., player balance, ranks, and auctions) and synchronizes them dynamically.

---

## Features and Functionality

### 1. **Web Server Integration**
Skynomi.Web sets up an HTTP or HTTPS server to expose:
- **Player Information:** Details such as balances and ranks.
- **Online Players:** Count and current status of players.
- **Auctions:** Real-time auction data for active game items.

### 2. **Real-Time WebSocket Updates**
WebSocket-based communication delivers live updates to connected clients:
- **Online Players:**
  - Updates WebSocket clients with changes in the online player list or count.
- **Player Data:**
  - Updates clients dynamically if a player's rank or balance changes.
- **Auction Data:**
  - Synchronizes new, updated, or deleted auction listings.

### 3. **Hooks into Skynomi**
The extension leverages **TShock** and **Skynomi** hooks:
- **PlayerJoin:**
  - Triggered upon a player joining the game, adding the player's data to the system and notifying clients.
- **PlayerLeave:**
  - Triggered when a player leaves the server, removing their data and broadcasting the update.

### 4. **Integration with Skynomi's Cache Manager**
Skynomi.Web uses **CacheManager** for efficient data management and synchronization:
- It monitors and synchronizes:
  - Player balances and ranks.
  - Auction data.
  - Any additional cached resources within the Skynomi plugin.

---

## Troubleshooting

1. **Web server fails to start**
   - Ensure the `Port` specified in the configuration is available.

2. **Player hooks don’t trigger updates**
   - Verify that the Skynomi plugin is correctly loaded by TShock.
   - Review the server logs for errors or warnings related to the extension.

3. **Configuration changes are not applied**
   - Use `/reload` within TShock to refresh the extension.

---

## Notes

- Skynomi.Web requires the Skynomi plugin to function; ensure both are correctly installed and configured.
- Restart the server after any significant configuration changes, such as port settings or enabling HTTPS.
- WebSocket-based real-time updates are dependent on proper server WebSocket support.

Enjoy using the **Skynomi.Web** extension to enhance Skynomi with a powerful web server and real-time player data interactions!