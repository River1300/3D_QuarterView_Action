using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartZone : MonoBehaviour
{
    public GameManager manager;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            manager.StageStart();
        }
    }
}
