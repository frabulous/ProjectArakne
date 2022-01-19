using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AracneAI : MonoBehaviour
{
    [SerializeField][Range(1,6)] private int pairOfLegs;
    [SerializeField] private GameObject legPrefab, handlePrefab, polePrefab, targetPrefab;

    [SerializeField] private Transform bodyTransform, legsTransform;
    [SerializeField] private float bodyWidth, bodyLength;
    [SerializeField] private float handleDistance;
    [SerializeField] private Vector3 poleDelta;
    [SerializeField] private float bodyStartHeight;

    [Header("Target settings")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float castDistance = 10f;
    [SerializeField] Vector3 castOffset;
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private float legSpeed = 20f;

    private bool[] hasToMoveLegs;


    private GameObject[] legObjs;
    private Transform[] legHandles, legPoles, legTargets;

    void Start()
    {
        InitLegs();
        bodyTransform.position += Vector3.up*bodyStartHeight;
    }

    void Update()
    {
        /*if (pairOfLegs*2 != legObjs.Length)
        {
            //TODO
            //InitLegs();
        }*/
    }

    void LateUpdate()
    {
        //Vector3 averageLegPos = Vector3.zero;
        float averageLegsHeight = 0f;
        // UPDATE LEGS
        for (int i=0; i < pairOfLegs; i++)
        {
            CheckTarget(i);
            CheckHandle(i);

            CheckTarget(2*pairOfLegs-1 - i);
            CheckHandle(2*pairOfLegs-1 - i);

            //averageLegPos = (averageLegPos + (legHandles[2*pairOfLegs-1 - i].position + legHandles[i].position)*.5f)*.5f;
            averageLegsHeight = (averageLegsHeight + (legHandles[2*pairOfLegs-1 - i].position.y + legHandles[i].position.y)*.5f)*.5f;
        }

        // UPDATE BODY
        //bodyTransform.position = averageLegPos + transform.up*bodyStartHeight;
        Vector3 bodyPos = new Vector3(bodyTransform.position.x, averageLegsHeight + bodyStartHeight, bodyTransform.position.z);
        //bodyTransform.position = new Vector3(bodyTransform.position.x, averageLegsHeight + bodyStartHeight, bodyTransform.position.z);
        //Debug.DrawLine(averageLegPos + transform.up*bodyStartHeight, -transform.up*2, Color.green);
        /*
        RaycastHit hit;
        if (Physics.Raycast(bodyTransform.position+castOffset, -transform.up, out hit, castOffset.y+bodyStartHeight*0.3f, whatIsGround))
        {
            Debug.DrawRay(bodyTransform.position+castOffset, -transform.up * hit.distance, Color.yellow);
            Debug.Log("Body too low");
            //bodyTransform.position = hit.point + transform.up*bodyStartHeight*.3f;
            bodyPos = hit.point + transform.up*bodyStartHeight*.4f;
        }
        */
        bodyTransform.position = bodyPos;
    }

    private void InitLegs()
    {
        hasToMoveLegs = new bool[pairOfLegs*2];
        legObjs = new GameObject[pairOfLegs*2];
        legHandles = new Transform[pairOfLegs*2];
        legPoles = new Transform[pairOfLegs*2];
        legTargets = new Transform[pairOfLegs*2];

        for (var i=0; i < 2*pairOfLegs; i++)
        {
            legObjs[i] = Instantiate(legPrefab, legsTransform);
            legHandles[i] = Instantiate(handlePrefab).transform;
            legPoles[i] = Instantiate(polePrefab, legHandles[i]).transform;
            legTargets[i] = Instantiate(targetPrefab, this.transform).transform;

            legObjs[i].name += " N" + i;
            legHandles[i].name += " N" + i;
            legPoles[i].name += " N" + i;
            legTargets[i].name += " N" + i;
        }
        
        float legGap = 0f;
        Vector3 pos = new Vector3(bodyWidth,0,0);
        if (pairOfLegs>1)
        {
            legGap = bodyLength*2f/(float)(pairOfLegs-1);
            pos = new Vector3(bodyWidth,0,-bodyLength-legGap);
        }
        
        for (var i=0; i < pairOfLegs; i++)
        {
            pos = pos + new Vector3(0,0,legGap);
            Debug.Log("pos: "+ pos.ToString());
            //RIGHT side
            legObjs[i].transform.Translate(pos, Space.Self);
            legHandles[i].position = legObjs[i].transform.position + new Vector3(handleDistance,0,0);
            //legHandles[i].position = (legObjs[i].transform.position - bodyTransform.position).normalized * handleDistance + bodyTransform.position;
            legPoles[i].position = legHandles[i].position + poleDelta;
            legTargets[i].position = legHandles[i].position;
            
            legTargets[i].GetComponent<NextPositionTarget>().legTarget = legHandles[i];
            
            FastIK leafBone = legObjs[i].GetComponentInChildren<FastIK>();
            leafBone.target = legHandles[i];
            leafBone.pole = legPoles[i];

            //LEFT side
            int j = i+pairOfLegs;
            legObjs[j].transform.Translate(new Vector3(-bodyWidth,0,pos.z));
            legHandles[j].position = legObjs[j].transform.position + new Vector3(-handleDistance,0,0);
            legPoles[j].position = legHandles[j].position + new Vector3(-poleDelta.x, poleDelta.y, poleDelta.z);
            legTargets[j].position = legHandles[j].position;
            legTargets[j].GetComponent<NextPositionTarget>().legTarget = legHandles[j];
            leafBone = legObjs[j].GetComponentInChildren<FastIK>();
            leafBone.target = legHandles[j];
            leafBone.pole = legPoles[j];
        }

        // Put the legs in a zigzag pattern
        Vector3 deltaZ = Vector3.forward * maxDistance*0.33f;
        for (var i = 0; i < pairOfLegs; i++)
        {
            legHandles[i].Translate(deltaZ, Space.Self);
            legPoles[i].Translate(deltaZ*0.5f, Space.Self);
            legHandles[2*pairOfLegs-1 - i].Translate(deltaZ, Space.Self);
            legPoles[2*pairOfLegs-1 - i].Translate(deltaZ*0.5f, Space.Self);

            deltaZ *= -1;
        }
    }

    void CheckTarget(int i)
    {
        RaycastHit hit;
        if (Physics.Raycast(legTargets[i].position+castOffset, -transform.up, out hit, castDistance, whatIsGround))
        {
            Debug.DrawRay(legTargets[i].position+castOffset, -transform.up * hit.distance, Color.yellow);
            Debug.Log("Did Hit - downwards");
            legTargets[i].position = hit.point;
        }
        else if (Physics.Raycast(legTargets[i].position-castOffset, transform.up, out hit, castDistance, whatIsGround))
        {
            Debug.DrawRay(legTargets[i].position-castOffset, transform.up * hit.distance, Color.yellow);
            Debug.Log("Did Hit - upwards");
            legTargets[i].position = hit.point;
        }
        else
        {
            Debug.DrawRay(legTargets[i].position, -transform.up * castDistance, Color.white);
            Debug.Log("Did not Hit");
        }

    }
    void CheckHandle(int i)
    {
        float distanceFromLeg = (legTargets[i].position - legHandles[i].position).magnitude;
//        Debug.Log("distance from leg: " + distanceFromLeg);
        Debug.DrawLine(legTargets[i].position, legHandles[i].position, Color.red);

        int oppositeIndex = 2*pairOfLegs-1 - i;
        if (!hasToMoveLegs[i] && distanceFromLeg > maxDistance && !hasToMoveLegs[oppositeIndex])
        {
            hasToMoveLegs[i] = true;
        }

        if (hasToMoveLegs[i])
        {
            if (distanceFromLeg < 0.01f)
            {
                legHandles[i].position = legTargets[i].position;
                hasToMoveLegs[i] = false;
            }
            else if (distanceFromLeg > maxDistance*2f)
            {
                legHandles[i].position = Vector3.MoveTowards(legHandles[i].position, legTargets[i].position, Time.deltaTime*legSpeed*3f);
            }
            else if (distanceFromLeg > maxDistance*0.6f) //first moving the leg up, before aiming to target directly
            {
                legHandles[i].position = Vector3.MoveTowards(legHandles[i].position,
                        legHandles[i].position+.5f*(legTargets[i].position-legHandles[i].position) + transform.up*.4f,
                        Time.deltaTime*legSpeed
                        );
            }
            else
            {
                legHandles[i].position = Vector3.MoveTowards(legHandles[i].position, legTargets[i].position, Time.deltaTime*legSpeed);
            }
        }
    }

    private void CheckBody()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        float legGap = bodyLength;
        if (pairOfLegs>1)
        {
            legGap = bodyLength*2f/(float)(pairOfLegs-1);
        }
        Vector3 pos;

        for (int i=0; i < pairOfLegs; i++)
        {
            pos = bodyTransform.position + new Vector3(bodyWidth,0,i*legGap - bodyLength);
            if (pairOfLegs<2) pos = bodyTransform.position + new Vector3(bodyWidth,0,0);

            Gizmos.DrawWireSphere(pos, .1f); // right root
            Gizmos.DrawWireSphere(pos - new Vector3(2*bodyWidth,0,0), .1f); // left root
            Gizmos.DrawWireCube(pos = pos + new Vector3(handleDistance,0,0), Vector3.one*.2f); // right handle
            Gizmos.DrawWireCube(pos - new Vector3(2*(bodyWidth+handleDistance),0,0), Vector3.one*.2f); // left handle
            //Gizmos.DrawWireCube(pos = (pos - bodyTransform.position).normalized*handleDistance + bodyTransform.position, Vector3.one*.2f); // right handle
            //Gizmos.DrawWireCube((new Vector3(-pos.x, pos.y,pos.z)- bodyTransform.position).normalized*handleDistance, Vector3.one*.2f); // left handle
            Gizmos.DrawWireSphere(pos = pos + poleDelta, .2f); // left pole
            Gizmos.DrawWireSphere(pos - new Vector3(2*(bodyWidth+handleDistance+poleDelta.x), 0,0), .2f); // right pole
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position + Vector3.up*bodyStartHeight, new Vector3(bodyWidth*2, .5f, bodyLength*2));
    }
}
