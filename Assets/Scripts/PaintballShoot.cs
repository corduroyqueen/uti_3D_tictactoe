using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaintballShoot : MonoBehaviour
{
    //dis script controls the paintball, the player shooting it, and a bit of the model scaling
    //there is only a need for one paintball at a time so i figured rather than making a prefab
    //it would be fine to just have one paintball with some states

    public int playerNum = 1;
    public Color playerColor;

    Vector3 dragPoint;
    Vector3 dragOffset;
    Vector3 startPos;
    Vector3 spawnPos;
    float grav = 0.03f;

    public GameObject paintballModel;
    Material paintballModelMaterial;
    float paintballBounceVel = 0;

    public GameObject slingshotSling;
    public Vector3 velocity;

    //enum state machine
    //i know doing state machines with classes is better but the game is so small so i went for the enums
    // ?????
    public enum STATE_TYPE { DEFAULT, SPAWNING, WAITING, AIMING, TRAVELING, DEAD };
    public STATE_TYPE state;
    int dead_timer = -100; //dead timer starts negative so the balloon will drop later for the opening camera movement

    public Text textext;

    public Color[] playerColors = new Color[3];
    private float screenDragConstant;
    private float slingshotHeightOnScreen;

    private bool onMobile;

    

    void Start()
    {
        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.Portrait;
        slingshotHeightOnScreen = Screen.height * 0.31f;
        screenDragConstant = (420f / slingshotHeightOnScreen);

        state = STATE_TYPE.DEFAULT;

        //letting the players choose the colors might be nice
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

        onMobile = (Application.platform == RuntimePlatform.Android) || (Application.platform == RuntimePlatform.IPhonePlayer);
    }
    
    //update and the switchState function after control the states
    //i know doing state machines with classes is better and more scalable than enums
    //but since its a small game i went for the enums
    void Update()
    {
        if (state == STATE_TYPE.DEFAULT)
        {
            return;
        }

        //these float adjusts the aiming for different screen sizes
        //having them in update here lets you change resolutions in the editor so im leaving them for that purpose
        slingshotHeightOnScreen = Screen.height * 0.31f;
        screenDragConstant = (420f / slingshotHeightOnScreen); 

        mobileInputController();

        velocity += new Vector3(0f, -grav, 0f); //adding grav here so i can set to zero in the states if i need to before position is moved
        
        switch(state)
        {
            case STATE_TYPE.SPAWNING:
                if (transform.position.y < startPos.y)
                {
                    switchState(STATE_TYPE.WAITING);
                }
                break;
            case STATE_TYPE.WAITING:
                paintballPendulum();
                velocity = Vector3.zero;
                transform.position = startPos;
                break;
            case STATE_TYPE.AIMING:
                velocity = Vector3.zero;
                paintballModel.transform.localScale = Vector3.one * 30f;
                break;
            case STATE_TYPE.TRAVELING:
                slingshotSling.transform.position = vSpring(slingshotSling.transform.position, startPos, 30f);
                if (transform.position.y < -20)
                {
                    switchState(STATE_TYPE.DEAD);
                }
                break;
            case STATE_TYPE.DEAD:
                velocity = Vector3.zero;
                dead_timer++;
                if (dead_timer < 40)
                {
                    return;
                }
                dead_timer = 0;
                switchState(STATE_TYPE.SPAWNING);
                break;
        }
        
        transform.position += velocity;
        paintballModel.transform.LookAt(transform.position + new Vector3(velocity.x, velocity.y * 1.5f, velocity.z));
    }

    void switchState(STATE_TYPE newState)
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
                switchPlayerTurn(playerNum);
                break;
            case STATE_TYPE.DEFAULT:
                transform.position = spawnPos;
                break;
            default:
                break;
        }
    }

    //enabling/disabling functions for the menu buttons
    public void turnOnPaintball()
    {
        switchState(STATE_TYPE.DEAD);
    }

    public void turnOffPaintball()
    {
        switchState(STATE_TYPE.DEFAULT);
    }

    //switching player turn function
    void switchPlayerTurn(int player)
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

    //spring for the slingshot sling to fire
    //i want it to be more springy and bounce around but not sure i have time
    public static float fSpring(float from, float to, float time)
    {
        time = Mathf.Clamp01(time);
        time = (Mathf.Sin(time * Mathf.PI * (.2f + 2.5f * time * time * time)) * Mathf.Pow(1f - time, 2.2f) + time) * (1f + (1.2f * (1f - time)));
        return from + (to - from) * time;
    }

    public static Vector3 vSpring(Vector3 from, Vector3 to, float time)
    {
        return new Vector3(fSpring(from.x, to.x, time), fSpring(from.y, to.y, time), fSpring(from.z, to.z, time));
    }

    //some basic pendulum math that makes the balloon a tiny bit more juicy when it lands
    void paintballPendulum()
    {
        paintballBounceVel += (30f - paintballModel.transform.localScale.z);
        paintballBounceVel *= 0.9999f;

        float zval = (paintballModel.transform.localScale.z + paintballBounceVel) * 0.5f;
        paintballModel.transform.localScale = new Vector3(30f + (30f-zval), 30f + (30f - zval), zval);
    }

    //next are all the input functions
    //first is mouse for desktop and after is touch for mobile
    void OnMouseDown()
    {
        if (!onMobile && state == STATE_TYPE.WAITING)
        {
            switchState(STATE_TYPE.AIMING);
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

            //textext.text = Input.mousePosition.ToString(); //debug
        }
    }

    void OnMouseUp()
    {
        if (!onMobile && state == STATE_TYPE.AIMING)
        {
            switchState(STATE_TYPE.TRAVELING);
            Vector3 angle = (startPos - transform.position) * 0.03f;
            velocity = new Vector3(angle.x * 4f, dragOffset.y * screenDragConstant * 0.0020f, angle.z * 14f); //i went for just coding the physics myself instead of using rigidbodies
        }
    }

    void mobileInputController()
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
                        switchState(STATE_TYPE.AIMING);
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

                    //textext.text = touch.position.ToString();
                }
                if (touch.phase == TouchPhase.Ended)
                {
                    switchState(STATE_TYPE.TRAVELING);
                    Vector3 angle = (startPos - transform.position) * 0.03f;
                    velocity = new Vector3(angle.x * 4f, dragOffset.y * screenDragConstant * 0.0020f, angle.z * 14f);
                }
            }
        }
    }

}