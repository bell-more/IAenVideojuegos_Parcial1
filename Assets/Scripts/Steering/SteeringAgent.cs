using Unity.VisualScripting;
using UnityEngine;

public class SteeringAgent : Agent
{
    [Header("References")]

    [Header("Stats")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxSteering;
    [SerializeField] private float slowingDistance;
    [SerializeField] private float minDistance = 0.1f;

    public float MaxSpeed => maxSpeed;

    public float MaxSteering => maxSteering;
    public Vector3 Velocity => velocity;
    public float GetMinDistance => minDistance; 

    private void Update()
    {
        transform.position = Bounds.Instance.OutOfBounds(transform.position);

        transform.position += velocity * Time.deltaTime;

        if (velocity != Vector3.zero) transform.forward = velocity;
    }

    public Vector3 CalculateSteering(Vector3 desiredVelocity)
    {
        Vector3 steering = desiredVelocity - velocity;
        steering = Vector3.ClampMagnitude(steering, maxSteering);

        return steering;
    }

    public void ApplySteering(Vector3 steering)
    {
        velocity += steering * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
    }

    private Vector3 DesiredVelocity(Vector3 target)
    {
        Vector3 desiredVelocity = (target - transform.position).normalized;
        desiredVelocity *= maxSpeed;

        return desiredVelocity;
    }
    public Vector3 Seek(Vector3 target)
    {
        Vector3 desired = DesiredVelocity(target);
        return CalculateSteering(desired);
    }

    private Vector3 Flee(Vector3 target)
    {
        Vector3 desired = DesiredVelocity(target);
        return CalculateSteering(-desired);
    }

    public Vector3 Arrive(Vector3 target)
    {
        Vector3 direction = target - transform.position;

        float distance = direction.magnitude;

        if (distance < minDistance)
        {
            return CalculateSteering(Vector3.zero);
        }

        float desiredSpeed = Mathf.Clamp(maxSpeed * distance / slowingDistance, 0, maxSpeed);

        Vector3 desired = direction.normalized * desiredSpeed;
        
        return CalculateSteering(desired);
    }

    private Vector3 PredictTargetPosition(Agent target)
    {
        Vector3 direction = target.transform.position - transform.position;
        float distance = direction.magnitude;

        float predictionTime;

        if (maxSpeed <= 0)
        {
            predictionTime = 0;
        }
        else
        {
            predictionTime = distance / (maxSpeed + target.velocity.magnitude);
        }

        Vector3 futurePosition = target.transform.position + target.velocity * predictionTime;

        return futurePosition;
    }
    public Vector3 Pursuit(Agent target)
    {
        Vector3 futurePosition = PredictTargetPosition(target);
        return Seek(futurePosition);
    }

    public Vector3 Evade(Agent target)
    {
        return Flee(PredictTargetPosition(target));
    }

    public void Stop()
    {
        velocity = Vector3.zero;
    }
}

