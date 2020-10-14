using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTile : Tile
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        walkable = false;
    }

    private void Update()
    {
        
    }
}
