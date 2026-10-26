
using UnityEngine;

public static class GroundCircleGizmo
{
    public static void Draw(Vector3 position, float radius, Color color, float height = 0.6f)
    {
        if (radius <= 0f) return;

        Vector3 center = new Vector3(position.x, height, position.z);
        const int segments = 64;
        float angleStep = 360f / segments;
        Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);

        Gizmos.color = color;

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }

    public static void DrawVisionCone(Vector3 position, float yRotation, float viewRadius, float viewAngle, Color color, float height = 0.6f)
    {
        if (viewRadius <= 0f) return;

        Vector3 center = new Vector3(position.x, height, position.z);
        float startAngle = yRotation - viewAngle / 2f;
        DrawArcLines(center, viewRadius, startAngle, viewAngle, color);
        Gizmos.color = color;
        Gizmos.DrawLine(center, center + DirectionFromAngle(startAngle) * viewRadius);
        Gizmos.DrawLine(center, center + DirectionFromAngle(startAngle + viewAngle) * viewRadius);
    }

    public static void DrawArc(Vector3 position, float yRotation, float radius, float viewAngle, Color color, float height = 0.6f)
    {
        if (radius <= 0f) return;

        Vector3 center = new Vector3(position.x, height, position.z);
        DrawArcLines(center, radius, yRotation - viewAngle / 2f, viewAngle, color);
    }

    private static void DrawArcLines(Vector3 center, float radius, float startAngle, float angle, Color color)
    {
        const int segments = 30;
        float angleStep = angle / segments;
        Vector3 previousPoint = center + DirectionFromAngle(startAngle) * radius;
        Gizmos.color = color;

        for (int i = 1; i <= segments; i++)
        {
            Vector3 currentPoint = center + DirectionFromAngle(startAngle + angleStep * i) * radius;
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }

    private static Vector3 DirectionFromAngle(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
    }
}
