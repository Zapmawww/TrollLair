using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class RemoveContact : MonoBehaviour
{
    public SteeringBasics thief;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Torch"))
        {
            thief.maxVelocity *= 1.1f;
            Destroy(other.gameObject);
        }

        if (other.gameObject.CompareTag("Troll"))
        {
            thief.maxVelocity *= 0.91f;
            Destroy(other.gameObject);
        }

        if (other.gameObject.CompareTag("Gem")
           || other.gameObject.CompareTag("Chest"))
            Destroy(other.gameObject);

        if (other.gameObject.CompareTag("TrollChief"))
            Destroy(thief.gameObject);
    }
}
