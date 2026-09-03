using UnityEngine;

public class Bounds : MonoBehaviour
{

    public static Bounds Instance { get; private set; }
    [SerializeField] private float height = 30f;
    [SerializeField] private float width = 60f;
    [SerializeField] private bool drawGizmos;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void OnDrawGizmos()
    {
        if(!drawGizmos) return;

        Gizmos.color = Color.pink;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, 0, height));
    }

}
