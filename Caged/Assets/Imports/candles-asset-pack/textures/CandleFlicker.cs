using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleFlicker : MonoBehaviour
{
    [SerializeField] float min;
    [SerializeField] float max;
    [SerializeField] float flickTime;

    [SerializeField] MeshRenderer candleLight;

    void Start(){
        
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker(){

        WaitForSeconds waitTime = new WaitForSeconds(UnityEngine.Random.Range(min, max));
        while (true){

            candleLight.enabled = false;
            yield return new WaitForSeconds(flickTime);
            candleLight.enabled = true;

            yield return waitTime;
        }
    }
}
