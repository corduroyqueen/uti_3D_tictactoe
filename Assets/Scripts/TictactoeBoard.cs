using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TictactoeBoard : MonoBehaviour
{
    /// This script handles the tictactoe game board, and its win/tie state
    /// it also checks where the paintball has collided with it
    private int[,] board = new int[3, 3];
    private GameObject[,] boardPanels = new GameObject[3, 3];
    public GameObject panelPrefab;
    private int side = 3;
    private int player_one = 1;
    private int player_two = 2;
    private int recent_player = 0;
    int[] score = new int[2];

    public GameObject endingCanvasGroupObject;
    public TMPro.TextMeshProUGUI endingWinText, endingTieText, p1ScoreText, p2ScoreText;

    private GameObject panelContainer;

    void Start()
    {
        panelContainer = new GameObject("panelContainer"); //This instantiates the panels for game board from a panel prefab
        for (int y=0; y<side; y++)
        {
            for (int x=0; x<side; x++)
            {
                boardPanels[y, x] = Instantiate(panelPrefab, new Vector3(x*3f-3, (side-y)*3f-1.5f, 20f), Quaternion.identity, panelContainer.transform);
            }
        }
    }

    /// The next 3 functions check if a row/column/diagonal is all the same value, if it is then someone's won
    bool RowWinCheck ()
    {
        for (int i = 0; i < 3; i++)
        {
            if(board[i,0] == board[i,1] &&
            board[i,1] == board[i,2] &&
            board[i,0]!=0)
            {
                return true;
            }
            
        }
        return false;
    }

    bool ColumnWinCheck()
    {
        for (int i = 0; i < 3; i++)
        {
            if (board[0, i] == board[1, i] &&
            board[1, i] == board[2, i] &&
            board[0, i] != 0)
            {
                return true;
            }
            
        }
        return false;
    }

    bool DiagonalWinCheck()
    {
        if (board[0,0] == board[1,1] &&
        board[1,1] == board[2,2] &&
        board[0, 0] != 0)
        {
            return true;
        }

        if (board[0, 2] == board[1, 1] &&
        board[1, 1] == board[2, 0] &&
        board[1, 1] != 0)
        {
            return true;
        }

        return false;
    }

    /// Next three scripts check for a tie by seeing if a row/column/diagonal is impossible to score in
    /// (the way it does this is checking if there's at least one red and one blue in the row)
    bool GeneralTieCheck(int posOne, int posTwo)
    {
        return (posOne != posTwo) && ((posOne + posTwo) > 2); //since the board is actually ints i can check if theres a red and a blue in the row just by adding them
    }

    bool RowTieCheck()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!(GeneralTieCheck(board[i,0],board[i,1]) ||
            GeneralTieCheck(board[i, 1], board[i, 2]) ||
            GeneralTieCheck(board[i, 0], board[i, 2])))
            {
                return false;
            }
        }
        return true;
    }

    bool ColumnTieCheck()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!(GeneralTieCheck(board[0, i], board[1,i]) ||
            GeneralTieCheck(board[1, i], board[2, i]) ||
            GeneralTieCheck(board[0, i], board[2, i])))
            {
                return false;
            }
        }
        return true;
    }

    bool DiagonalTieCheck()
    {
        if (!((GeneralTieCheck(board[0, 0], board[1, 1]) ||
            GeneralTieCheck(board[1, 1], board[2, 2]) ||
            GeneralTieCheck(board[0, 0], board[2, 2])) &&
            (GeneralTieCheck(board[0, 2], board[1, 1]) ||
            GeneralTieCheck(board[1, 1], board[2, 0]) ||
            GeneralTieCheck(board[0, 2], board[2, 0]))))
        {
            return false;
        }

        return true;
    }

    /// Next three functions are bool checks for win/draw
    bool GameOverWinner()
    {
        return (RowWinCheck() || ColumnWinCheck() || DiagonalWinCheck());
    }

    bool GameOverTie()
    {
        return (RowTieCheck() && ColumnTieCheck() && DiagonalTieCheck());
    }

    bool checkEndGame()
    {
        return (GameOverTie() || GameOverWinner());
    }

    /// This oncollisionenter handles what happens when the ball collides with the board
    /// it runs some math and sees which quadrant the paintball is in
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameObject ball = collision.gameObject;
            Vector2Int ballPos = new Vector2Int(RoundTarget(9f-ball.transform.position.y), RoundTarget(ball.transform.position.x + 4.5f - 0.2f));
            PaintballShoot ballScript = ball.GetComponent<PaintballShoot>(); //need the paintball script for the playercolor

            if (board[ballPos.x, ballPos.y] == 0) { //checking if space is empty i.e. 0

                board[ballPos.x, ballPos.y] = ballScript.playerNum;
                boardPanels[ballPos.x, ballPos.y].GetComponent<Renderer>().material.SetColor("_Color", ballScript.playerColors[ballScript.playerNum]);
                recent_player = ballScript.playerNum; //easier to figure out who's won by storing the recent player than checking the entire board
            }

            if (checkEndGame()) //if this new move has ended the game, start the ending menu
            {
                ball.GetComponent<PaintballShoot>().TurnOffPaintball();
                TriggerEndingMenu(ballScript.playerColors[ballScript.playerNum]);
            }
        }
    }

    int RoundTarget(float position) //this is the math for checking the quadrant the ball's landed in
    {
        return (int) Mathf.Clamp((Mathf.Round(position)/3f),0,2);
    }
    
    /// The rest of the script handles the player score and ending text
    /// This first functions just sets the ending text depending on if its a win, tie, and who's won
    void TriggerEndingMenu(Color winnerColor)
    {
        StartMenuFadeIn(endingCanvasGroupObject);

        if (GameOverTie())
        {
            endingWinText.text = "";
            endingTieText.text = "TIE GAME!";
        }
        else if (GameOverWinner())
        {
            score[recent_player - 1]++;
            p1ScoreText.text = "P1: " + score[0];
            p2ScoreText.text = "P2: " + score[1];
            endingWinText.color = winnerColor;
            endingTieText.text = "\nWINS!";
            if (recent_player == player_one)
            {
                endingWinText.text = "P1";
            } 
            else 
            {
                endingWinText.text = "P2";
            }
        }
    }
    /// General menu fade in function, can use for start menu or ending menu
    void StartMenuFadeIn(GameObject canvasObject)
    {
        canvasObject.GetComponent<CanvasGroup>().interactable = true;
        canvasObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        canvasObject.GetComponent<FadeCanvasScript>().StartFadeInCanvasGroup();
    }

    /// Board wiping coroutine upon reset
    public void StartWipeBoard()
    {
        StartCoroutine(WipeBoard());
    }

    IEnumerator WipeBoard()
    {
        for (int y = 0; y < side; y++)
        {
            for (int x = 0; x < side; x++)
            {
                board[y, x] = 0;
                boardPanels[y, x].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                yield return new WaitForSeconds(0.2f);
            }
        }
        yield break;
    }
}
