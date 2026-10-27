using UnityEngine;

public class SteeringAgent : Agent
{
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxSteering;
    [SerializeField] private float slowingDistance;
    [SerializeField] private float minDistance = 0.1f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float steeringGrip = 5f;

    [SerializeField] private bool useBounds = true;
    [SerializeField] private bool isBoid = false;

    public float MaxSpeed => maxSpeed;
    public Vector3 Velocity => velocity;

    private void Update()
    {
        if (useBounds && Bounds.Instance != null)
        {
            transform.position = Bounds.Instance.OutOfBounds(transform.position);
        }

        transform.position += velocity * Time.deltaTime;

        if (isBoid) Boid();
        else WolvesHunter();

    }
    private void WolvesHunter()
    {
        if (velocity.sqrMagnitude > 0.001f)
        {
            Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
            Quaternion targetRotation = Quaternion.LookRotation(flatVelocity);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            velocity = transform.forward * velocity.magnitude;
        }
    }

    private void Boid()
    {
        if (velocity.sqrMagnitude > 0.001f)
        {
            Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
            Quaternion targetRotation = Quaternion.LookRotation(flatVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public Vector3 CalculateSteering(Vector3 desiredVelocity)
    {
        Vector3 steering = desiredVelocity - velocity;
        steering *= steeringGrip;
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