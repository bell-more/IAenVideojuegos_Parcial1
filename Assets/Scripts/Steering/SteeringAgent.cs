using Unity.VisualScripting;
using UnityEngine;

public class SteeringAgent : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxSteering;
    [SerializeField] private float slowingDistance;
    [SerializeField] private float minDistance = 0.1f;

    private Vector3 velocity;
    private void Update()
    {
        Arrive();
        transform.position += velocity * Time.deltaTime;

        if (velocity != Vector3.zero) transform.forward = velocity;
    }

    private void Seek()
    {
        Vector3 desiredVelocity = (target.position - transform.position).normalized;
        desiredVelocity *= maxSpeed;

        Vector3 steering = desiredVelocity - velocity;

        steering = Vector3.ClampMagnitude(steering, maxSteering * Time.deltaTime);
        velocity += steering;

    }

    private void Flee()
    {
        Vector3 desiredVelocity = (transform.position - target.position).normalized;
        desiredVelocity *= maxSpeed;

        Vector3 steering = desiredVelocity - velocity;

        steering = Vector3.ClampMagnitude(steering, maxSteering * Time.deltaTime);
        velocity += steering;

    }

    private void Arrive()
    {
        Vector3 direction = target.position - transform.position;

        float distance = direction.magnitude;

        if (distance < minDistance)
        {
            velocity = Vector3.zero;
            return;
        }

        float targetSpeed = maxSpeed * (distance / slowingDistance);
        float desiredSpeed = Mathf.Min(targetSpeed, maxSpeed);

        Vector3 desired = direction.normalized * desiredSpeed;
        Vector3 steering = desired - velocity;

        steering = Vector3.ClampMagnitude(steering, maxSteering * Time.deltaTime);
        velocity += steering;
    }
}

