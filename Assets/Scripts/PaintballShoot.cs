using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaintballShoot : MonoBehaviour
{
    /// This script controls the paintball, how the player shoots it, and a bit of the model scaling


    private bool onMobile;

    public int playerNum = 1; //vars for which player is currently playing and their colors
    public Color playerColor;
    public Color[] playerColors = new Color[3];
    public ColorSliderScript playerOneSlider, playerTwoSlider;


    Vector3 startPos; //physics and important positions for spawning
    Vector3 spawnPos;
    int dead_timer = -100; //dead timer starts negative so the balloon will drop later for the opening camera movement
    float grav = 0.03f;
    public Vector3 velocity;

    /// State machine for the paintball is done with enums
    public enum STATE_TYPE { DEFAULT, SPAWNING, WAITING, AIMING, TRAVELING, DEAD };
    public STATE_TYPE state;
    

    Vector3 dragPoint; //variables used for dragging the slingshot sling
    Vector3 dragOffset;
    private float screenDragConstant;
    private float slingshotHeightOnScreen;

    public GameObject paintballModel; //references for mod
    Material paintballModelMaterial;
    float paintballBounceVel = 0;
    public GameObject slingshotSling;

    void Start()
    {
        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.Portrait;

        onMobile = (Application.platform == RuntimePlatform.Android) || (Application.platform == RuntimePlatform.IPhonePlayer);

        slingshotHeightOnScreen = Screen.height * 0.31f; //this variable is used to adjust the sling dragging for different mobile screens
        screenDragConstant = (420f / slingshotHeightOnScreen);

        state = STATE_TYPE.DEFAULT;

        playerColors[0] = Color.black;
        playerColors[1] = Color.red;
        playerColors[2] = Color.blue;
        playerNum = 2;

        paintballModel = gameObject.transform.GetChild(0).gameObject;
        paintballModelMaterial = paintballModel.GetComponent<Renderer>().material;
        paintballModelMaterial.SetColor("_Color", playerColors[playerNum]);
        
        startPos = transform.position;
        spawnPos = startPos + Vector3.up * 8f;
        transform.position = spawnPos;

    }
    
    ///Update and the SwitchState function control the states of the paintball
    void Update()
    {
        if (state == STATE_TYPE.DEFAULT)
        {
            playerColors[1] = playerOneSlider.selectedColor;
            playerColors[2] = playerTwoSlider.selectedColor;
            return;
        }

        /// these floats adjusts the aiming for different screen sizes
        /// having them in update here lets you change resolutions in the editor but this should be deleted if exporting
        slingshotHeightOnScreen = Screen.height * 0.31f;
        screenDragConstant = (420f / slingshotHeightOnScreen); 

        MobileInputController();

        velocity += new Vector3(0f, -grav, 0f); //adding grav here so i can set to zero in the states if i need to before position is moved
        
        switch(state)
        {
            case STATE_TYPE.SPAWNING: //spawning = when the ball is falling from spawnPos to startPos
                if (transform.position.y < startPos.y)
                {
                    SwitchState(STATE_TYPE.WAITING);
                }
                break;
            case STATE_TYPE.WAITING: //waiting = when the ball is resting in the slingshot
                PaintballPendulum(); //this function makes the paintball a little bouncy
                velocity = Vector3.zero;
                transform.position = startPos;
                break;
            case STATE_TYPE.AIMING: //aiming = when you're dragging it
                velocity = Vector3.zero;
                paintballModel.transform.localScale = Vector3.one * 30f;
                break;
            case STATE_TYPE.TRAVELING: //traveling = when the ball is in the air
                slingshotSling.transform.position = VectorSpring(slingshotSling.transform.position, startPos, 30f);
                if (transform.position.y < -20)
                {
                    SwitchState(STATE_TYPE.DEAD);
                }
                break;
            case STATE_TYPE.DEAD: //dead = a state where nothing happens so players can react to the shot
                velocity = Vector3.zero;
                dead_timer++;
                if (dead_timer < 40)
                {
                    return;
                }
                dead_timer = 0;
                SwitchState(STATE_TYPE.SPAWNING);
                break;
        }
        
        transform.position += velocity;
        paintballModel.transform.LookAt(transform.position + new Vector3(velocity.x, velocity.y * 1.5f, velocity.z));
    }

    /// This function switches the state and handles all one-time variable sets ie velocity becoming zero or setting the start position
    void SwitchState(STATE_TYPE newState)
    {
        state = newState;
        switch (state)
        {
            case STATE_TYPE.SPAWNING:
                velocity = Vector3.zero;
                break;
            case STATE_TYPE.WAITING:
                transform.position = startPos;
                paintballBounceVel = 15f;
                break;
            case STATE_TYPE.AIMING:
                if (!onMobile)
                {
                    dragPoint = new Vector3(Input.mousePosition.x, slingshotHeightOnScreen, 0f);
                }
                break;
            case STATE_TYPE.TRAVELING:
                break;
            case STATE_TYPE.DEAD:
                transform.position = spawnPos;
                velocity = Vector3.zero;
                GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
                SwitchPlayerTurn(playerNum);
                break;
            case STATE_TYPE.DEFAULT:
                transform.position = spawnPos;
                break;
            default:
                break;
        }
    }

    /// This functions enable/disable the paintball for the menu to be able to interact with the paintball
    public void TurnOnPaintball()
    {
        SwitchState(STATE_TYPE.DEAD);
    }

    public void TurnOffPaintball()
    {
        SwitchState(STATE_TYPE.DEFAULT);
    }

    //switching player turn function
    void SwitchPlayerTurn(int player)
    {
        switch (playerNum)
        {
            case 1:
                playerNum = 2;
                break;
            case 2:
                playerNum = 1;
                break;
        }
        paintballModelMaterial.SetColor("_Color", playerColors[playerNum]);
    }

    /// Spring for the slingshot sling to look like it's firing the paintball
    public static float FloatSpring(float from, float to, float time)
    {
        time = Mathf.Clamp01(time);
        time = (Mathf.Sin(time * Mathf.PI * (.2f + 2.5f * time * time * time)) * Mathf.Pow(1f - time, 2.2f) + time) * (1f + (1.2f * (1f - time)));
        return from + (to - from) * time;
    }

    public static Vector3 VectorSpring(Vector3 from, Vector3 to, float time)
    {
        return new Vector3(FloatSpring(from.x, to.x, time), FloatSpring(from.y, to.y, time), FloatSpring(from.z, to.z, time));
    }

    /// Pendulum math that makes the balloon a tiny bit more juicy when it lands by changing its scale
    void PaintballPendulum()
    {
        paintballBounceVel += (30f - paintballModel.transform.localScale.z);
        paintballBounceVel *= 0.9999f;

        float zval = (paintballModel.transform.localScale.z + paintballBounceVel) * 0.5f;
        paintballModel.transform.localScale = new Vector3(30f + (30f-zval), 30f + (30f - zval), zval);
    }

    /// The rest of the script is used for input functions
    /// first is mouse for desktop and after is touch for mobile
    void OnMouseDown()
    {
        if (!onMobile && state == STATE_TYPE.WAITING)
        {
            SwitchState(STATE_TYPE.AIMING);
        }
    }

    void OnMouseDrag()
    {
        if (!onMobile && state == STATE_TYPE.AIMING)
        {
            //this code does the math for how much the slingshot moves when you're dragging it
            dragOffset = dragPoint - new Vector3(Input.mousePosition.x,Mathf.Clamp(Input.mousePosition.y, 0f, slingshotHeightOnScreen),Input.mousePosition.z);
            Vector3 dragVector = startPos - new Vector3(dragOffset.x, dragOffset.y * screenDragConstant, dragOffset.y * screenDragConstant) * 0.01f;
            transform.position = Vector3.Lerp(startPos, dragVector, 0.5f);
            
            slingshotSling.transform.position = transform.position; //this moves the sling model to the drag point
            slingshotSling.transform.LookAt(startPos);
        }
    }

    void OnMouseUp()
    {
        if (!onMobile && state == STATE_TYPE.AIMING)
        {
            SwitchState(STATE_TYPE.TRAVELING);
            Vector3 angle = (startPos - transform.position) * 0.03f;
            velocity = new Vector3(angle.x * 4f, dragOffset.y * screenDragConstant * 0.0020f, angle.z * 14f); //i went for just coding the physics myself instead of using rigidbodies
        }
    }

    void MobileInputController()
    {
        if (!onMobile)
        {
            return;
        }

        foreach (Touch touch in Input.touches)
        {
            if (state == STATE_TYPE.WAITING && touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray))
                {
                    if (state == STATE_TYPE.WAITING)
                    {
                        dragPoint = new Vector3(touch.position.x, slingshotHeightOnScreen, 0f);
                        SwitchState(STATE_TYPE.AIMING);
                    }
                }
            } else if (state == STATE_TYPE.AIMING)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    //aiming math code
                    dragOffset = dragPoint - new Vector3(touch.position.x, Mathf.Clamp(touch.position.y, 0f, slingshotHeightOnScreen), 0f);

                    Vector3 dragVector = startPos - new Vector3(dragOffset.x, dragOffset.y * screenDragConstant, dragOffset.y * screenDragConstant) * 0.01f;
                    transform.position = Vector3.Lerp(startPos, dragVector, 0.5f);

                    slingshotSling.transform.position = transform.position; //this moves the sling model to the drag point
                    slingshotSling.transform.LookAt(startPos);
                }
                if (touch.phase == TouchPhase.Ended)
                {
                    SwitchState(STATE_TYPE.TRAVELING);
                    Vector3 angle = (startPos - transform.position) * 0.03f;
                    velocity = new Vector3(angle.x * 4f, dragOffset.y * screenDragConstant * 0.0020f, angle.z * 14f);
                }
            }
        }
    }

}