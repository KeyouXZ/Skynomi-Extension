# Skynomi.PlaytimeReward

## Getting Started

Skynomi.PlaytimeReward is a extension for Skynomi. This extension rewards players with in-game currency based on their playtime.

## Commands

### Playtime Command

* **Usage**: `/playtime`
* **Description**: Displays the player's current playtime.
* **Permission**: `playtime.playtime`

### Get Reward Command

* **Usage**: `/getreward`
* **Description**: Claims the reward based on the player's accumulated playtime.
* **Permission**: `playtime.getreward`

## Configuration

The plugin uses a configuration file to store settings. The configuration file is located in the `Skynomi/PlaytimeReward.json` directory.

### Configuration Options

* **Balance Reward For 30 Minutes**: The amount of in-game currency to reward players.

### Example Configuration

The following is an example configuration file:

```json
{
    "Balance Reward For 30 Minutes": 30000
}
```
