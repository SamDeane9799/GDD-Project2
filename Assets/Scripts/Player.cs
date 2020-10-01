using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LevelObject
{
    // Start is called before the first frame update
    public Tile currentTile;
    public float movementRange;

    public bool moving;


    void Start()
    {
        DontDestroyOnLoad(this);
        movementRange = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
            MoveToTile();
    }

    public void MoveToTile()
    {
        if (Mathf.Abs(Vector2.Distance(transform.position, currentTile.gameObject.transform.position)) <= .1f)
        {
            moving = false;
        }
        transform.position = Vector2.Lerp(transform.position, currentTile.gameObject.transform.position, .05f);


        Vector3 dir = currentTile.gameObject.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

}
