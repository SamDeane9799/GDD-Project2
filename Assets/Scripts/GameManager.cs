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
    public Player playerPrefab;
    private Player mainPlayer;
    public static GameState currentGameState;
    #endregion

    #region File IO
    private StreamReader reader;
    private const string settingsPath = "Assets/txt/settings.txt";
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        //Sets the game state and tell the game to not destroy this
        DontDestroyOnLoad(this);
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
        InitializePlayer();
    }

    // Update is called once per frame
    void Update()
    {
    }


    //Sets up our player in the game
    //For testing purposes I am setting it up in the title screen so I can test audio
    public void InitializePlayer()
    {
        mainPlayer = Instantiate<Player>(playerPrefab);
    }


    //Method that should be called everytime we load a new scene
    //Should ideally be called after everything loads
    //Not yet implemented
    /* private void onLoad()
     {

     }*/
}
