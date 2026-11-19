using System.Diagnostics;

namespace NoSkippy.Services;

public sealed class SpamLock
{
    private readonly Configuration configuration;
    private long lastInput = long.MinValue / 2;

    public SpamLock(Configuration configuration)
    {
        this.configuration = configuration;
    }

    public bool Locked { get; private set; }

    public bool ShouldSwallow(long nowMs)
    {
        var gap = nowMs - lastInput;
        lastInput = nowMs;

        if (Locked)
        {
            if (gap >= configuration.LockReleaseMs) Locked = false;
            else return true;
        }

        if (gap >= configuration.RapidClickMs) return false;

        Locked = true;
        return true;
    }

    public void Reset()
    {
        Locked = false;
        lastInput = long.MinValue / 2;
    }

    [Conditional("DEBUG")]
    public static void SelfCheck()
    {
        var lck = new SpamLock(new Configuration());
        Debug.Assert(!lck.ShouldSwallow(0), "first click passes");
        Debug.Assert(!lck.ShouldSwallow(1000), "slow second click passes");
        Debug.Assert(lck.ShouldSwallow(1200), "fast click engages lock");
        Debug.Assert(lck.ShouldSwallow(1800), "still locked, timer restarted");
        Debug.Assert(lck.ShouldSwallow(2400), "locked while clicking steadily");
        Debug.Assert(!lck.ShouldSwallow(3200), "released after quiet period");
    }
}
