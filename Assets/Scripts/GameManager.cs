using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public enum GameState
{
    PAUSED,
    PLAY
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
    public static GameState currentGameState;
    #endregion

    #region File IO
    private StreamReader reader;
    private const string settingsPath = "Assets/txt/settings.txt";
    #endregion

    public static Tile[,] tileBoard = new Tile[GRID_WIDTH, GRID_HEIGHT];
    public static Obstacle[,] obstaclePositions = new Obstacle[GRID_WIDTH, GRID_HEIGHT];

    // Start is called before the first frame update
    void Awake()
    {
        //Sets the game state and tell the game to not destroy this
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnLoad;
        currentGameState = GameState.PLAY;

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
                Debug.Log(mousePosition.x + " | " + mousePosition.y);
                RaycastHit2D projection = Physics2D.Raycast(new Vector2(0, 0), mousePosition.normalized);
                if (projection.collider != null)
                {
                    Debug.Log("Clicked X: " + projection.collider.GetComponent<Tile>().X + " Y: " + projection.collider.GetComponent<Tile>().Y);
                }
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
        }
    }
}
