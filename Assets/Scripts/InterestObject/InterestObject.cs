using UnityEngine;

public class InterestObject : MonoBehaviour
{
    [SerializeField] private int life = 2;
    public int Life => life;
    public bool IsAlive => life > 0;

    [SerializeField] private Hunter hunter;

    public void SetHunter(Hunter hunter)
    {
        this.hunter = hunter;
    }

    public void TakeDamage(int damage)
    {
        life -= damage;

        if (life <= 0)
        {
            life = 0;
            if (hunter != null) hunter.RemoveInterestObject();
            Destroy(gameObject);
        }
    }
}
