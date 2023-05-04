using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class ObjectSensor : NearSensor
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Gem")
            || other.gameObject.CompareTag("Torch")
            || other.gameObject.CompareTag("Chest"))
            TryToAdd(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Gem")
            || other.gameObject.CompareTag("Torch")
            || other.gameObject.CompareTag("Chest"))
            TryToRemove(other);
    }
}
