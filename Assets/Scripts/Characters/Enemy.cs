using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    private const float visionLength = 1.5f;
    private const int visionConeDegrees = 70;
    private bool foundEnemy;

    private Vector3 rayDir;
    void Start()
    {
        foundEnemy = false;
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
        //Checking if we can continue to move in our direction that we are already in
        Vector2 nextPosition = new Vector2(currentTile.X + Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), currentTile.Y + Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
        if((nextPosition.x != -1 && nextPosition.x != GameManager.GRID_WIDTH) && (nextPosition.y != -1 && nextPosition.y != GameManager.GRID_HEIGHT) && 
            GameManager.obstaclePositions[(int)nextPosition.x, (int)nextPosition.y] == null && GameManager.tileBoard[(int)nextPosition.x, (int)nextPosition.y].walkable)
        {
            //If we can then we move the enemy
            currentTile = GameManager.tileBoard[(int)nextPosition.x, (int)nextPosition.y];
            actionPoints -= 1;
        }
        //If we cant then we want to turn around and move in that direction
        else
        {
            transform.Rotate(new Vector3(0, 0, 1), 180);
            nextPosition = new Vector2(currentTile.X + Mathf.Round(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad)), currentTile.Y + Mathf.Round(Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad)));
            
            if ((nextPosition.x != -1 && nextPosition.x != GameManager.GRID_WIDTH) && (nextPosition.y != -1 && nextPosition.y != GameManager.GRID_HEIGHT) &&
            GameManager.obstaclePositions[(int)nextPosition.x, (int)nextPosition.y] == null && GameManager.tileBoard[(int)nextPosition.x, (int)nextPosition.y].walkable)
            {
                //Setting our position to the new tile in the opposite direction
                currentTile = GameManager.tileBoard[(int)nextPosition.x, (int)nextPosition.y];
                actionPoints -= 1;
            }
        }
    }

    //This is our method that is used to detect players
    public void DetectPlayer(int numOfRays)
    {
        //Setting up the layer masks needed to check for in our rays
        int characterMask = 1 << 8;
        int terrainMask = 1 << 9;
        int layerMask = terrainMask | characterMask;

        Player potentialPlayer = null;
        //Here is where we actually project our rays
        //They will be done evenly amongst a 70 degree cone
        for (int i = 0; i < numOfRays; i++)
        {
            //Calculating the quaternion that our ray will be rotated
            //Quaternion rayRot = Quaternion.Euler(0, 0, (((float)i / numOfRays) * visionConeDegrees - (visionConeDegrees/2)) + transform.rotation.eulerAngles.z);
            //Rotating the direction of our ray
            //rayDir = rayRot * Vector3.right;
            //Casting the ray
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, visionLength, layerMask);
            //Checking if we hit something
            if (hit)
            {
                //Checking if we hit a player
                if (hit.collider.TryGetComponent<Player>(out potentialPlayer))
                {
                    //Declaring that the player has lost and changing the gamestate
                    GameManager.currentGameState = GameState.LOSE;
                    FoundPlayer(hit.transform.position);
                }
            }
        }
    }

    //Telling the enemy what to do when it finds the player
    private void FoundPlayer(Vector3 playerPos)
    {
        //We find the direction of the player and then walk towards it and look at it
        Vector3 dir = playerPos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
