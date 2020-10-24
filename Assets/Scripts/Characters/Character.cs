using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : LevelObject
{
    public override int X => currentTile.X;

    public override int Y => currentTile.Y;
    //comment
    // Start is called before the first frame update
    public bool moving;
    public Tile currentTile;
    //Our range that we can move at
    public float actionPoints;

    public Sprite facingUp;
    public Sprite facingRight;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (GameManager.currentGameState != GameState.LOSE)
        {
            //If we are not at our currentTile set moving to true so MoveToTile() is called
            if (Mathf.Abs(Vector2.Distance(transform.position, currentTile.gameObject.transform.position)) >= .1f)
                moving = true;
            //If we are moving we call the method to lerp to the current tile
            if (moving)
                MoveToTile();
        }
    }

    public virtual void MoveToTile()
    {
        //Checking if we reached our destination
        if (Mathf.Abs(Vector2.Distance(transform.position, currentTile.gameObject.transform.position)) <= .01f)
        {
            moving = false;
        }
        //Lerping our position
        transform.position = Vector2.Lerp(transform.position, currentTile.gameObject.transform.position, .075f);
        //transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
        //Setting our rotation
        if (this is Player)
        {
            Vector3 dir = currentTile.gameObject.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Debug.Log(angle);
            if (Mathf.Abs(angle - 90) <= 1f)
            {
                GetComponent<SpriteRenderer>().sprite = facingUp;
                Debug.Log("Facingup");
            }
            else
                GetComponent<SpriteRenderer>().sprite = facingRight;
        }
    }
}
