using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class ThiefSensor : NearSensor
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
            TryToAdd(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
            TryToRemove(other);
    }
}
