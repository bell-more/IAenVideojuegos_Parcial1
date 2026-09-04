using UnityEngine;

public class Bounds : MonoBehaviour
{

    public static Bounds Instance { get; private set; }
    [SerializeField] private float height = 30f;
    [SerializeField] private float width = 60f;
    [SerializeField] private float offset = 1.5f;     
    [SerializeField] private float padding = 2f;
    [SerializeField] private bool drawGizmos;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Vector3 OutOfBounds(Vector3 position)
    {
        Vector3 newPos = position;

        float limitX = (width / 2f) + offset;
        float limitZ = (height / 2f) + offset;

        // Eje X: usamos else if para evitar comprobaciones dobles irreales
        if (position.x > limitX)
        {
            newPos.x = -limitX + padding;
        }
        else if (position.x < -limitX)
        {
            newPos.x = limitX - padding;
        }

        // Eje Z
        if (position.z > limitZ)
        {
            newPos.z = -limitZ + padding;
        }
        else if (position.z < -limitZ)
        {
            newPos.z = limitZ - padding;
        }

        return newPos;
    }
    private void OnDrawGizmos()
    {
        if(!drawGizmos) return;

        Gizmos.color = Color.pink;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, 0, height));
    }

}
