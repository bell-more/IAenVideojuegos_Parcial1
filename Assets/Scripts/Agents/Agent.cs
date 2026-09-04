using UnityEngine;

public class Agent : MonoBehaviour
{
    private Vector3 Velocity;

    public Vector3 velocity
    {
        get => Velocity;
        set => Velocity = value;
    }
}
