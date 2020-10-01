using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LevelObject
{
    // This is our current tile that we should be on
    public Tile currentTile;
    //Our range that we can move at
    public float movementRange;

    //Bool that checks if we're moving
    public bool moving;


    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        //If we are moving we call the method to lerp to the current tile
        if (moving)
            MoveToTile();

        //If we are not at our currentTile set moving to true so MoveToTile() is called
        if (Mathf.Abs(Vector2.Distance(transform.position, currentTile.gameObject.transform.position)) >= .1f)
            moving = true;
       
    }

    //Method that is run until the player reaches their current tile
    public void MoveToTile()
    {
        //Checking if we reached our destination
        if (Mathf.Abs(Vector2.Distance(transform.position, currentTile.gameObject.transform.position)) <= .1f)
        {
            moving = false;
        }
        //Lerping our position
        transform.position = Vector2.Lerp(transform.position, currentTile.gameObject.transform.position, .05f);
        //transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
        //Setting our rotation
        Vector3 dir = currentTile.gameObject.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

}
