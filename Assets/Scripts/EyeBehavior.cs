using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeBehavior : MonoBehaviour
{
    public Transform lookAtTarget;
    public Transform colliderCenter;
    public float colliderSize = 0.5f;
    public LayerMask whatCollides;

    private Vector3 lookAtPosition;

    void Start() 
    {
        if (!colliderCenter)
            colliderCenter = transform;
    }
    void Update()
    {
        //Collider[] obstacles = Physics.OverlapSphere(colliderCenter.position, colliderSize, whatCollides);
        //if (obstacles.Length>0)
        //{
        //    lookAtPosition = obstacles[0].transform.position;
        //}
        //else 
        if (lookAtTarget)
            lookAtPosition = lookAtTarget.position;
        else
            lookAtPosition = transform.position + transform.forward;

        transform.LookAt(lookAtPosition);
    }

    /*private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.cyan;
        if(colliderCenter)
            Gizmos.DrawWireSphere(colliderCenter.position, colliderSize);
        else
            Gizmos.DrawWireSphere(transform.position, colliderSize);
    }*/
}
