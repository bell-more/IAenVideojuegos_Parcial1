using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 4f;
    private int damage = 1;
    private Boid targetBoid;
    private Vector3 moveDirection;
    [SerializeField] private LayerMask boidLayer;

    public void Initialize(Boid target)
    {
        targetBoid = target;

        moveDirection = (targetBoid.transform.position - transform.position).normalized;
        transform.forward = moveDirection;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        if (((1 << other.gameObject.layer) & boidLayer) != 0)
        {
            Boid boid = other.GetComponentInParent<Boid>();

            if (boid != null)
            {
                boid.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
