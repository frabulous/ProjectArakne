using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatAround : MonoBehaviour
{
    public float radius = 1.0f;
    public float timer = 3.0f;

    private Vector3 localCenter;
    private float timeout;

    // Start is called before the first frame update
    void Start()
    {
        localCenter = transform.localPosition;
        timeout = timer;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeout > 0f)
        {
            timeout -= Time.deltaTime;
        }
        else
        {
            StartCoroutine(SmoothMove());
            timeout = Random.Range(0.9f, 1.1f) * timer;
        }
    }

    //coroutine that smoothly translates the object to a random position within the radius
    IEnumerator SmoothMove()
    {
        Vector3 target = localCenter + Random.insideUnitSphere * radius;
        float elapsedTime = 0;
        float time = 2.0f;
        Vector3 startingPos = transform.localPosition;
        while (elapsedTime < time)
        {
            transform.localPosition = Vector3.Lerp(startingPos, target, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = target;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.parent.position + localCenter, radius);
    }
}
