using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager
{
    // Start is called before the first frame update


    public List<Enemy> Enemies
    {
        get { return enemies; }
    }

    private List<Enemy> enemies = new List<Enemy>();
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
