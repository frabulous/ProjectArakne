using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpider : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Transform target;
    public float stopDistance = 2f;
    public float slowDistance = 10f;

    public bool isBlocked;

    void Start()
    {
        StartCoroutine(WaitAndEnable());
        this.enabled = false;
    }

    void Update()
    {
        if (isBlocked) return;
        
        float speed = moveSpeed;
        if (target)
        {
            //SEEK the target considering only xz plane
            float sqrDistanceXZ = new Vector2(target.position.x - transform.position.x,
                                    target.position.z - transform.position.z).sqrMagnitude;
            if (sqrDistanceXZ < stopDistance*stopDistance)
            {
                //just stop
                speed = 0;
            }
            else if (sqrDistanceXZ < slowDistance*slowDistance)
            {
                //slow down
                speed = Mathf.Lerp(0.5f, moveSpeed, sqrDistanceXZ/(slowDistance*slowDistance));
            }

            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        }
        //Debug.Log("speed = " + speed);
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        
    }
    
    private IEnumerator WaitAndEnable()
    {
        yield return new WaitForSeconds(2f);
        this.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (target)
        {
            Vector3 destination = new Vector3(target.position.x, transform.position.y, target.position.z);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(destination, slowDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(destination, stopDistance);
        }
    }
}
