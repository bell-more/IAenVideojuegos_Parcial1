using UnityEngine;
using UnityEngine.EventSystems;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 4f;
    private int damage = 1;
    private Boid targetBoid;
    private Vector3 moveDirection;

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

        Debug.Log("projectile hit a boid collider");

        if (other.gameObject.layer == LayerMask.NameToLayer("Boid"))
        {
            Boid boid = other.GetComponentInParent<Boid>();

            if (boid != null && boid == targetBoid)
            {
                boid.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
