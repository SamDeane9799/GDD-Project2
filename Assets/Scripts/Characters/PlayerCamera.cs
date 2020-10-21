﻿using System.Collections;
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
    public Canvas levelCanvas;
    public Text levelText;
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
    private int highestLevelAcheived;

    Vector2 position = new Vector2(0, 0);
    public GameState previousGameState;

    void Awake()
    {
        DontDestroyOnLoad(this);
        canvasTracker.Push(titleCanvas);

        SceneManager.sceneLoaded += OnLoad;
        //if(SceneManager.GetActiveScene().name != "StartScene")
        //{
        //    abilityCanvas.gameObject.SetActive(true);
        //}

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
            if (level < levelSelectButtons.Count)
                levelSelectButtons[level].interactable = true;
            level++;
            loop = int.TryParse(reader.ReadLine(), out line);
        }
        reader.Close();
        highestLevelAcheived = level;

        ApplySoundChanges();
    }

    // Update is called once per frame
    void Update()
    {
        //Code that runs in the title screen goes here
        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            //titleCanvas.gameObject.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Escape) && canvasTracker.Count > 1)
                BackButton();
        }

        //Code that runs in the gamescene
        //Right now it only pauses the game and unpauses
        if (SceneManager.GetActiveScene().name != "StartScene")
        {
            //REMOVE LATER THIS IS ONLY TEMPORARY SO WE CAN TEST IN THE REGULAR SCENES WITHOUT ERROR
            //titleCanvas.gameObject.SetActive(false);
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
            if (levelText.color.a <= 0.01f && levelCanvas.gameObject.activeSelf)
            {
                Debug.Log("Disabling");
                levelCanvas.gameObject.SetActive(false);
            }
        }
    }

    //Loads to the next scene
    public void StartButton()
    {
        //level++;
        Debug.Log("Level value: " + level);

        // This if statement facilitates "Completion Mode"
        if (level > MAX_LEVEL)
        {
            Debug.Log("MAX_LEVEL in PlayerCamera has been exceeded, so we're loading level 1");
            level = 1;
        }

        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>().StopInstance();
        }

        Debug.Log("Loading Level " + level);
        GameManager.gameManagerObject.LoadNextScene();
        titleCanvas.gameObject.SetActive(false);
        abilityCanvas.gameObject.SetActive(true);
        levelCanvas.gameObject.SetActive(true);
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
            canvasTracker.Pop();
            abilityCanvas.gameObject.SetActive(false);
            titleCanvas.gameObject.SetActive(true);
            SceneManager.LoadScene(0);
            Awake();
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
        //SceneManager.LoadScene(index);
        level = index;
        GameManager.gameManagerObject.LoadNextScene();
        abilityCanvas.gameObject.SetActive(true);
        canvasTracker.Peek().gameObject.SetActive(false);
        canvasTracker.Clear();
    }

    public void UpdateLevels()
    {
        if (level >= highestLevelAcheived) // Ensures that the player can't "re-lock" levels by completing earlier levels acesssed via the level selector
        {
            StreamWriter levelWriter = new StreamWriter(Application.dataPath + "/StreamingAssets/txt/levels.txt");
            for (int i = 0; i < level; i++)
            {
                levelWriter.WriteLine("1");
            }

            // With the other code involved, this essentially adds a "completion mode" where the start button loads the first level but all of the levels are accessible in "level select"
            if (level == MAX_LEVEL)
                levelWriter.WriteLine("1");

            levelWriter.Close();

            highestLevelAcheived = level;
        }
    }

    public void OnLoad(Scene scene, LoadSceneMode mode)
    {
        //Using this to load in the player when we load into the specific scene
        if (scene.name != "StartScene")
        {
            Debug.Log("Starting level canvas");
            levelCanvas.gameObject.SetActive(true);
            levelText.text = "Level " + level;
            levelText.color = new Color(levelText.color.r, levelText.color.g, levelText.color.b, 1.0f);
        }
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
