using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FastIK : MonoBehaviour
{
    [Header("Initial Setup")]
    [SerializeField] [Range(2,10)] private int chainLength = 2;
    [SerializeField] public Transform target;
    [SerializeField] public Transform pole;

    [Header("Solver Parameters")]
    /// <summary>
    ///  Amount of iterations per frame the solver will do to reach the target
    /// </summary>
    [SerializeField] private int iterations = 10;
    /// <summary>
    ///  The tolerance of the solver.
    ///  It will stop the iterations when the distance from the target gets lower
    /// </summary>
    [SerializeField] private float delta = .01f;

    //[SerializeField] [Range(0,1)] private float strength = 1;
    public bool useNewTechnique;


    private Transform[] bones;
    private Vector3[] positions;    
    private float[] bonesLength;
    private float completeLength;
    //for rotations:
    private Vector3[] startDirectionsToNext;
    private Quaternion[] startRotationsBone;
    private Quaternion startRotationTarget, startRotationRoot;
    
    
    void Awake()
    {
        Initialize();
    }

    void LateUpdate()
    {
        //SolveIK();
    }

    public void Initialize()
    {
        bones = new Transform[chainLength+1];
        positions = new Vector3[chainLength+1];
        bonesLength = new float[chainLength];
        completeLength = 0;

        startDirectionsToNext = new Vector3[chainLength+1];
        startRotationsBone = new Quaternion[chainLength+1];
        startRotationTarget = target.rotation;

        Transform curr = this.transform;
        for (var i = bones.Length-1; i>=0; i--)
        {
            bones[i] = curr;
            startRotationsBone[i] = curr.rotation;

            if (i==bones.Length-1)
            {
                // we are in the leaf bone (1st loop iteration)
                startDirectionsToNext[i] = target.position - curr.position;
            }
            else
            {
                startDirectionsToNext[i] = bones[i+1].position - curr.position;
                bonesLength[i] = startDirectionsToNext[i].magnitude;
                //bonesLength[i] = (curr.position - bones[i+1].position).magnitude;
                completeLength += bonesLength[i];
            }

            curr = curr.parent;
        }

        print("Initialized!");
        print("bones: " + bones.ToString());
        print("positions: " + positions.ToString());
        print("bonesLength: " + bonesLength.ToString());
        print("completeLength: " + completeLength);
    }


    public void SolveIK()
    {
        if (target==null) return;
        if (bonesLength.Length != chainLength) Initialize();

        // Getting the bone positions to make some computations without affecting them directly
        for (int i=0; i < bones.Length; i++)
        {
            positions[i] = bones[i].position;
        }

        var rootRot = (bones[0].parent != null) ? bones[0].parent.rotation : Quaternion.identity;
        var rootRotDiff = rootRot * Quaternion.Inverse(startRotationRoot);

        if (Vector3.SqrMagnitude(target.position - positions[0]) >= completeLength*completeLength)
        {
            // In this case the target is further than the length of the (extended) limb;
            // so we just need to align the bones and direct everything towards the target

            Vector3 direction = (target.position - positions[0]).normalized;

            for (int i=1; i < positions.Length; i++) // we skip the root, that stays in place
            {
                positions[i] = positions[i-1] + direction*bonesLength[i-1];
            }
        }
        else
        {
            // In this case we need to bend the limb and try to place the leaf bone on target accordingly;
            // we do it through iterations from leaf to root and viceversa to be more accurate

            for (int n=0; n < iterations; n++)
            {
                /// BACKWARD
                /// 
                positions[positions.Length-1] = target.position; //base step: put the leaf bone on the target

                for (int i = positions.Length-2; i > 0; i--) //N.B: i>0 means we don't move the root bone at all!
                {
                    if (!useNewTechnique)
                    {
                        //any other bone in the chain is put along the line connecting it to its child bone
                        positions[i] = positions[i+1] + (positions[i] - positions[i+1]).normalized * bonesLength[i];
                    }
                    else 
                    {
                        //TODO
                        //each bone in the chain is put along the line passing through i+1 and the point in between i and i-1 bones
                        Vector3 dir = ((positions[i-1] + positions[i])*.5f) - positions[i+1];
                        positions[i] = positions[i+1] + dir.normalized * bonesLength[i];
                    }
                }

                /// FORWARD
                /// 
                for (int i=1; i < positions.Length; i++)
                {
                    positions[i] = positions[i-1] + (positions[i] - positions[i-1]).normalized * bonesLength[i-1];
                }

                if (Vector3.SqrMagnitude(target.position - positions[positions.Length-1]) < delta*delta)
                    break; // the leaf bone is close enough to the target
            }
        }

        if (pole != null)
        {
            for (int i=positions.Length-2; i > 0; i--) // we skip root and leaf bones
            {
                // For each internal bone, we project the pole and the bone 
                // on the plane passing through the previous bone
                // and having as normal the vector from the previous bone to the next one.
                // Then we rotate the bone around the normal to minimize the angle and to align it with the pole
                
                Plane plane = new Plane(positions[i+1] - positions[i-1], positions[i-1]);
                Vector3 projectedPole = plane.ClosestPointOnPlane(pole.position);
                Vector3 projectedBone = plane.ClosestPointOnPlane(positions[i]);
                float angle = Vector3.SignedAngle(projectedBone - positions[i-1], projectedPole - positions[i-1], plane.normal);
                positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i-1]) + positions[i-1];
                
            }

            /// TEST: same approach but using vector products instead of plane computations
            /*for (int i=1; i < positions.Length-1; i++) // we skip root and leaf bones
            {
                Vector3 v = positions[i+1]-positions[i-1];
                Vector3 w = positions[i]-positions[i-1];
                Vector3 n = Vector3.Cross(w, v);
                Vector3 j = Vector3.Cross(v, n).normalized;
                Vector3 projectedBone = positions[i-1] + j * Vector3.Dot(w, j);

                w = pole.position - positions[i-1];
                n = Vector3.Cross(w, v);
                j = Vector3.Cross(v, n).normalized;
                Vector3 projectedPole = positions[i-1] + j * Vector3.Dot(w, j);

                v = v.normalized;

                float angle = Vector3.SignedAngle(projectedBone - positions[i-1], projectedPole - positions[i-1], v);
                positions[i] = Quaternion.AngleAxis(angle, v) * (positions[i] - positions[i-1]) + positions[i-1];
            }*/
        }

        // Setting the rotations and positions for the actual bones
        for (int i=0; i < positions.Length; i++)
        {
            if (i==positions.Length-1)
                bones[i].rotation = target.rotation * Quaternion.Inverse(startRotationTarget)*startRotationsBone[i];
            else
                bones[i].rotation = Quaternion.FromToRotation(startDirectionsToNext[i], positions[i+1]-positions[i])*startRotationsBone[i];

            bones[i].position = positions[i];
        }

    }

    private void OnDrawGizmos() 
    {
        /*
        Transform temp = this.transform;
        Transform next = this.transform.parent;
        for (var i = 0; i<chainLength && temp!=null && next!=null; i++)
        {
            float scale = Vector3.Distance(temp.position, next.position)*.1f;

            Handles.color = Color.green;
            Handles.matrix = Matrix4x4.TRS(temp.position,
                                        Quaternion.FromToRotation(Vector3.up, 
                                                                next.position - temp.position),
                                                                new Vector3(scale,
                                                                        Vector3.Distance(next.position, temp.position),
                                                                        scale));
            Handles.DrawWireCube(Vector3.up*.5f, Vector3.one);

            temp = next;
            next = next.parent;
        }*/
    }
}
