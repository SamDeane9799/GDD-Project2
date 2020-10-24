using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeTile : Tile
{
    // Start is called before the first frame update
    void Awake()
    {
        SetPosition();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        SetPosition();
    }


    // Sets the position of the trees so that they don't overlap each other
    private void SetPosition()
    {
        GetComponent<SpriteRenderer>().sortingOrder = (int)-transform.position.y;
    }
}
