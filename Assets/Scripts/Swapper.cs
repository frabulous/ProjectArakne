using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swapper : MonoBehaviour
{
    [SerializeField] GameObject obj1, obj2;
    public void SwapObjects()
    {
        if (!obj1 && !obj2) return;
        
        if(obj1.activeSelf)
        {
            obj1.SetActive(false);
            obj2.SetActive(true);
        }
        else
        {
            //obj2.GetComponent<Ground>().OnDisable();
            obj2.SetActive(false);
            obj1.SetActive(true);
        }
    }
}
