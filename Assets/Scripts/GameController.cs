using UnityEngine;

/// <summary>Static match state, as introduced in lesson 17. Init starts a fresh match.</summary>
public static class GameController
{
    public static int TotalCollectibles { get; private set; }
    public static int CollectedCount { get; private set; }
    public static int Remaining => TotalCollectibles - CollectedCount;
    public static bool GameOver => TotalCollectibles > 0 && Remaining == 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => Init(0);

    public static void Init(int total)
    {
        TotalCollectibles = Mathf.Max(0, total);
        CollectedCount = 0;
    }

    public static void Collect()
    {
        if (Remaining > 0) CollectedCount++;
    }
}
