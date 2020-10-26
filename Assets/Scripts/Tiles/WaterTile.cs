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
        walkable = false;
    }

    public void FreezeTile()
    {
        GetComponent<SpriteRenderer>().sprite = frozenSprite;
        walkable = true;
    }
}
