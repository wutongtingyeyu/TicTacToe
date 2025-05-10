using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    public ChessGrid[] chessGrids = new ChessGrid[9];        // 棋格
    public Button exitButton;                                // 退出游戏按钮
    public Button restartButton;                             // 重置游戏按钮

    public int count = 0;                                    // 纪录落子数量

    public Text titleText;
    public Text tipText;

    // Start is called before the first frame update
    void Start()
    {
        chessGrids = GetComponentsInChildren<ChessGrid>();
        exitButton.onClick.AddListener(ExitGame);
        restartButton.onClick.AddListener(ResetGame);

        //int[,] result = FindAllWinCondition(3, 3);           // 3*3的棋盘， 3子连线即算赢
        /*
        string output = "二维数组:\n";
        for (int i = 0; i < result.GetLength(0); i++)
        {
            for (int j = 0; j < result.GetLength(1); j++)
            {
                output += result[i, j] + " ";
            }
            output += "\n";
        }
        print(output);
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChessGridClicked()
    {
        // 落子后, 判断游戏结果
        count += 1;
        ChessFlag gameWiner = JudgmentWiner();
        if (gameWiner == ChessFlag.player1)
        {
            GameOver(ChessFlag.player1);
            GameManager.Instance.PlayingAudioClip(GameManager.Instance.victoryClip);
            return;
        }
        else if (gameWiner == ChessFlag.player2)
        {
            GameOver(ChessFlag.player2);
            GameManager.Instance.PlayingAudioClip(GameManager.Instance.failClip);
            return;
        }
        else if (count == 9)
        {
            GameOver(ChessFlag.None);
            GameManager.Instance.PlayingAudioClip(GameManager.Instance.drawClip);
            return;
        }
        //  切换回合
        RoundSwitching();
    }

    public void RoundSwitching()
    {   // 转换游戏回合
        GameManager.Instance.SwitchingGameRound();
        UpdateTextMessage();
        // 如果当前是人机对战，则电脑进行落子
        if (GameManager.Instance.playWay == PlayWays.PVE && GameManager.Instance.gameRound == GameRound.player2)
        {
            StartCoroutine("AsyncComputerPlaying");
        }
    }

    IEnumerator AsyncComputerPlaying()
    {
        yield return new WaitForSeconds(1);
        ComputerPlaying();
    }

    public void ComputerPlaying()
    {
        // 获取可以落子的位置
        List<ChessGrid> emptyChessGrid = new List<ChessGrid>();
        foreach (ChessGrid chessGrid in chessGrids)
        {
            if (chessGrid.flag == ChessFlag.None)
            {
                emptyChessGrid.Add(chessGrid);
            }
        }
        // 判断是否有电脑落子后即可获胜的位置
        ChessGrid nextChessGrid = GetWinChessGrid(emptyChessGrid, ChessFlag.player2);
        if (nextChessGrid != null)
        {   
            // 落子赢得游戏
            nextChessGrid.DrawChessImage();
            return;
        }
        // 判断玩家落子后即可获胜的位置，进行围堵
        nextChessGrid = GetWinChessGrid(emptyChessGrid, ChessFlag.player1);
        if (nextChessGrid != null)
        {
            // 落子围堵玩家
            nextChessGrid.DrawChessImage();
            return;
        }
        // 下一步棋无法获胜，随机找个位置下
        if (emptyChessGrid.Count > 0)
        {
            int emptyCount = emptyChessGrid.Count;
            int index = Random.Range(0, emptyCount);
            nextChessGrid = emptyChessGrid[index];
            nextChessGrid.DrawChessImage();
        }
    }

    public ChessGrid GetWinChessGrid(List<ChessGrid> emptyChessGrids, ChessFlag chessFlag)
    {
        foreach (ChessGrid chessGrid in emptyChessGrids)
        {
            chessGrid.flag = chessFlag;
            ChessFlag result = JudgmentWiner();
            if (result == chessFlag)
            {  
                chessGrid.flag = ChessFlag.None;
                return chessGrid;
            }
            chessGrid.flag = ChessFlag.None;
        }
        return null;
    }

    public int[,] FindAllWinCondition(int size, int lineCount)
    {
        // 获取 n*n 棋盘所有可能获胜的情况
        List<List<int>> winList = new List<List<int>>();
        int sizeSquare = size * size;

        int[] hArray = new int[lineCount];
        int[] vArray = new int[lineCount];
        int[] leftUpArray = new int[lineCount];
        int[] leftDownArray = new int[lineCount];

        for (int i=0; i<sizeSquare; i++)
        {
            for (int j=0; j<lineCount; j++)
            {
                // 水平连线
                hArray[j] = i + j;
                // 垂直连线
                vArray[j] = i + j * size;
                // 左上到右下 对角下
                leftUpArray[j] = i + j * size + j;
                // 右上到左下对角线
                leftDownArray[j] = i + j * size -j;
            }
            // 判断数组内的数据，是否符合要求，从而满足胜利条件   
            if (RequirementRange(sizeSquare, hArray, size, 1))
            {
                winList.Add(hArray.ToList());
            }
            if (RequirementRange(sizeSquare, vArray, size, 2))
            {
                winList.Add(vArray.ToList());
            }
            if (RequirementRange(sizeSquare, leftUpArray, size, 3))
            {
                winList.Add(leftUpArray.ToList());
            }
            if (RequirementRange(sizeSquare, leftDownArray, size, 4))
            {
                winList.Add(leftDownArray.ToList());
            }
        }
        int rows = winList.Count;
        int cols = lineCount;
        int[,] winArray = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                winArray[i, j] = winList[i][j];
            }
        }
        return winArray;
    }

    private bool RequirementRange(int length, int[] array, int size, int flag)
    {
        // 判断数组内的元素是否符合数值区间要求
        int maxNumber = array.Max();
        int minNumber = array.Min();
        if (maxNumber >= length || minNumber < 0)
        {
            return false;
        }
        // 将余数存入一个数组
        int[] quotients = new int[array.Length];
        int[] remainders = new int[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            quotients[i] = array[i] / size;
            remainders[i] = array[i] % size;
        }

        if (flag == 1)
        {   
            // 水平方向，判断商数是否一样
            bool result = quotients.All(x => x == quotients[0]);
            return result;
        }
        else if (flag == 2)
        {
            // 垂直方向， 判断余数是否一样
            bool result = remainders.All(x => x == remainders[0]);
            return result;
        }
        else if (flag == 3 || flag == 4)
        {
            // 左上到右下，右上到左下 对角下, 商数差值相同, 余数差值相同
            int temp1 = quotients[1] - quotients[0];
            int temp2 = remainders[1] - remainders[0];
            for (int i = 1; i < quotients.Length; i++)
            {
                if ((quotients[i] - quotients[i - 1]) != temp1)
                {
                    return false;
                }
            }
            for (int i = 1; i < remainders.Length; i++)
            {
                if ((remainders[i] - remainders[i - 1]) != temp2)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public ChessFlag JudgmentWiner()
    {
        // 每次落子后判断游戏赢家
        //int[,] winArray = FindAllWinCondition(3, 3);           // 3*3的棋盘， 3子连线即算赢
        // 先直接写死吧，如果要扩展，就用上面代码在初始化中去获取n*n棋盘， k子连线获胜的情况，再稍微调整下，
        int[,]winArray = new int[8, 3] {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8},
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8},
            {0, 4, 8}, {2, 4, 6}
        };

        for (int i = 0; i < 8; i++)
        {
            int first = winArray[i, 0];
            int second = winArray[i, 1];
            int third = winArray[i, 2];
            ChessGrid chessGrid_1 = chessGrids[first];
            ChessGrid chessGrid_2 = chessGrids[second];
            ChessGrid chessGrid_3 = chessGrids[third];

            if (chessGrid_1.flag == chessGrid_2.flag && chessGrid_1.flag ==chessGrid_3.flag && chessGrid_1.flag != ChessFlag.None)
            {
                if (chessGrid_1.flag == ChessFlag.player1)
                {
                    return ChessFlag.player1;
                }
                else if (chessGrid_1.flag == ChessFlag.player2)
                {
                    return ChessFlag.player2;
                }
            }
        }
        return ChessFlag.None;
    }

    public void GameOver(ChessFlag chessFlag)
    {
        // 游戏结束
        GameManager.Instance.gameOver = true;
        foreach (ChessGrid chessGrid in chessGrids)
        {
            chessGrid.SetButtonEnable(false);
        }
        UpdateTextMessage(chessFlag);
    }

    public void UpdateTextMessage(ChessFlag chessFlag = ChessFlag.None)
    {
        // 更新信息栏
        if (GameManager.Instance.gameOver)
        {
            // 游戏结束， 依据赢家展示对应UI
            if (chessFlag == ChessFlag.player1)
            {
                // 玩家胜利
                titleText.text = "GameOver";
                tipText.text = "Player1 Win!";
            }
            else if (chessFlag == ChessFlag.player2)
            {
                // 电脑胜利，或者玩家2胜利
                titleText.text = "GameOver";
                tipText.text = "Player2 Win!";
            }
            else
            {
                // 平局
                titleText.text = "GameOver";
                tipText.text = "平局";
            }
        }
        else
        {
            if (GameManager.Instance.gameRound == GameRound.player1)
            {
                titleText.text = "信息栏";
                tipText.text = "Player1 Round";
            }
            else if (GameManager.Instance.gameRound == GameRound.player2)
            {
                titleText.text = "信息栏";
                tipText.text = "Player2 Round";
            }
        }
    }

    public void ResetGame()
    {
        StopCoroutine("AsyncComputerPlaying");
        // 重置游戏
        foreach (ChessGrid chessGrid in chessGrids)
        {
            chessGrid.ResetChessStatus();
        }
        GameManager.Instance.gameOver = false;
        GameManager.Instance.gameRound = GameRound.player1;

        count = 0;
        titleText.text = "信息栏";
        tipText.text = "Player1 Round";
    }

    public void ExitGame()
    {
        GameManager.Instance.ExitGame();
    }
}
