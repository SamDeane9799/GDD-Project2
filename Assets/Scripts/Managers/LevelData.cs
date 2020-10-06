using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelDataScriptableObject", order = 1)]
public class LevelData : ScriptableObject
{
    public int enemiesToSpawn;
    public Vector2[] enemySpawnLocations;

    public Vector2 playerSpawnLocation;
    public Vector2 winTileSpawnLocation;
}
