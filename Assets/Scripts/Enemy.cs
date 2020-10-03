using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();        
    }

    //Here we move our enemy forward depending on which way it's rotated and we rotate it if it's in front of an obstacle
    //We should also be checking for the player being in front of the enemy here
    public void EnemyTurn()
    {
        Vector2 nextPosition = new Vector2(currentTile.X + Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), currentTile.Y + Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
        if((nextPosition.x != -1 && nextPosition.x != GameManager.GRID_WIDTH) && (nextPosition.y != -1 && nextPosition.y != GameManager.GRID_HEIGHT) && 
            GameManager.obstaclePositions[(int)nextPosition.x, (int)nextPosition.y] == null)
        {
            currentTile = GameManager.tileBoard[(int)nextPosition.x, (int)nextPosition.y];
            actionPoints -= 1;
        }
        else
        {
            transform.Rotate(new Vector3(0, 0, 1), 180);
            nextPosition = new Vector2(currentTile.X + Mathf.Round(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad)), currentTile.Y + Mathf.Round(Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad)));
            
            if ((nextPosition.x != -1 && nextPosition.x != GameManager.GRID_WIDTH) && (nextPosition.y != -1 && nextPosition.y != GameManager.GRID_HEIGHT) &&
            GameManager.obstaclePositions[(int)nextPosition.x, (int)nextPosition.y] == null)
            {
                currentTile = GameManager.tileBoard[(int)nextPosition.x, (int)nextPosition.y];
                actionPoints -= 1;
            }
        }
    }
}
