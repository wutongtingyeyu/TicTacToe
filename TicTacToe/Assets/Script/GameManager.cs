using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


// 玩法方式
public enum PlayWays
{
    PVE,           // 人机
    PVP,           // 玩家vs玩家
}

// 游戏回合
public enum GameRound
{
    player1,         
    player2,
}


public class GameManager : MonoBehaviour
{
    // 定义棋子精灵
    public Sprite playerChessSprite1;
    public Sprite playerChessSprite2;


    public Button playerButton1;                             // 人机对战按钮
    public Button playerButton2;                             // 玩家对战按钮
    public Button closeButton;
    public Button audioButton;                               // 音量键按钮
    public Button menuButton;                                // 首界面菜单按钮

    public GameObject mainCanvas;
    public GameObject gameCanvas;

    public AudioClip playerClip1;
    public AudioClip playerClip2;
    public AudioClip victoryClip;
    public AudioClip failClip;
    public AudioClip drawClip;
    
    public bool gameOver;
    public bool gameMusic;

    public GameRound gameRound;

    public PlayWays playWay;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }

        gameOver = false;
        gameMusic = true;
        playWay = PlayWays.PVE;
        gameRound = GameRound.player1;
    }

    // Start is called before the first frame update
    void Start()
    {
        playerButton1.onClick.AddListener(SetGamePVE);
        playerButton2.onClick.AddListener(SetGamePVP);
        closeButton.onClick.AddListener(ExitGame);
        audioButton.onClick.AddListener(SetAudioEnable);
        menuButton.onClick.AddListener(ReturnMainWindow);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchingGameRound()
    {
        // 转换游戏回合
        if (gameRound == GameRound.player1)
        {
            gameRound = GameRound.player2;
        }
        else
        {
            gameRound = GameRound.player1;
        }
    }

    public void SetGamePVE()
    {
        playWay = PlayWays.PVE;
        mainCanvas.SetActive(false);
        gameCanvas.SetActive(true);
    }

    public void SetGamePVP()
    {
        playWay = PlayWays.PVP;
        mainCanvas.SetActive(false);
        gameCanvas.SetActive(true);
    }

    public void  SetAudioEnable()
    {
        if (gameMusic)
        {
            gameMusic = false;
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.Stop();
            Color color = new Color(0.8f, 0.4f, 0.4f, 1.0f);
            audioButton.image.color = color;
        }
        else
        {
            gameMusic = true;
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.Play();
            Color color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            audioButton.image.color = color;
        }
    }

    public void PlayingAudioClip(AudioClip audioClip)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(audioClip);
    }

    public void ReturnMainWindow()
    {
        GameControl gameControl = FindFirstObjectByType<GameControl>();
        gameControl.ResetGame();
        // 返回主窗口
        gameCanvas.SetActive(false);
        mainCanvas.SetActive(true);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
