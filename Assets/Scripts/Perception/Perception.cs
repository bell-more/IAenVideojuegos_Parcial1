using System.Collections.Generic;
using UnityEngine;
public class Perception : MonoBehaviour
{
    [SerializeField] private float checkInterval = 0.2f;
    [SerializeField] private float eyeHeight = 1f;
    [SerializeField] private LayerMask obstacleLayer;

    private List<Collider> targetsInRange = new List<Collider>();
    public List<Collider> TargetsInRange => targetsInRange;
    private SphereCollider triggerCollider;

    private float checkTimer;
    private Transform cachedTarget;
    private float cachedRadius;
    private LayerMask cachedLayer;
    private bool cachedUseFOV;
    private float cachedViewAngle;
    private bool hasCachedResult;

    private void Awake()
    {
        triggerCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (!targetsInRange.Contains(other))
        {
            targetsInRange.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;
        if (targetsInRange.Contains(other))
        {
            targetsInRange.Remove(other);
        }
    }

    private void Update()
    {
        if (checkTimer > 0f) checkTimer -= Time.deltaTime;
    }

    public Transform GetClosestVisibleTarget(float radius, LayerMask targetLayer, bool useFOV = false, float viewAngle = 90f)
    {

        bool settingsChanged = !hasCachedResult || cachedRadius != radius || cachedLayer != targetLayer || cachedUseFOV != useFOV || cachedViewAngle != viewAngle;

        if (settingsChanged || checkTimer <= 0f)
        {
            cachedRadius = radius;
            cachedLayer = targetLayer;
            cachedUseFOV = useFOV;
            cachedViewAngle = viewAngle;
            cachedTarget = FindClosestVisibleTarget(radius, targetLayer, useFOV, viewAngle);
            hasCachedResult = true;
            checkTimer = Mathf.Max(0f, checkInterval);
        }

        return cachedTarget;
    }

    private Transform FindClosestVisibleTarget(float radius, LayerMask targetLayer, bool useFOV, float viewAngle)
    {
        targetsInRange.RemoveAll(t => t == null);

        Transform closestTarget = null;
        float closestDistanceSqr = radius * radius;

        foreach (Collider targetCollider in targetsInRange)
        {
            if (targetCollider == null || !targetCollider.gameObject.activeInHierarchy || !targetCollider.enabled) continue;
            if (((1 << targetCollider.gameObject.layer) & targetLayer) == 0) continue;

            Transform target = ResolveTargetTransform(targetCollider);
            if (target == null || target == transform) continue;

            Vector3 direction = target.position - transform.position;
            float distanceSqr = direction.sqrMagnitude;

            if (distanceSqr >= closestDistanceSqr) continue;
            if (useFOV && !IsWithinViewAngle(target, viewAngle)) continue;
            if (!HasLineOfSight(target.position)) continue;

            closestDistanceSqr = distanceSqr;
            closestTarget = target;
        }

        return closestTarget;
    }

    private Transform ResolveTargetTransform(Collider targetCollider)
    {
        Wolf wolf = targetCollider.GetComponentInParent<Wolf>();
        if (wolf != null) return wolf.transform;

        Hunter hunter = targetCollider.GetComponentInParent<Hunter>();
        if (hunter != null) return hunter.transform;

        return targetCollider.transform;
    }

    public bool IsWithinViewAngle(Transform target, float viewAngle)
    {
        if (target == null) return false;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f) return true;

        return Vector3.Angle(transform.forward, direction.normalized) <= viewAngle * 0.5f;
    }

    public bool HasLineOfSight(Transform target)
    {
        return target != null && HasLineOfSight(target.position);
    }

    public bool HasLineOfSight(Vector3 targetPosition)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 targetEye = targetPosition + Vector3.up * eyeHeight;
        Vector3 direction = targetEye - origin;

        if (direction.sqrMagnitude <= 0.0001f) return true;

        return !Physics.Raycast(origin, direction.normalized, direction.magnitude, obstacleLayer);
    }

    public void InvalidateCache()
    {
        hasCachedResult = false;
        cachedTarget = null;
        checkTimer = 0f;
    }

}