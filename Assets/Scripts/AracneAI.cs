using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AracneAI : MonoBehaviour
{
    [SerializeField][Range(1,6)] private int pairOfLegs;
    [SerializeField] private GameObject legPrefab, handlePrefab, polePrefab, targetPrefab;
    private FastIK[] leafBones;

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
    private Coroutine bodyAdjustHeight;

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
        StartCoroutine(PlaceBodyAt(bodyDefaultHeight, 1.75f));
        
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
        //averageLegsPos = Vector3.zero;
        averageLegsHeight = 0f;
        // UPDATE LEGS
        for (int i=0; i < pairOfLegs; i++)
        {
            CheckTarget(i);
            CheckHandle(i);
            leafBones[i].SolveIK();

            int j = 2*pairOfLegs-1 - i;
            CheckTarget(j);
            CheckHandle(j);
            leafBones[j].SolveIK();

            //averageLegsPos = (averageLegsPos + (legHandles[j].position + legHandles[i].position)*.5f)*.5f;
            averageLegsHeight = (averageLegsHeight + (legHandles[j].position.y + legHandles[i].position.y)*.5f)*.5f;
        }
        
        //if (isBodyAnimating) return;

        // UPDATE BODY
        float nextBodyY = UpdateBody();
        bodyDummy.position = lastValidBodyPos;
        if (Mathf.Abs(nextBodyY - bodyCurrHeight) > .05f)
        {
            if(bodyAdjustHeight!=null) StopCoroutine(bodyAdjustHeight);
            bodyAdjustHeight = StartCoroutine(PlaceBodyAt(nextBodyY, .2f));
        }
        //bodyCurrHeight = nextBodyY;

        //Vector3 bodyPos = new Vector3(transform.position.x, transform.position.y + averageLegsHeight + bodyCurrHeight, transform.position.z);
        Vector3 bodyPos = transform.position + (averageLegsHeight + bodyCurrHeight)*transform.up;
        
        bodyTransform.position = bodyPos;
    }

    private void InitLegs()
    {
        hasToMoveLegs = new bool[pairOfLegs*2];
        legObjs = new GameObject[pairOfLegs*2];
        legHandles = new Transform[pairOfLegs*2];
        legPoles = new Transform[pairOfLegs*2];
        legTargets = new Transform[pairOfLegs*2];
        leafBones = new FastIK[pairOfLegs*2];

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
            
            //FastIK leafBone = legObjs[i].GetComponentInChildren<FastIK>();
            //leafBone.target = legHandles[i];
            //leafBone.pole = legPoles[i];
            leafBones[i] = legObjs[i].GetComponentInChildren<FastIK>();
            leafBones[i].target = legHandles[i];
            leafBones[i].pole = legPoles[i];
            leafBones[i].Initialize();

            //LEFT side
            int j = i+pairOfLegs;
            legObjs[j].transform.Translate(new Vector3(-bodyWidth,0,pos.z));
            legHandles[j].position = legObjs[j].transform.position + new Vector3(-handleDistance,0,0);
            legPoles[j].position = legHandles[j].position + new Vector3(-poleDelta.x, poleDelta.y, poleDelta.z);
            legTargets[j].position = legHandles[j].position;
            legTargets[j].GetComponent<NextPositionTarget>().legTarget = legHandles[j];
            /*leafBone = legObjs[j].GetComponentInChildren<FastIK>();
            leafBone.target = legHandles[j];
            leafBone.pole = legPoles[j];*/
            leafBones[j] = legObjs[j].GetComponentInChildren<FastIK>();
            leafBones[j].target = legHandles[j];
            leafBones[j].pole = legPoles[j];
            leafBones[j].Initialize();
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
        if (!hasToMoveLegs[i])
        {
            // check if grounded
            RaycastHit hit;
            if (Physics.Raycast(legHandles[i].position+castOffset, -transform.up, out hit, castDistance, whatIsGround))
            {
                //Debug.DrawRay(legHandles[i].position+castOffset, -transform.up * hit.distance, Color.cyan);
                //Debug.Log("Did Hit - downwards");
                legHandles[i].position = hit.point;
            }
        }
        
        // check the distance from target
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

    private float UpdateBody() // should be executed after updating legs
    {
        //TODO: maybe try with defaultHeight instead of currHeight
        //Vector3 candidateNextPos = averageLegsPos + bodyDefaultHeight*transform.up;
        Vector3 candidateNextPos = transform.position + (averageLegsHeight + bodyDefaultHeight)*transform.up;
        //Vector3 diff = (candidateNextPos - lastValidBodyPos);
        
        // try to move the body just in the next position, without affecting the body height
        //willBodyHit = Physics.BoxCast(lastValidBodyPos, bodyBox.size*.5f, diff.normalized, out bodyHit, transform.rotation, viewDistance, whatIsGround);
        willBodyHit = Physics.CheckBox(candidateNextPos, bodyBox.size*.5f, transform.rotation, whatIsGround);

        if (!willBodyHit)
        {
            // ok, we can move the body ahead and update the positions
            lastValidBodyPos = candidateNextPos;
            return bodyCurrHeight;
        }
        else // check for free space and offset the body accordingly
        {
            Debug.Log("collision detected!");
            float step = 0.15f; //the deltaY for each box-check
            
            // try BELOW            
            candidateNextPos = transform.position + (averageLegsHeight+bodyDefaultHeight - step)*transform.up;
            //candidateNextPos = averageLegsPos + (bodyCurrHeight - step)*transform.up;

            while(candidateNextPos.y >= averageLegsHeight + MIN_bodyHeight)
            {
                willBodyHit = Physics.CheckBox(candidateNextPos, bodyBox.size*.5f, transform.rotation, whatIsGround);
                if(!willBodyHit)
                {
                    lastValidBodyPos = candidateNextPos;
                    //bodyCurrHeight = candidateNextPos.y - averageLegsPos.y;
                    //return;//break;
                    return candidateNextPos.y - averageLegsHeight;
                }
                candidateNextPos -= transform.up*step;
            }
            
            // no free space below, try ABOVE
            candidateNextPos = transform.position + (averageLegsHeight+bodyDefaultHeight + step)*transform.up;
            //candidateNextPos = averageLegsPos + (bodyCurrHeight + step)*transform.up;
            while(candidateNextPos.y <= averageLegsHeight + MAX_bodyHeight)
            {
                willBodyHit = Physics.CheckBox(candidateNextPos, bodyBox.size*.5f, transform.rotation, whatIsGround);
                if(!willBodyHit)
                {
                    lastValidBodyPos = candidateNextPos;
                    //bodyCurrHeight = candidateNextPos.y - averageLegsHeight;
                    //return;//break;
                    return candidateNextPos.y - averageLegsHeight;
                }
                candidateNextPos += transform.up*step;
            }
            // if here, no free space available
            Debug.Log("Warning: unable to avoid the obstacle");
            return bodyCurrHeight;
        }
    }
    IEnumerator PlaceBodyAt(float targetHeight, float animSeconds)
    {
        //Vector3 startPos = bodyTransform.position;
        float startY = bodyCurrHeight;
        float t = 0f;
        while (t <= animSeconds)
        {
            t = t + Time.deltaTime;
            float percent = Mathf.Clamp01(t / animSeconds);
            
            //bodyTransform.position = Vector3.Lerp(startPos, targetPos, percent);
            bodyCurrHeight = Mathf.Lerp(startY, targetHeight, percent);
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

    /*private void CheckBody()
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
    }*/
}
