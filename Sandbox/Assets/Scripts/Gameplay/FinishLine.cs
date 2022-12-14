using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    //public EventManager.OnVoidDelegate OnContact;

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.CompareTag("Player"))
        {
            EventManager.OnRaceEnd?.Invoke(false);
            Destroy(this.gameObject);
        }
    }
}
