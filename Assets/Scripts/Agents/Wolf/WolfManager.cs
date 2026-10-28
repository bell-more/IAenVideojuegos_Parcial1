using System;
using System.Collections.Generic;
using UnityEngine;

public class WolfManager : MonoBehaviour
{
    public static WolfManager Instance { get; private set; }
    public event Action<Vector3, Wolf> OnHunterDetected;
    public event Action OnSafeZoneReached;

    private List<Wolf> allWolves = new List<Wolf>();
    private void OnEnable()
    {
        SafeZoneNotifier.OnHunterEnteredSafeZone += HandleHunterEnteredSafeZone;
    }

    private void OnDisable()
    {
        SafeZoneNotifier.OnHunterEnteredSafeZone -= HandleHunterEnteredSafeZone;
    }

    public void NotifySafeZoneReached()
    {
        OnSafeZoneReached?.Invoke();
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterWolf(Wolf wolf)
    {
        if (!allWolves.Contains(wolf)) allWolves.Add(wolf);
    }

    public void UnregisterWolf(Wolf wolf)
    {
        if (allWolves.Contains(wolf)) allWolves.Remove(wolf);
    }

    public void NotifyHunterDetected(Vector3 position, Wolf sender)
    {
        OnHunterDetected?.Invoke(position, sender);
    }

    private void HandleHunterEnteredSafeZone()
    {
        NotifySafeZoneReached();
    }


    public bool IsAnyWolfActive()
    {
        foreach (Wolf wolf in allWolves)
        {
            if (wolf != null && (wolf.IsSeeingHunter || wolf.IsAlerted || wolf.IsInvestigating))
            {
                return true;
            }
        }
        return false;
    }
}