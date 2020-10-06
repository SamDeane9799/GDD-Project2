using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelDataScriptableObject", order = 1)]
public class LevelData : ScriptableObject
{
    public Enemy enemyPrefab;
    public int enemiesToSpawn;
    public List<Vector2> enemySpawnLocations;

    public Vector2 playerSpawnLocation;
    public Vector2 winTileSpawnLocation;
}
