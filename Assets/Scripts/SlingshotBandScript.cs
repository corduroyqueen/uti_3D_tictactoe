using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingshotBandScript : MonoBehaviour
{
    /// This script makes the two slingshot bands point at the sling
    /// (bands are the ropes and sling is the holder for the balloon)
    /// the targetPivot gameObjects are children of the slingshot sling model

    public GameObject targetPivot;

    void Update()
    {
        transform.LookAt(targetPivot.transform);
        transform.localScale = new Vector3(1f, 1f, Vector3.Distance(transform.position, targetPivot.transform.position) / 2f);
    }
}
