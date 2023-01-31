using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArakneAI : MonoBehaviour
{
    public Transform bodyDummy;
    [SerializeField] private GameObject legPrefab, handlePrefab, polePrefab, targetPrefab;
    private FabrIK[] leafBones;

    [SerializeField] private Transform bodyTransform, legsTransform;
    
    private const float MIN_bodyHeight = 1f, MAX_bodyHeight = 4f;
    
    [Space(10)]
    [Header("- SETUP -")]
    [Space(10)]
    [SerializeField][Range(MIN_bodyHeight, MAX_bodyHeight)] private float bodyDefaultHeight;

    [SerializeField][Range(0.1f,2.0f)] private float bodyWidth =.75f, bodyLength=.9f;
    /// <summary>
    ///  It determins how far the legs are from the body
    /// </summary>
    [Tooltip("It determins how far the legs are from the body")]
    [SerializeField] private float handleDistance;
    /// <summary>
    /// The offset of the leg IK pole vector
    /// </summary>
    [Tooltip("The offset of the leg IK pole vector")]
    [SerializeField] private Vector3 poleDelta;

    [Space(10)]
    [Header("- RUN-TIME settings -")]
    [Space(10)]

    [Space(10)]
    /// <summary>
    ///  Half the number of legs
    /// </summary>
    [Tooltip("Half the number of legs")]
    [SerializeField][Range(1,6)] private int pairsOfLegs;    
    [Space(10)]

    [SerializeField][Range(MIN_bodyHeight, MAX_bodyHeight)] private float bodyCurrHeight;
    private float castDistance = 10f;
    /// <summary>
    ///  Max altitude difference allowed
    /// </summary>
    [Tooltip("Max altitude difference allowed")]
    [SerializeField] float maxStepHeight = 4f;
    Vector3 castOffset;
    /// <summary>
    ///  Limit distance from handle to target before triggering the step animation
    /// </summary>
    [Tooltip("Limit distance from handle to target before triggering the step animation")]
    [SerializeField][Range(1.2f, 1.9f)] private float stepGap = 1.3f;
    /// <summary>
    ///  The step animation speed
    /// </summary>
    [Tooltip("The step animation speed")]
    [SerializeField] private float legSpeed = 20f;
    [SerializeField] private LayerMask whatIsGround;

    private float averageLegsHeight;
    private Vector3 averageLegsPos;
    private bool[] hasToMoveLegs;

    private BoxCollider bodyBox;
    bool willBodyHit;
    RaycastHit bodyHit;
    float viewDistance = .1f;
    private float bodyTimeout;
    private Vector3 lastValidBodyPos;
    private Coroutine bodyAdjustHeight;
    private bool bodyAdjustCRisRunning;

    private GameObject[] legObjs;
    private Transform[] legHandles, legPoles, legTargets;
    private Transform handlesContainer;

    void Start()
    {
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);

        bodyBox = bodyTransform.GetComponent<BoxCollider>();
        bodyCurrHeight = bodyTransform.localPosition.y;
        lastValidBodyPos = bodyTransform.position + Vector3.up*bodyDefaultHeight;
        //isBodyAnimating = true;
        InitLegs();
        //bodyTransform.position += Vector3.up*bodyStartHeight;
        StartCoroutine(PlaceBodyAt(bodyDefaultHeight, 1.75f));
        
        bodyTransform.GetChild(0).localScale = new Vector3(bodyLength*111.11f, bodyWidth*133.34f, 80f);
        bodyBox.size = new Vector3(bodyWidth*2.2f, 1f, bodyLength*3.2f);
    }
    void Update()
    {
        if (pairsOfLegs*2 != legObjs.Length)
        {
            //Changing legs amount at run-time
            InitLegs();
        }
    }

    void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);

        //averageLegsPos = Vector3.zero;
        averageLegsHeight = 0f;
        //int count = 0;
        // UPDATE LEGS
        for (int i=0; i < pairsOfLegs; i++)
        {
            CheckLegTarget(i);
            CheckLegHandle(i);
            leafBones[i].SolveIK();

            int j = 2*pairsOfLegs-1 - i;
            CheckLegTarget(j);
            CheckLegHandle(j);
            leafBones[j].SolveIK();
            
            /*if(!hasToMoveLegs[i]) {
                averageLegsHeight += legHandles[i].position.y;
                count++;
            }
            if(!hasToMoveLegs[j]) {
                averageLegsHeight += legHandles[j].position.y;
                count++;
            }*/

            averageLegsHeight += (legHandles[j].position.y + legHandles[i].position.y)*.5f;
        }
        averageLegsHeight /= (float)pairsOfLegs;
        //if(count!=0) averageLegsHeight /= (float)count;

        // UPDATE BODY
        float nextBodyY = UpdateBody();
        
        bodyDummy.position = lastValidBodyPos;

        float bodyHeightDelta = Mathf.Abs(nextBodyY - bodyCurrHeight);
        if (bodyHeightDelta > .05f)
        {
            //if(bodyAdjustHeight!=null) StopCoroutine(bodyAdjustHeight);
            if (!bodyAdjustCRisRunning)
                bodyAdjustHeight = StartCoroutine(PlaceBodyAt(nextBodyY, .4f));
        }
        //bodyCurrHeight = nextBodyY;

        //Vector3 bodyPos = new Vector3(transform.position.x, transform.position.y + averageLegsHeight + bodyCurrHeight, transform.position.z);

        Vector3 bodyPos = transform.position + (averageLegsHeight + bodyCurrHeight)*transform.up;
        bodyTransform.position = bodyPos;
    }

    private void InitLegs()
    {
        if (legsTransform)
        {
            for (int i=0; i<legsTransform.childCount; i++)
            {
                Destroy(legsTransform.GetChild(i).gameObject);
            }
        }
        if (legObjs != null)
            for (int i=0; i < legObjs.Length; i++)
            {
                if(legObjs[i]) Destroy(legObjs[i]);
                //if(legPoles[i]) Destroy(legPoles[i].gameObject);
            }
        
        if (handlesContainer!=null)
            Destroy(handlesContainer.gameObject);
        if (legTargets!=null)
            Destroy(legTargets[0].parent.gameObject);
        if (legPoles!=null)
            Destroy(legPoles[0].parent.gameObject);
        

        hasToMoveLegs = new bool[pairsOfLegs*2];
        legObjs = new GameObject[pairsOfLegs*2];
        legHandles = new Transform[pairsOfLegs*2];
        legPoles = new Transform[pairsOfLegs*2];
        legTargets = new Transform[pairsOfLegs*2];
        leafBones = new FabrIK[pairsOfLegs*2];

        handlesContainer = new GameObject("LEG HANDLES").transform;
        handlesContainer.SetPositionAndRotation(transform.position, transform.rotation);
        
        Transform handlesTargetsContainer = new GameObject("LEG TARGETS").transform;
        handlesTargetsContainer.SetParent(this.transform, false);

        Transform polesContainer = new GameObject("LEG POLES").transform;
        polesContainer.SetParent(bodyTransform, false);

        for (var i=0; i < 2*pairsOfLegs; i++)
        {
            legObjs[i] = Instantiate(legPrefab, legsTransform);
            legHandles[i] = Instantiate(handlePrefab, handlesContainer).transform;
            legPoles[i] = Instantiate(polePrefab, polesContainer).transform;
            legTargets[i] = Instantiate(targetPrefab, handlesTargetsContainer).transform;

            legObjs[i].name += " n" + i;
            legHandles[i].name += " n" + i;
            legPoles[i].name += " n" + i;
            legTargets[i].name += " n" + i;
        }
        
        float legGap = 0f;
        Vector3 pos = new Vector3(bodyWidth,0,0);
        Vector3 dir = transform.right;
        float poleDeltaH = Mathf.Sqrt(poleDelta.x*poleDelta.x + poleDelta.z*poleDelta.z);

        if (pairsOfLegs>1)
        {
            legGap = bodyLength*2f/(float)(pairsOfLegs-1);
            pos = new Vector3(bodyWidth,0,-bodyLength-legGap);
        }
        
        for (var i=0; i < pairsOfLegs; i++)
        {
            pos = pos + new Vector3(0,0,legGap);
            dir = pos.normalized;
            //Debug.Log("pos: "+ pos.ToString());

            //RIGHT side
            legObjs[i].transform.Translate(pos, Space.Self);
            legHandles[i].position = legObjs[i].transform.position + transform.TransformDirection(dir)*handleDistance; // new Vector3(handleDistance,0,0);
            //legHandles[i].position = (legObjs[i].transform.position - bodyTransform.position).normalized * handleDistance + bodyTransform.position;
            legPoles[i].position = legHandles[i].position + poleDelta.y*transform.up + poleDeltaH*transform.TransformDirection(dir);
            legTargets[i].position = legHandles[i].position;
            
            legTargets[i].GetComponent<NextPositionTarget>().legTarget = legHandles[i];
            
            leafBones[i] = legObjs[i].GetComponentInChildren<FabrIK>();
            leafBones[i].target = legHandles[i];
            leafBones[i].pole = legPoles[i];
            leafBones[i].Initialize();

            //LEFT side
            int j = i+pairsOfLegs;
            dir.Set(-dir.x, dir.y, dir.z);

            legObjs[j].transform.Translate(new Vector3(-bodyWidth,0,pos.z), Space.Self);
            legHandles[j].position = legObjs[j].transform.position + transform.TransformDirection(dir)*handleDistance ; // new Vector3(-handleDistance,0,0);
            legPoles[j].position = legHandles[j].position + poleDelta.y*transform.up + poleDeltaH*transform.TransformDirection(dir);
            legTargets[j].position = legHandles[j].position;
            legTargets[j].GetComponent<NextPositionTarget>().legTarget = legHandles[j];
            /*leafBone = legObjs[j].GetComponentInChildren<FastIK>();
            leafBone.target = legHandles[j];
            leafBone.pole = legPoles[j];*/
            leafBones[j] = legObjs[j].GetComponentInChildren<FabrIK>();
            leafBones[j].target = legHandles[j];
            leafBones[j].pole = legPoles[j];
            leafBones[j].Initialize();
        }

        // Put the legs in a zigzag pattern
        Vector3 deltaZ = Vector3.forward * stepGap*0.4f;
        for (var i = 0; i < pairsOfLegs; i++)
        {
            legTargets[i].Translate(deltaZ, Space.Self);

            deltaZ *= -1;
            //int oppositeIndex = 2*pairsOfLegs-1 - i; //diagonally opposite
            int oppositeIndex = i < pairsOfLegs ? i+pairsOfLegs : i-pairsOfLegs;
            
            legTargets[oppositeIndex].Translate(deltaZ, Space.Self);

            //deltaZ *= -1;
        }
    }
    void CheckLegTarget(int i)
    {
        castOffset = new Vector3(0f, maxStepHeight, 0f);
        RaycastHit hit;
        if (Physics.Raycast(legTargets[i].position+castOffset, -transform.up, out hit, castDistance, whatIsGround))
        {
            Debug.DrawRay(legTargets[i].position+castOffset, -transform.up * hit.distance, Color.yellow);
            //Debug.Log("Did Hit - downwards");
            legTargets[i].position = hit.point;
        }
        /* //The bottom-up raycast doesnt work because of the single-face ground plane pointing up
        else if (Physics.Raycast(legTargets[i].position-castOffset, transform.up, out hit, castDistance, whatIsGround))
        {
            Debug.DrawRay(legTargets[i].position-castOffset, transform.up * hit.distance, Color.yellow);
            //Debug.Log("Did Hit - upwards");
            legTargets[i].position = hit.point;
        }*/
        else if (Physics.Raycast(legTargets[i].position+Vector3.up*100, -transform.up, out hit, Mathf.Infinity, whatIsGround))
        {
            //Trying to find ground from very high
            legTargets[i].position = hit.point;
        }
        else
        {
            Debug.DrawRay(legTargets[i].position, -transform.up * castDistance, Color.red);
            //Debug.Log("Did not Hit");
        }

    }
    void CheckLegHandle(int i)
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
        
        // check the distance from target (using sqrMagnitude for performance)
        float sqrDistanceFromLeg = (legTargets[i].position - legHandles[i].position).sqrMagnitude;
        float sqrStepGap = stepGap*stepGap;
        //Debug.Log("distance from leg: " + distanceFromLeg);
        Debug.DrawLine(legTargets[i].position, legHandles[i].position, Color.red);

        //int oppositeIndex = 2*pairsOfLegs-1 - i;
        int oppositeIndex = i < pairsOfLegs ? i+pairsOfLegs : i-pairsOfLegs;
        if (!hasToMoveLegs[i] && sqrDistanceFromLeg > sqrStepGap && !hasToMoveLegs[oppositeIndex])
        {
            //check if the neighbouring leg which is in front (i+1) isn't moving
            //this check is skipped for front legs <-> (i+1)%pairsOfLegs == 0
            if ((i+1)%pairsOfLegs == 0 || !hasToMoveLegs[i+1])
            {
                hasToMoveLegs[i] = true;
            }
        }

        // check if the leg is too far from the target, and if so, teleport it
        if (sqrDistanceFromLeg > sqrStepGap*9f)
        {
            legHandles[i].position = legTargets[i].position;
            hasToMoveLegs[i] = false;
        }
        
        if (hasToMoveLegs[i])
        {
            if (sqrDistanceFromLeg < 0.04f) // || sqrDistanceFromLeg > sqrStepGap*9f)
            {
                // the handle has reached target
                legHandles[i].position = legTargets[i].position;
                hasToMoveLegs[i] = false;
            }
            else if (sqrDistanceFromLeg > sqrStepGap*4f)
            {
                // the handle is far from target, so move it faster 
                legHandles[i].position = Vector3.MoveTowards(legHandles[i].position, legTargets[i].position, Time.deltaTime*legSpeed*3f);
            }
            else if (sqrDistanceFromLeg > sqrStepGap*0.49f) 
            {
                // first move the leg up, before aiming to target directly
                legHandles[i].position = Vector3.MoveTowards(legHandles[i].position,
                        legHandles[i].position + (legTargets[i].position-legHandles[i].position)*.5f + transform.up*.4f,
                        Time.deltaTime*legSpeed
                        );
            }
            else
            {
                // descend to target
                legHandles[i].position = Vector3.MoveTowards(legHandles[i].position, legTargets[i].position, Time.deltaTime*legSpeed);
            }
        }
        
    }

    private float UpdateBody() // should be executed after updating legs
    {
        Vector3 candidateNextPos = transform.position + (averageLegsHeight + bodyDefaultHeight)*transform.up + transform.forward*this.GetComponent<MoveAgent>().currentSpeed;
        Vector3 backupPos = candidateNextPos;
        //Debug.DrawLine(transform.position + (averageLegsHeight + bodyDefaultHeight)*transform.up, candidateNextPos, Color.red);
        Debug.DrawLine(bodyTransform.position, candidateNextPos, Color.green);

        this.GetComponent<MoveAgent>().isBlocked = false;
        
        // try to move the body just in the next position, without affecting the body height
        willBodyHit = Physics.CheckBox(candidateNextPos, bodyBox.size*.5f, transform.rotation, whatIsGround);

        if (!willBodyHit)
        {
            //// OK, we can move the body ahead and update the positions
            /// 
            lastValidBodyPos = candidateNextPos;
            return bodyCurrHeight;
        }
        else // check for free space, then offset the body accordingly
        {
            //Debug.Log("collision detected!");
            float step = 0.15f; // the deltaY for each box-check
            
            //// Try BELOW
            ///
            candidateNextPos = backupPos - transform.up*step;
            //candidateNextPos = transform.position + (averageLegsHeight+bodyDefaultHeight - step)*transform.up;
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
            
            //// if here, no free space below. Try ABOVE
            ///
            candidateNextPos = backupPos + transform.up*step;
            //candidateNextPos = transform.position + (averageLegsHeight+bodyDefaultHeight + step)*transform.up;
            
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

            //// if here, no free space available
            /// 
            Debug.LogWarning("Warning: unable to avoid the obstacle");
            this.GetComponent<MoveAgent>().isBlocked = true;
            return bodyCurrHeight;
        }
    }
    IEnumerator PlaceBodyAt(float targetHeight, float animSeconds)
    {
        bodyAdjustCRisRunning = true;
        //Vector3 startPos = bodyTransform.position;
        float startHeight = bodyCurrHeight;
        float t = 0f;
        while (t <= animSeconds)
        {
            t = t + Time.deltaTime;
            float percent = Mathf.Clamp01(t / animSeconds);
            
            //bodyTransform.position = Vector3.Lerp(startPos, targetPos, percent);
            bodyCurrHeight = Mathf.Lerp(startHeight, targetHeight, percent);
            yield return null;
        }
        bodyAdjustCRisRunning = false;
    }
    //failed test to make the agent jump
    /*IEnumerator Jump(float targetHeight, float animSeconds)
    {
        float startHeight = bodyCurrHeight;
        float t = 0f;
        while (t <= animSeconds)
        {
            t = t + Time.deltaTime;
            float percent = Mathf.Clamp01(t / animSeconds);
            
            //bodyTransform.position = Vector3.Lerp(startPos, targetPos, percent);
            bodyCurrHeight = Mathf.Lerp(startHeight, MIN_bodyHeight, percent);
            yield return null;
        }
        yield return new WaitForSeconds(.5f);
        
        t=0f;
        while (t <= animSeconds)
        {
            t = t + Time.deltaTime;
            float percent = Mathf.Clamp01(t / animSeconds);
            
            //bodyTransform.position = Vector3.Lerp(startPos, targetPos, percent);
            bodyCurrHeight = Mathf.Lerp(MIN_bodyHeight, targetHeight, percent);
            yield return null;
        }
    }*/

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying)
        {
            float legGap = bodyLength;
            if (pairsOfLegs>1)
            {
                legGap = bodyLength*2f/(float)(pairsOfLegs-1);
            }
            Vector3 pos;
            Vector3 dir;

            for (int i=0; i < pairsOfLegs; i++)
            {
                dir = pairsOfLegs<2 ? new Vector3(bodyWidth,0,0) : new Vector3(bodyWidth,0,i*legGap - bodyLength);
                pos = bodyTransform.position;// + new Vector3(bodyWidth,0,i*legGap - bodyLength);

                Gizmos.color = Color.yellow;

                Gizmos.DrawWireSphere(pos + dir, .1f); // right root
                Gizmos.DrawWireSphere(pos + new Vector3(-dir.x,dir.y,dir.z), .1f); // left root

                dir = dir.normalized * handleDistance;
                Gizmos.DrawWireCube(pos + dir, Vector3.one*.2f); // right handle
                Gizmos.DrawWireCube(pos - dir, Vector3.one*.2f); // left handle
                //Gizmos.DrawWireCube(pos = pos + new Vector3(handleDistance,0,0), Vector3.one*.2f); // right handle
                //Gizmos.DrawWireCube(pos - new Vector3(2*(bodyWidth+handleDistance),0,0), Vector3.one*.2f); // left handle
                
                Gizmos.DrawWireSphere(pos + dir + poleDelta, .2f); // right pole
                Gizmos.DrawWireSphere(pos - dir + new Vector3(-poleDelta.x,poleDelta.y,poleDelta.z), .2f); // left pole
            }
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
