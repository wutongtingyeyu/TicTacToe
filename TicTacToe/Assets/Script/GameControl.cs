using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    public ChessGrid[] chessGrids = new ChessGrid[9];        // ���
    public Button exitButton;                                // �˳���Ϸ��ť
    public Button restartButton;                             // ������Ϸ��ť

    public int count = 0;                                    // ��¼��������

    public Text titleText;
    public Text tipText;

    // Start is called before the first frame update
    void Start()
    {
        chessGrids = GetComponentsInChildren<ChessGrid>();
        exitButton.onClick.AddListener(ExitGame);
        restartButton.onClick.AddListener(ResetGame);

        //int[,] result = FindAllWinCondition(3, 3);           // 3*3�����̣� 3�����߼���Ӯ
        /*
        string output = "��ά����:\n";
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
        // ���Ӻ�, �ж���Ϸ���
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
        //  �л��غ�
        RoundSwitching();
    }

    public void RoundSwitching()
    {   // ת����Ϸ�غ�
        GameManager.Instance.SwitchingGameRound();
        UpdateTextMessage();
        // �����ǰ���˻���ս������Խ�������
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
        // ��ȡ�������ӵ�λ��
        List<ChessGrid> emptyChessGrid = new List<ChessGrid>();
        foreach (ChessGrid chessGrid in chessGrids)
        {
            if (chessGrid.flag == ChessFlag.None)
            {
                emptyChessGrid.Add(chessGrid);
            }
        }
        // �ж��Ƿ��е������Ӻ󼴿ɻ�ʤ��λ��
        ChessGrid nextChessGrid = GetWinChessGrid(emptyChessGrid, ChessFlag.player2);
        if (nextChessGrid != null)
        {   
            // ����Ӯ����Ϸ
            nextChessGrid.DrawChessImage();
            return;
        }
        // �ж�������Ӻ󼴿ɻ�ʤ��λ�ã�����Χ��
        nextChessGrid = GetWinChessGrid(emptyChessGrid, ChessFlag.player1);
        if (nextChessGrid != null)
        {
            // ����Χ�����
            nextChessGrid.DrawChessImage();
            return;
        }
        // ��һ�����޷���ʤ������Ҹ�λ����
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
        // ��ȡ n*n �������п��ܻ�ʤ�����
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
                // ˮƽ����
                hArray[j] = i + j;
                // ��ֱ����
                vArray[j] = i + j * size;
                // ���ϵ����� �Խ���
                leftUpArray[j] = i + j * size + j;
                // ���ϵ����¶Խ���
                leftDownArray[j] = i + j * size -j;
            }
            // �ж������ڵ����ݣ��Ƿ����Ҫ�󣬴Ӷ�����ʤ������   
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
        // �ж������ڵ�Ԫ���Ƿ������ֵ����Ҫ��
        int maxNumber = array.Max();
        int minNumber = array.Min();
        if (maxNumber >= length || minNumber < 0)
        {
            return false;
        }
        // ����������һ������
        int[] quotients = new int[array.Length];
        int[] remainders = new int[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            quotients[i] = array[i] / size;
            remainders[i] = array[i] % size;
        }

        if (flag == 1)
        {   
            // ˮƽ�����ж������Ƿ�һ��
            bool result = quotients.All(x => x == quotients[0]);
            return result;
        }
        else if (flag == 2)
        {
            // ��ֱ���� �ж������Ƿ�һ��
            bool result = remainders.All(x => x == remainders[0]);
            return result;
        }
        else if (flag == 3 || flag == 4)
        {
            // ���ϵ����£����ϵ����� �Խ���, ������ֵ��ͬ, ������ֵ��ͬ
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
        // ÿ�����Ӻ��ж���ϷӮ��
        //int[,] winArray = FindAllWinCondition(3, 3);           // 3*3�����̣� 3�����߼���Ӯ
        // ��ֱ��д���ɣ����Ҫ��չ��������������ڳ�ʼ����ȥ��ȡn*n���̣� k�����߻�ʤ�����������΢�����£�
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
        // ��Ϸ����
        GameManager.Instance.gameOver = true;
        foreach (ChessGrid chessGrid in chessGrids)
        {
            chessGrid.SetButtonEnable(false);
        }
        UpdateTextMessage(chessFlag);
    }

    public void UpdateTextMessage(ChessFlag chessFlag = ChessFlag.None)
    {
        // ������Ϣ��
        if (GameManager.Instance.gameOver)
        {
            // ��Ϸ������ ����Ӯ��չʾ��ӦUI
            if (chessFlag == ChessFlag.player1)
            {
                // ���ʤ��
                titleText.text = "GameOver";
                tipText.text = "Player1 Win!";
            }
            else if (chessFlag == ChessFlag.player2)
            {
                // ����ʤ�����������2ʤ��
                titleText.text = "GameOver";
                tipText.text = "Player2 Win!";
            }
            else
            {
                // ƽ��
                titleText.text = "GameOver";
                tipText.text = "ƽ��";
            }
        }
        else
        {
            if (GameManager.Instance.gameRound == GameRound.player1)
            {
                titleText.text = "��Ϣ��";
                tipText.text = "Player1 Round";
            }
            else if (GameManager.Instance.gameRound == GameRound.player2)
            {
                titleText.text = "��Ϣ��";
                tipText.text = "Player2 Round";
            }
        }
    }

    public void ResetGame()
    {
        StopCoroutine("AsyncComputerPlaying");
        // ������Ϸ
        foreach (ChessGrid chessGrid in chessGrids)
        {
            chessGrid.ResetChessStatus();
        }
        GameManager.Instance.gameOver = false;
        GameManager.Instance.gameRound = GameRound.player1;

        count = 0;
        titleText.text = "��Ϣ��";
        tipText.text = "Player1 Round";
    }

    public void ExitGame()
    {
        GameManager.Instance.ExitGame();
    }
}
