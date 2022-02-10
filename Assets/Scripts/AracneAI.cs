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

    private const float MIN_bodyHeight = 1f, MAX_bodyHeight = 4f;
    [SerializeField][Range(MIN_bodyHeight, MAX_bodyHeight)] private float bodyDefaultHeight;
    [SerializeField] private float bodyCurrHeight;
    public Transform bodyDummy;

    [Header("Target settings")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float castDistance = 10f;
    [SerializeField] Vector3 castOffset;
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private float legSpeed = 20f;

    private float averageLegsHeight;
    private Vector3 averageLegsPos;
    private bool[] hasToMoveLegs;
    //private bool isBodyAnimating;
    private BoxCollider bodyBox;
    bool willBodyHit;
    RaycastHit bodyHit;
    float viewDistance = .1f;
    private float bodyTimeout;
    private Vector3 lastValidBodyPos;

    private GameObject[] legObjs;
    private Transform[] legHandles, legPoles, legTargets;

    void Start()
    {
        bodyBox = bodyTransform.GetComponent<BoxCollider>();
        bodyCurrHeight = bodyTransform.localPosition.y;
        lastValidBodyPos = bodyTransform.position + Vector3.up*bodyDefaultHeight;
        //isBodyAnimating = true;
        InitLegs();
        //bodyTransform.position += Vector3.up*bodyStartHeight;
        StartCoroutine(PlaceBodyAt(transform.position + Vector3.up*bodyDefaultHeight, 1.75f));
        
    }

    void Update()
    {
        /*if (pairOfLegs*2 != legObjs.Length)
        {
            //TODO: adding legs at run-time
            //InitLegs();
        }*/
    }

    void LateUpdate()
    {
        averageLegsPos = Vector3.zero;
        float averageLegsHeight_tmp = 0f;
        // UPDATE LEGS
        for (int i=0; i < pairOfLegs; i++)
        {
            CheckTarget(i);
            CheckHandle(i);

            CheckTarget(2*pairOfLegs-1 - i);
            CheckHandle(2*pairOfLegs-1 - i);

            averageLegsPos = (averageLegsPos + (legHandles[2*pairOfLegs-1 - i].position + legHandles[i].position)*.5f)*.5f;
            averageLegsHeight_tmp = (averageLegsHeight_tmp + (legHandles[2*pairOfLegs-1 - i].position.y + legHandles[i].position.y)*.5f)*.5f;
        }
        averageLegsHeight = averageLegsHeight_tmp;

        //if (isBodyAnimating) return;

        // UPDATE BODY
        //bodyTransform.position = averageLegPos + transform.up*bodyStartHeight;
        Vector3 bodyPos = new Vector3(bodyTransform.position.x, averageLegsHeight + bodyCurrHeight, bodyTransform.position.z);
        
        //bodyTransform.position = new Vector3(bodyTransform.position.x, averageLegsHeight + bodyStartHeight, bodyTransform.position.z);
        //Debug.DrawLine(averageLegPos + transform.up*bodyStartHeight, -transform.up*2, Color.green);
        /*
        RaycastHit hit;
        if (Physics.Raycast(bodyTransform.position+castOffset, -transform.up, out hit, castOffset.y+bodyDefaultHeight*0.3f, whatIsGround))
        {
            Debug.DrawRay(bodyTransform.position+castOffset, -transform.up * hit.distance, Color.yellow);
            Debug.Log("Body too low");
            //bodyTransform.position = hit.point + transform.up*bodyStartHeight*.3f;
            bodyPos = hit.point + transform.up*bodyDefaultHeight*.4f;
        }*/
        
        bodyTransform.position = bodyPos;
    }

    void FixedUpdate()
    {
        //CheckBody();
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
        Vector3 deltaZ = Vector3.forward * legGap*0.23f;
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
            //Debug.Log("Did Hit - downwards");
            legTargets[i].position = hit.point;
        }
        /*
        else if (Physics.Raycast(legTargets[i].position-castOffset, transform.up, out hit, castDistance, whatIsGround))
        {
            Debug.DrawRay(legTargets[i].position-castOffset, transform.up * hit.distance, Color.yellow);
            //Debug.Log("Did Hit - upwards");
            legTargets[i].position = hit.point;
        }*/
        else
        {
            Debug.DrawRay(legTargets[i].position, -transform.up * castDistance, Color.white);
            //Debug.Log("Did not Hit");
        }

    }
    void CheckHandle(int i)
    {
        float distanceFromLeg = (legTargets[i].position - legHandles[i].position).magnitude;
        //Debug.Log("distance from leg: " + distanceFromLeg);
        Debug.DrawLine(legTargets[i].position, legHandles[i].position, Color.red);

        int oppositeIndex = 2*pairOfLegs-1 - i;
        if (!hasToMoveLegs[i] && distanceFromLeg > maxDistance && !hasToMoveLegs[oppositeIndex])
        {
            hasToMoveLegs[i] = true;
        }

        if (hasToMoveLegs[i])
        {
            if (distanceFromLeg < 0.1f || distanceFromLeg > maxDistance*3)
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
        if (Mathf.Abs(bodyCurrHeight - bodyDefaultHeight) > .11f)
        {
            bodyTimeout -= Time.deltaTime;
            //Debug.Log("BodyTimeout = "+ bodyTimeout);
            if (bodyTimeout <= 0)
            {
                //try to put the body back in position
                Vector3 dir = bodyDefaultHeight*Vector3.up
                                - bodyTransform.localPosition;
                Debug.DrawRay(bodyBox.bounds.center, dir, Color.yellow, 2f);
                willBodyHit = Physics.BoxCast(bodyBox.bounds.center, bodyBox.size*.5f, dir.normalized, out bodyHit, transform.rotation, dir.magnitude, whatIsGround);
                if (!willBodyHit)
                {
                    bodyCurrHeight = bodyDefaultHeight;
                    Debug.Log("Body: back in position!");
                }
                else
                {
                    bodyTimeout += Time.deltaTime;
                    Debug.Log("Body: waiting for space...");
                }
            }
        }
        //cast the body collider forwards
        willBodyHit = Physics.BoxCast(bodyBox.bounds.center, bodyBox.size*.5f, transform.forward, out bodyHit, transform.rotation, viewDistance, whatIsGround);
        //willBodyHit = Physics.BoxCast(transform.position+transform.up*(bodyDefaultHeight-averageLegsY), bodyBox.size*.5f, transform.forward, out bodyHit, transform.rotation, viewDistance, whatIsGround);
        if (!willBodyHit)
        {
            // No collisions
            Debug.DrawLine(bodyBox.bounds.center, bodyBox.bounds.center + transform.forward*viewDistance, Color.blue);
            Debug.Log("no collisions...");
            //bodyCurrHeight = bodyDefaultHeight;
            bodyDummy.position = bodyBox.bounds.center + transform.forward*viewDistance;
            return;
        }
        // If here, no free space in front of the body
        Debug.Log("Hit : " + bodyHit.collider.name);

        float step = .25f;
        float targetHeight = bodyCurrHeight - averageLegsHeight;
        bool willCollide;

        // LET'S CHECK BELOW the current body position for free space
        for (int i=1; targetHeight > MIN_bodyHeight; i++)
        {
            targetHeight = targetHeight - i*step;
            willCollide = Physics.BoxCast(bodyTransform.position-transform.up*i*step, bodyBox.size*.5f, transform.forward, out bodyHit, transform.rotation, viewDistance, whatIsGround);
            if (!willCollide)
            {
                // There is free space for the body at this position:
                // we can set the body height accordingly and return
                Debug.Log("steps = "+ i*step + "; Y = "+ targetHeight);
                //Vector3 targetPos = bodyBox.bounds.center + transform.forward*viewDistance - transform.up*steps;
                Debug.DrawLine(bodyTransform.position, bodyTransform.position-transform.up*i*step, Color.green);
                bodyCurrHeight = targetHeight;
                bodyDummy.localPosition = transform.up*targetHeight + transform.forward*viewDistance;
                bodyTimeout = 3f;
                return; //break;
            }
        }
        // If here, no free space underneath the body: 
        // LET'S CHECK ABOVE the current body position for free space
        targetHeight = bodyCurrHeight - averageLegsHeight;
        for (int i=1; targetHeight < MAX_bodyHeight; i++)
        {
            targetHeight = targetHeight + i*step;
            willCollide = Physics.BoxCast(bodyTransform.position+transform.up*i*step, bodyBox.size*.5f, transform.forward, out bodyHit, transform.rotation, viewDistance, whatIsGround);
            if (!willCollide)
            {
                // There is free space for the body at this position:
                // we can set the body height accordingly and return
                Debug.Log("steps = "+ i*step + "; Y = "+ targetHeight);
                Debug.DrawLine(bodyTransform.position, bodyTransform.position+transform.up*i*step, Color.green);
                bodyCurrHeight = targetHeight;
                bodyDummy.localPosition = transform.up*targetHeight + transform.forward*viewDistance;
                bodyTimeout = 3f;
                return;
            }
        }
        // If here, no free space neither above nor below
        Debug.Log("Warning: unavoidable obstacle ahead!");
    }

    private void UpdateBody() // should be executed after updating legs
    {
        //TODO: maybe try with defaultHeight instead of currHeight
        Vector3 candidateNextPos = averageLegsPos + bodyCurrHeight*transform.up;
        Vector3 diff = (candidateNextPos - lastValidBodyPos);
        //try to move the body just in the next position, without affecting the body height
        willBodyHit = Physics.BoxCast(lastValidBodyPos, bodyBox.size*.5f, diff.normalized, out bodyHit, transform.rotation, diff.magnitude, whatIsGround);

        if (!willBodyHit)
        {
            // ok, we can move the body ahead and update the positions
            
            lastValidBodyPos = candidateNextPos;

        }
        else // check for free space and offset the body accordingly
        {
            // try below
            // if no free space, try above
        }
    }
    IEnumerator PlaceBodyAt(Vector3 targetPos, float animSeconds)
    {
        //Vector3 startPos = bodyTransform.position;
        float startY = bodyCurrHeight;
        float t = 0f;
        while (t <= animSeconds)
        {
            t = t + Time.deltaTime;
            float percent = Mathf.Clamp01(t / animSeconds);
            
            //bodyTransform.position = Vector3.Lerp(startPos, targetPos, percent);
            bodyCurrHeight = Mathf.Lerp(startY, targetPos.y, percent);
            yield return null;
        }
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
        Gizmos.DrawWireCube(transform.position + Vector3.up*bodyDefaultHeight, new Vector3(bodyWidth*2, .5f, bodyLength*2));

        //body casts
        Gizmos.DrawRay(bodyTransform.position, transform.forward * viewDistance);
        if (bodyBox)
            Gizmos.DrawWireCube(bodyTransform.position + transform.forward * viewDistance, bodyBox.size);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position+transform.up*(bodyDefaultHeight-averageLegsHeight), .25f);
    }
}
