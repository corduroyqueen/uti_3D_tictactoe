using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TictactoeBoard : MonoBehaviour
{
    //script that handles the tictactoe game/board/scoring, as well as the paintball colliding with the board
    private int[,] board = new int[3, 3];
    private GameObject[,] boardPanels = new GameObject[3, 3];
    public GameObject panelPrefab;
    private int side = 3;
    private int player_one = 1;
    private int player_two = 2;
    private int recent_player = 0;
    int[] score = new int[2];

    public GameObject endingCanvasGroupObject;
    public Text endingText;

    private GameObject panelContainer;

    void Start()
    {
        //this instantiates the panels for game board from a panel prefab
        panelContainer = new GameObject("panelContainer");
        for (int y=0; y<side; y++)
        {
            for (int x=0; x<side; x++)
            {
                boardPanels[y, x] = Instantiate(panelPrefab, new Vector3(x*3f-3, (side-y)*3f-1.5f, 20f), Quaternion.identity, panelContainer.transform);
            }
        }
    }

    //next 3 functions check if a row is all the same value, if it is then someone's won
    bool rowWinCheck ()
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

    bool columnWinCheck()
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

    bool diagonalWinCheck()
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
    //next three scripts check if a row/column/diagonal is impossible to score in
    //which means it's tied
    //the way it does this is checking if there's at least one red and one blue in the row
    bool generalTieCheck(int posOne, int posTwo)
    {
        return (posOne != posTwo) && ((posOne + posTwo) > 2); //since the board is actually ints i can check if theres a red and a blue in the row pretty easily that way
    }

    bool rowTieCheck()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!(generalTieCheck(board[i,0],board[i,1]) ||
            generalTieCheck(board[i, 1], board[i, 2]) ||
            generalTieCheck(board[i, 0], board[i, 2])))
            {
                return false;
            }
        }
        return true;
    }

    bool columnTieCheck()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!(generalTieCheck(board[0, i], board[1,i]) ||
            generalTieCheck(board[1, i], board[2, i]) ||
            generalTieCheck(board[0, i], board[2, i])))
            {
                return false;
            }
        }
        return true;
    }

    bool diagonalTieCheck()
    {
        if (!((generalTieCheck(board[0, 0], board[1, 1]) ||
            generalTieCheck(board[1, 1], board[2, 2]) ||
            generalTieCheck(board[0, 0], board[2, 2])) &&
            (generalTieCheck(board[0, 2], board[1, 1]) ||
            generalTieCheck(board[1, 1], board[2, 0]) ||
            generalTieCheck(board[0, 2], board[2, 0]))))
        {
            return false;
        }

        return true;
    }

    //next three are bool checks, mostly for readability
    bool gameOverWinner()
    {
        return (rowWinCheck() || columnWinCheck() || diagonalWinCheck());
    }

    bool gameOverDraw()
    {
        return (rowTieCheck() && columnTieCheck() && diagonalTieCheck());
    }

    bool checkEndGame()
    {
        return (gameOverDraw() || gameOverWinner());
    }

    //this on collision enter handles what happens when the ball collides with the board
    //basically it runs some math and sees which quadrant the paintball is in
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameObject ball = collision.gameObject;
            Vector2Int ballPos = new Vector2Int(roundTarget(9f-ball.transform.position.y), roundTarget(ball.transform.position.x + 4.5f - 0.2f));

            if (board[ballPos.x, ballPos.y] == 0) { //checking if space is empty i.e. 0
                PaintballShoot ballScript = ball.GetComponent<PaintballShoot>();

                board[ballPos.x, ballPos.y] = ballScript.playerNum;
                boardPanels[ballPos.x, ballPos.y].GetComponent<Renderer>().material.SetColor("_Color", ballScript.playerColors[ballScript.playerNum]);
                recent_player = ballScript.playerNum; //easier to figure out who's won by storing the recent player than checking the entire board
            }

            if (checkEndGame()) //if the games over starts the ending menu
            {
                ball.GetComponent<PaintballShoot>().turnOffPaintball();
                TriggerEndingMenu();
            }
        }
    }

    int roundTarget(float position) //this is the math for checking the quadrant
    {
        return (int) Mathf.Clamp((Mathf.Round(position)/3f),0,2);
    }
    
    void TriggerEndingMenu()
    {
        startMenuFadeIn(endingCanvasGroupObject);

        if (gameOverDraw())
        {
            endingText.text = "TIE GAME!";
        }
        else if (gameOverWinner())
        {
            score[recent_player - 1]++;
            if (recent_player == player_one)
            {
                endingText.text = "RED WINS!";
            } 
            else 
            {
                endingText.text = "BLUE WINS!";
            }
        }
    }

    void startMenuFadeIn(GameObject canvasObject)
    {
        canvasObject.GetComponent<CanvasGroup>().interactable = true;
        canvasObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        canvasObject.GetComponent<FadeCanvasScript>().startFadeInCanvasGroup();
    }

    //cute board wiping coroutine
    public void startWipeBoard()
    {
        StartCoroutine(wipeBoard());
    }

    IEnumerator wipeBoard()
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
