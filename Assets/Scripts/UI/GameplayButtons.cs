using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayButtons : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GetComponent<GameManager>();
    }

    public void MoveObject()
    {
        Debug.Log("Moving Object");
    }

    public void BurnObject()
    {
        Debug.Log("Burning Object");
    }

    public void FreezeObject()
    {
        Debug.Log("Freezeing Object");
    }
}
