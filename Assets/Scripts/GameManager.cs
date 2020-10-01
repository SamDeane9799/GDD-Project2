using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public enum GameState
{
    NONE,
    PAUSED,
    PLAYERTURN,
    ENEMYTURN
}
public class GameManager : MonoBehaviour
{
    public const int GRID_WIDTH = 20;
    public const int GRID_HEIGHT = 12;

    #region Properties
    //Get and set properties for the volume levels to make it easier to read and code
    public static float soundFXVolume
    {
        get { return soundSettings[1]; }
        set { soundSettings[1] = value; }
    }
    public static float musicVolume
    {
        get { return soundSettings[2]; }
        set { soundSettings[2] = value; }
    }
    public static float masterVolume
    {
        get { return soundSettings[0]; }
        set { soundSettings[0] = value; }
    }
    #endregion

    #region Sound Variables
    public static List<AudioSource> musicSources;
    public static List<AudioSource> soundFXSources;

    private static List<float> soundSettings;
    #endregion

    #region Player
    //Player prefab
    public PlayerCamera playerCameraPrefab;
    private PlayerCamera playerCam;
    public Player playerPrefab;
    private Player player;
    private List<Tile> availableTiles;
    public static GameState currentGameState;
    #endregion

    #region File IO
    private StreamReader reader;
    private const string settingsPath = "Assets/txt/settings.txt";
    #endregion

    public static Tile[,] tileBoard = new Tile[GRID_WIDTH, GRID_HEIGHT];
    public static Obstacle[,] obstaclePositions = new Obstacle[GRID_WIDTH, GRID_HEIGHT];

    // Start is called before the first frame update
    void Start()
    {
        //Sets the game state and tell the game to not destroy this
        DontDestroyOnLoad(this);
        //SceneManager.sceneLoaded += OnLoad;
        OnLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        availableTiles = new List<Tile>();
        OnPlayersTurn();
        currentGameState = GameState.NONE;

        musicSources = new List<AudioSource>();
        soundFXSources = new List<AudioSource>();

        //Reading in our settings from a text file
        //Here is one example using volume
        soundSettings = new List<float>(3);
        reader = new StreamReader(settingsPath);

        soundSettings.Add(float.Parse(reader.ReadLine()));
        soundSettings.Add(float.Parse(reader.ReadLine()));
        soundSettings.Add(float.Parse(reader.ReadLine()));

        reader.Close();
        //Initializing player
        InitializePlayerCam();
    }

    // Update is called once per frame
    void Update()
    {
        //Code to check the positions of the tiles
        /*for(int i = 0; i < tileBoard.GetLength(0); i++)
        {
            for (int j = 0; j < tileBoard.GetLength(1); j++)
            {
                if(tileBoard[i, j] != null)
                    Debug.Log(tileBoard[i, j].name + " X: " + i + " Y: " + j);
            }
        }

        for(int i = 0; i < obstaclePositions.GetLength(0); i++)
        {
            for (int j = 0; j < obstaclePositions.GetLength(1); j++)
            {
                if(obstaclePositions[i, j] != null)
                    Debug.Log(obstaclePositions[i, j].name + " X: " + i + " Y: " + j);
            }
        }*/

        if(SceneManager.GetActiveScene().name == "SamTestScene")
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = playerCam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
                RaycastHit2D projection = Physics2D.Raycast(new Vector2(0, 0), mousePosition.normalized);
                if (projection.collider != null)
                {
                    //Debug.Log("Clicked X: " + projection.collider.GetComponent<Tile>().X + " Y: " + projection.collider.GetComponent<Tile>().Y);
                    player.currentTile = projection.collider.GetComponent<Tile>();
                    player.moving = true;
                }
            }

            if(Input.GetKeyDown(KeyCode.F))
            {
                player.currentTile = tileBoard[0, GRID_HEIGHT - 1];
                player.moving = true;
            }
        }
    }


    //Sets up our player in the game
    //For testing purposes I am setting it up in the title screen so I can test audio
    public void InitializePlayerCam()
    {
        playerCam = Instantiate<PlayerCamera>(playerCameraPrefab);
    }

    public void OnLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SamTestScene")
        {
            player = Instantiate(playerPrefab);
            player.currentTile = tileBoard[0, 0];
            player.moving = true;
        }
    }

    public void OnPlayersTurn()
    {
        GameManager.currentGameState = GameState.PLAYERTURN;

        List<Tile> openList = new List<Tile>();

        openList.Add(player.currentTile);
        Tile currentTile = openList[0];


        while(openList.Count > 0)
        {
            if (currentTile.X != GRID_WIDTH - 1 && tileBoard[currentTile.X + 1, currentTile.Y] != null && obstaclePositions[currentTile.X + 1, currentTile.Y] == null &&
            Mathf.Abs(Vector2.Distance(new Vector2(tileBoard[currentTile.X + 1, currentTile.Y].X + 1, tileBoard[currentTile.X + 1, currentTile.Y].Y), new Vector2(player.X, player.Y))) >= player.movementRange
            && !availableTiles.Contains(tileBoard[currentTile.X + 1, currentTile.Y]))
            {
                openList.Add(tileBoard[currentTile.X + 1, currentTile.Y]);
                availableTiles.Add(tileBoard[currentTile.X + 1, currentTile.Y]);
            }
            if (currentTile.X != 0 && tileBoard[currentTile.X - 1, currentTile.Y] != null && obstaclePositions[currentTile.X - 1, currentTile.Y] &&
            Mathf.Abs(Vector2.Distance(new Vector2(tileBoard[currentTile.X - 1, currentTile.Y].X - 1, tileBoard[currentTile.X - 1, currentTile.Y].Y), new Vector2(player.X, player.Y))) >= player.movementRange
            && !availableTiles.Contains(tileBoard[currentTile.X - 1, currentTile.Y]))
            {
                openList.Add(tileBoard[currentTile.X - 1, currentTile.Y]);
                availableTiles.Add(tileBoard[currentTile.X - 1, currentTile.Y]);
            }
            if (currentTile.Y != GRID_HEIGHT - 1 && tileBoard[currentTile.X, currentTile.Y + 1] != null && obstaclePositions[currentTile.X, currentTile.Y + 1] &&
            Mathf.Abs(Vector2.Distance(new Vector2(tileBoard[currentTile.X, currentTile.Y + 1].X - 1, tileBoard[currentTile.X, currentTile.Y + 1].Y), new Vector2(player.X, player.Y))) >= player.movementRange
            && !availableTiles.Contains(tileBoard[currentTile.X, currentTile.Y + 1]))
            {
                openList.Add(tileBoard[currentTile.X, currentTile.Y + 1]);
                availableTiles.Add(tileBoard[currentTile.X, currentTile.Y + 1]);
            }
            if (currentTile.Y != 0 && tileBoard[currentTile.X, currentTile.Y - 1] != null && obstaclePositions[currentTile.X, currentTile.Y - 1] &&
            Mathf.Abs(Vector2.Distance(new Vector2(tileBoard[currentTile.X, currentTile.Y - 1].X - 1, tileBoard[currentTile.X, currentTile.Y - 1].Y), new Vector2(player.X, player.Y))) >= player.movementRange
            && !availableTiles.Contains(tileBoard[currentTile.X, currentTile.Y - 1]))
            {
                openList.Add(tileBoard[currentTile.X, currentTile.Y - 1]);
                availableTiles.Add(tileBoard[currentTile.X, currentTile.Y - 1]);
            }


            openList.RemoveAt(0);
            if (openList.Count > 0)
            {
                currentTile = openList[0];
            }
        }

        foreach(Tile t in availableTiles)
        {
            Debug.Log("X: " + t.X + " | " + "Y: " + t.Y);
            t.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }
}
