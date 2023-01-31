using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Swapper : MonoBehaviour
{
    [SerializeField] GameObject grnd1_play, grnd2_proc;
    [SerializeField] GameObject proceduralGrndText;

    public void SwapObjects()
    {
        if (!grnd1_play && !grnd2_proc) return;
        
        if(grnd1_play.activeSelf)
        {
            grnd1_play.SetActive(false);
            grnd2_proc.SetActive(true);
            proceduralGrndText.SetActive(true);
        }
        else
        {
            //obj2.GetComponent<Ground>().OnDisable();
            grnd2_proc.SetActive(false);
            grnd1_play.SetActive(true);
            proceduralGrndText.SetActive(false);
        }
    }
}
