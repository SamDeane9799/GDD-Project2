using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    private const float visionLength = 3.0f;

    private Vector3 rayDir;
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

    public void DetectPlayer(int numOfRays)
    {
        int characterMask = 1 << 8;
        int terrainMask = 1 << 9;
        int layerMask = terrainMask | characterMask;

        Player potentialPlayer = null;

        for (int i = 0; i < numOfRays; i++)
        {
            Quaternion rayRot = Quaternion.Euler(0, 0, (((float)i / numOfRays) * 70 - 35) + transform.rotation.eulerAngles.z);
            rayDir = rayRot * Vector3.right;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir.normalized, visionLength, layerMask);
            if (hit)
            {
                if (hit.collider.TryGetComponent<Player>(out potentialPlayer))
                {
                    GameManager.currentGameState = GameState.LOSE;
                    FoundPlayer(hit.transform.position);
                }
            }
        }
    }

    private void FoundPlayer(Vector3 playerPos)
    {
        Vector3 dir = playerPos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.position = Vector2.Lerp(transform.position, playerPos, .01f);
    }

}
