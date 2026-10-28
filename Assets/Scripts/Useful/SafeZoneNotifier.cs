using System;

public static class SafeZoneNotifier
{
    public static event Action OnHunterEnteredSafeZone;

    public static void NotifyHunterEnteredSafeZone()
    {
        OnHunterEnteredSafeZone?.Invoke();
    }
}