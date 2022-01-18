using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextPositionTarget : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float castDistance = 10f;

    public Vector3 offset;

    public Transform legTarget;
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private float legSpeed = 20f;

    private bool hasToMoveLeg;

    void Awake()
    {
    }

    void LateUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position+offset, -transform.parent.up, out hit, castDistance, whatIsGround))
        {
            Debug.DrawRay(transform.position+offset, -transform.parent.up * hit.distance, Color.yellow);
            Debug.Log("Did Hit - downwards");
            transform.position = hit.point;
        }
        else if (Physics.Raycast(transform.position-offset, transform.parent.up, out hit, castDistance, whatIsGround))
        {
            Debug.DrawRay(transform.position-offset, transform.parent.up * hit.distance, Color.yellow);
            Debug.Log("Did Hit - upwards");
            transform.position = hit.point;
        }
        else
        {
            Debug.DrawRay(transform.position, -transform.parent.up * 1000, Color.white);
            Debug.Log("Did not Hit");
        }

        float distanceFromLeg = (transform.position - legTarget.position).magnitude;
//        Debug.Log("distance from leg: " + distanceFromLeg);
        Debug.DrawLine(transform.position, legTarget.position, Color.red);

        if (!hasToMoveLeg && distanceFromLeg > maxDistance)
        {
            //legTarget.position = transform.position;
            //legTarget.position = Vector3.MoveTowards(legTarget.position, transform.position, Time.deltaTime*legSpeed);
            hasToMoveLeg = true;
        }

        if (hasToMoveLeg)
        {
            if (distanceFromLeg < 0.01f)
            {
                legTarget.position = transform.position;
                hasToMoveLeg = false;
            }
            else if (distanceFromLeg > maxDistance*2f)
            {
                legTarget.position = Vector3.MoveTowards(legTarget.position, transform.position, Time.deltaTime*legSpeed*3f);
            }
            else if (distanceFromLeg > maxDistance*0.6f) //first moving the leg up, before aiming to target directly
            {
                legTarget.position = Vector3.MoveTowards(legTarget.position,
                        legTarget.position+.5f*(transform.position-legTarget.position) + transform.parent.up*.4f,
                        Time.deltaTime*legSpeed
                        );
            }
            else
            {
                legTarget.position = Vector3.MoveTowards(legTarget.position, transform.position, Time.deltaTime*legSpeed);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position+offset, .1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position-offset, .1f);
    }
}
