using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.IO;

public class PlayerCamera : MonoBehaviour
{
    public int Level
    {
        get { return level; }
        set { level = value; }
    }

    [SerializeField]
    private const int MAX_LEVEL = 5;

    #region UI Variables
    [Header("UI Variables")]
    public Canvas titleCanvas;
    public Canvas pauseCanvas;
    public Canvas abilityCanvas;
    public Button moveButton;
    public Button freezeButton;
    public Button burnButton;
    public Button resetButton;
    public Button passButton;
    public Stack<Canvas> canvasTracker = new Stack<Canvas>();

    public Slider musicSoundVolumeSlider;
    public Slider soundfxVolumeSlider;
    public Slider masterSoundVolumeSlider;
    public Button applyButton;

    public List<Button> levelSelectButtons;
    #endregion

    #region File IO
    private const string settingsPath = "Assets/StreamingAssets/txt/settings.txt";
    private StreamWriter writer;
    private StreamReader reader;
    #endregion

    private int level;

    Vector2 position = new Vector2(0, 0);
    public GameState previousGameState;

    void Awake()
    {
        Debug.Log("AWAKE");
        DontDestroyOnLoad(this);
        canvasTracker.Push(titleCanvas);

        if(SceneManager.GetActiveScene().name != "StartScene")
        {
            abilityCanvas.gameObject.SetActive(true);
        }

        //soundfxVolumeSlider.value = GameManager.soundFXVolume;
        //musicSoundVolumeSlider.value = GameManager.musicVolume;
        //masterSoundVolumeSlider.value = GameManager.masterVolume;
        //GameManager.musicSources.Add(GetComponent<AudioSource>());
        applyButton.interactable = false;

        reader = new StreamReader(Application.dataPath + "/StreamingAssets/txt/levels.txt");
        bool loop = true;
        int line = int.Parse(reader.ReadLine());
        level = 0;
        while (line != 0)
        {
            levelSelectButtons[level].interactable = true;
            level++;
            loop = int.TryParse(reader.ReadLine(), out line);
        }
        reader.Close();

        ApplySoundChanges();
    }

    // Update is called once per frame
    void Update()
    {
        //Code that runs in the title screen goes here
        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            if (Input.GetKeyDown(KeyCode.Escape) && canvasTracker.Count > 1)
                BackButton();
        }

        //Code that runs in the gamescene
        //Right now it only pauses the game and unpauses
        if (SceneManager.GetActiveScene().name != "StartScene")
        {
            //REMOVE LATER THIS IS ONLY TEMPORARY SO WE CAN TEST IN THE REGULAR SCENES WITHOUT ERROR
            titleCanvas.gameObject.SetActive(false);
            if (Input.GetKeyDown(KeyCode.Escape) && (GameManager.currentGameState == GameState.PLAYERTURN || GameManager.currentGameState == GameState.ENEMYTURN))
            {
                previousGameState = GameManager.currentGameState;
                GameManager.currentGameState = GameState.PAUSED;
                pauseCanvas.gameObject.SetActive(true);
                canvasTracker.Push(pauseCanvas);
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && GameManager.currentGameState == GameState.PAUSED)
            {
                if (canvasTracker.Count > 1)
                    BackButton();
                else
                {
                    ResumeButton();
                    canvasTracker.Pop();
                }
            }
        }
    }

    //Loads to the next scene
    public void StartButton()
    {
        level++;

        if(MAX_LEVEL == level)
        {
            level = 1;
        }


        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>().StopInstance();
        }

        SceneManager.LoadScene(level);
        titleCanvas.gameObject.SetActive(false);
        abilityCanvas.gameObject.SetActive(true);
        canvasTracker.Clear();
    }

    //Adds a new canvas to the stack depending on the canvas passed in
    public void goForwardCanvas(Canvas canvas)
    {
        canvasTracker.Peek().gameObject.SetActive(false);
        canvasTracker.Push(canvas);
        if (SceneManager.GetActiveScene().name != "StartScene")
            canvasTracker.Peek().transform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        canvasTracker.Peek().gameObject.SetActive(true);
    }

    //Sets the current canvas to false and pop its off
    //After that sets the canvas at the top of the stack to on
    public void BackButton()
    {
        Debug.Log("Back Button Pressed");
        canvasTracker.Peek().gameObject.SetActive(false);
        canvasTracker.Pop();
        canvasTracker.Peek().gameObject.SetActive(true);
    }

    public void VolumeSlider(int index)
    {
        if (!applyButton.interactable)
            applyButton.interactable = true;

        ////Used for all volume sliders to calculate what we should change the sound settings to
        //if (index == 0)
        //{
        //    GameManager.masterVolume = masterSoundVolumeSlider.value;
        //    GameManager.musicVolume = musicSoundVolumeSlider.value * GameManager.masterVolume;
        //    GameManager.soundFXVolume = soundfxVolumeSlider.value * GameManager.masterVolume;
        //}
        //else if (index == 1)
        //    GameManager.soundFXVolume = soundfxVolumeSlider.value * GameManager.masterVolume;
        //else if (index == 2)
        //    GameManager.musicVolume = musicSoundVolumeSlider.value * GameManager.masterVolume;

    }

    //Exits application when clicked
    public void ExitButton()
    {
        if (SceneManager.GetActiveScene().name != "StartScene")
        {
            ResumeButton();
            SceneManager.LoadScene(1);
        }
        else
            Application.Quit();
    }

    //Button used to unpause the game
    public void ResumeButton()
    {
        GameManager.currentGameState = previousGameState;
        pauseCanvas.gameObject.SetActive(false);
    }

    //Button used to apply settings so user can test them in the settings screen
    public void ApplyButton()
    {
        //Writing the changes to the txt file
        writer = new StreamWriter(settingsPath);
        writer.Write(masterSoundVolumeSlider.value.ToString() + "\n");
        writer.Write(soundfxVolumeSlider.value.ToString() + "\n");
        writer.Write(musicSoundVolumeSlider.value.ToString() + "\n");
        writer.Close();

        applyButton.interactable = false;

        ApplySoundChanges();
    }

    //Button used to go back to the main menu
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    public void LevelSelectButton(int index)
    {
        SceneManager.LoadScene(index);
        level = index;
        abilityCanvas.gameObject.SetActive(true);
        canvasTracker.Peek().gameObject.SetActive(false);
        canvasTracker.Clear();
    }

    public void UpdateLevels()
    {
        StreamWriter levelWriter = new StreamWriter(Application.dataPath + "/StreamingAssets/txt/levels.txt");
        for(int i = 0; i < level; i++)
        {
            levelWriter.WriteLine("1");
        }
        levelWriter.Close();
    }

    //Loops through our lists of audiosources and adjusts their volumes
    private void ApplySoundChanges()
    {
        //foreach (AudioSource a in GameManager.musicSources)
        //{
        //    a.volume = GameManager.musicVolume;
        //}
        //foreach (AudioSource a in GameManager.soundFXSources)
        //{
        //    a.volume = GameManager.soundFXVolume;
        //}
    }
}
