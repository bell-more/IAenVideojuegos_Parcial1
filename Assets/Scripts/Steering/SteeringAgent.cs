using Unity.VisualScripting;
using UnityEngine;

public class SteeringAgent : Agent
{
    [SerializeField] private Agent target;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxSteering;
    [SerializeField] private float slowingDistance;
    [SerializeField] private float minDistance = 0.1f;

    public enum SteeringModes { Seek, Flee, Arrive, Pursuit, Evade, Flocking}
    public SteeringModes currentSteering;

    private void Awake()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1, 1), 0f, Random.Range(-1, 1));
        velocity += randomDirection.normalized * maxSpeed;
    }
    private void Update()
    {
        if (target == null) return;

        velocity += GetSteering();

        transform.position += velocity * Time.deltaTime;

        if (velocity != Vector3.zero) transform.forward = velocity;
    }

    private Vector3 GetSteering()
    {
        switch (currentSteering)
        {
            case SteeringModes.Seek:
                return Seek(target.transform.position);
            case SteeringModes.Flee:
                return Flee(target.transform.position);
            case SteeringModes.Arrive:
                return Arrive(target.transform.position);
            case SteeringModes.Pursuit:
                return Pursuit(target);
            case SteeringModes.Evade:
                return Evade(target);
            case SteeringModes.Flocking:
                return transform.position;
            default:
                return Vector3.zero;
        }
    }
    private Vector3 CalculateSteering(Vector3 desiredVelocity)
    {
        Vector3 steering = desiredVelocity - velocity;
        steering = Vector3.ClampMagnitude(steering, maxSteering * Time.deltaTime);

        return steering;
    }

    private Vector3 DesiredVelocity(Vector3 target)
    {
        Vector3 desiredVelocity = (target - transform.position).normalized;
        desiredVelocity *= maxSpeed;

        return desiredVelocity;
    }
    private Vector3 Seek(Vector3 target)
    {
        Vector3 desired = DesiredVelocity(target);
        return CalculateSteering(desired);
    }

    private Vector3 Flee(Vector3 target)
    {
        Vector3 desired = DesiredVelocity(target);
        return CalculateSteering(-desired);
    }

    private Vector3 Arrive(Vector3 target)
    {
        Vector3 direction = target - transform.position;

        float distance = direction.magnitude;

        if (distance < minDistance)
        {
            return CalculateSteering(Vector3.zero);
        }

        float desiredSpeed = Mathf.Clamp(maxSpeed * distance / slowingDistance,0,maxSpeed);

        Vector3 desired = direction.normalized * desiredSpeed;
        return CalculateSteering(desired);
    }

    private Vector3 PredictTargetPosition(Agent target)
    {
        Vector3 direction = target.transform.position - transform.position;
        float distance = direction.magnitude;

        var prediction = distance / (maxSpeed + target.velocity.magnitude);
        Vector3 futurePos = target.transform.position + target.velocity * prediction;
        return futurePos;
    }
    private Vector3 Pursuit(Agent target)
    {
        return Seek(PredictTargetPosition(target));
    }

    private Vector3 Evade(Agent target)
    {
        return Flee(PredictTargetPosition(target));
    }
}

