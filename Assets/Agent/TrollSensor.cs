using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class TrollSensor : NearSensor
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Troll")
            || other.gameObject.CompareTag("TrollChief"))
            TryToAdd(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Troll")
            || other.gameObject.CompareTag("TrollChief"))
            TryToRemove(other);
    }
}
