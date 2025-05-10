using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum ChessFlag
{
    None,
    player1,
    player2,
}


public class ChessGrid : MonoBehaviour
{
    public Button gridButton;
    public Image chessImage;
    public ChessFlag flag;

    // Start is called before the first frame update
    void Start()
    {
        gridButton = GetComponent<Button>();
        Transform chessTransform = transform.Find("chess");
        if (chessTransform != null)
        {
            chessImage = chessTransform.GetComponent<Image>();
        }
        // chessImage = GetComponentInChildren<Image>();

        gridButton.onClick.AddListener(OnClickedChessGrid);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickedChessGrid()
    {
        if (GameManager.Instance.gameOver)
        {
            return;
        }
        if (GameManager.Instance.playWay == PlayWays.PVE && GameManager.Instance.gameRound == GameRound.player2)
        {
            return;
        }
        DrawChessImage();
    }

    public void DrawChessImage()
    {
        if (GameManager.Instance.gameRound == GameRound.player1)
        {
            chessImage.sprite = GameManager.Instance.playerChessSprite1;
            Color color = chessImage.color;
            color.a = 1.0f;
            chessImage.color = color;
            flag = ChessFlag.player1;
            // 播放落子声音
            GameManager.Instance.PlayingAudioClip(GameManager.Instance.playerClip1);
        }
        else
        {
            chessImage.sprite = GameManager.Instance.playerChessSprite2;
            Color color = chessImage.color;
            color.a = 1.0f;
            chessImage.color = color;
            flag = ChessFlag.player2;
            // 播放落子声音
            GameManager.Instance.PlayingAudioClip(GameManager.Instance.playerClip2);
        }
        // 设置棋格按钮不可交互
        gridButton.interactable = false;
        GameControl gameControl = GetComponentInParent<GameControl>();
        gameControl.OnChessGridClicked();
    }

    public void ResetChessStatus()
    {
        // 重置棋格状态
        gridButton.interactable = true;
        chessImage.sprite = null;
        Color color = chessImage.color;
        color.a = 0.0f;
        chessImage.color = color;
        flag = ChessFlag.None;
    }

    public void SetButtonEnable(bool bl)
    {
        gridButton.interactable = bl;
    }
}
