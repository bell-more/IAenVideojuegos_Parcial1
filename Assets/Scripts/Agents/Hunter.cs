using System.Collections.Generic;
using UnityEngine;

public class Hunter : MonoBehaviour
{
    [SerializeField] private LayerMask boidLayer;

    public List<Transform> targetsInRange = new List<Transform>();

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Boid"))
        {

        }
    }
}
