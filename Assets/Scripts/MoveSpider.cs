using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpider : MonoBehaviour
{
    public float moveSpeed = 2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
    }
}
