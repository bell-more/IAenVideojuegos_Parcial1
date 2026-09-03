using System.Collections.Generic;
using UnityEngine;

public class Hunter : MonoBehaviour
{
    [SerializeField] private LayerMask boidLayer;

    public List<Transform> targetsInRange = new List<Transform>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Boid"))
        {
            //changes state to pursuit
            //if it's close enough -> changes to attack
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Boid"))
        {
            //changes state to patrol
        }
    }
}
