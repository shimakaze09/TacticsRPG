using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // TODO: Add all of the game data that needed:
    /*
     * Player progress: This includes the player's current level, experience points, and any other attributes that determine the player's strength and abilities.
     * Inventory: This includes items that the player has collected, such as weapons, armor, and consumable items.
     * Party: This includes the characters that the player has recruited, their levels, equipment, and abilities.
     * Story progress: This includes the player's progress through the main story and any side quests or missions.
     * Game settings: This includes the player's preferred settings for things like the game's resolution, audio volume, and difficulty level.
     * Map information: This includes information about the current state of the world map, like completed missions, unlocked regions and the locations of the player's party.
     */

    public float playTime;
    public SerializableDictionary<string, int> unitLevel;

    // The values defined in this constructor will be the default values
    // the game starts with when there's no data to load
    public GameData()
    {
        playTime = 0;
        unitLevel = new SerializableDictionary<string, int>();
    }
}