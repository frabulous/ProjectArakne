using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwap : MonoBehaviour
{
    public Transform[] cameraAngles;
    private int curr;

    public void SwapAngle()
    {
        if (cameraAngles.Length>0)
        {
            curr = (curr+1)%cameraAngles.Length;

            transform.SetPositionAndRotation(cameraAngles[curr].position, cameraAngles[curr].rotation);
        }

    }
}
