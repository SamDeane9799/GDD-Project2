using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTile : Tile
{
    // Start is called before the first frame update
    public Sprite frozenSprite;
    void Start()
    {
        base.Start();
        walkable = true;
    }

    public void FreezeTile()
    {
        GetComponent<SpriteRenderer>().sprite = frozenSprite;
        Obstacle newObst = gameObject.AddComponent<Obstacle>();
        GameManager.obstaclePositions[X, Y] = newObst;
        newObst.gridRef = GameObject.Find("FloorTiles").GetComponent<Grid>();
    }
}
