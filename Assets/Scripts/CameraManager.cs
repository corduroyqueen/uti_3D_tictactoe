using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    /// Script that runs the camera movement that happens at the start of the game
    public Vector3 menuPosition, menuRotation, gameplayPosition, gameplayRotation;//, winnerPosition, winnerRotation;
    private Vector3 velocity = Vector3.zero;
    bool startingGame = false;

    void Update()
    {
        if (startingGame)
        {
            DampCameraPosition(gameplayPosition, 1f, gameplayRotation, 1f);
        }
    }

    /// This function moves the camera
    void DampCameraPosition(Vector3 targetPosition, float positionSpeed, Vector3 targetRotation, float rotationSpeed)
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionSpeed);
        float angle = Mathf.LerpAngle(transform.eulerAngles.x, targetRotation.x, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(angle, 0, 0);

        if (Vector3.Distance(transform.position,targetPosition)<0.1f)
        {
            startingGame = false;
        }

        
    }

    /// This function starts the movement, which is activated from the start button on the canvas
    public void StartGameFromMenu()
    {
        startingGame = true;
    }
}
