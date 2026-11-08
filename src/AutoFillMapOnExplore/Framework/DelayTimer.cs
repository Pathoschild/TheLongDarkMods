using System;

namespace Pathoschild.TheLongDarkMods.AutoFillMapOnExplore.Framework;

/// <summary>Tracks the time since an action was last performed.</summary>
internal sealed class DelayTimer
{
    /*********
    ** Fields
    *********/
    /// <summary>When the action was last applied.</summary>
    private DateTime LastApplied = DateTime.UtcNow;


    /*********
    ** Public methods
    *********/
    /// <summary>Get whether enough time has elapsed for a delayed action to apply again.</summary>
    /// <param name="delayInSeconds">The delay between each action, in seconds.</param>
    public bool Check(float delayInSeconds)
    {
        return
            delayInSeconds <= 0
            || (DateTime.UtcNow - this.LastApplied).TotalSeconds > delayInSeconds;
    }

    /// <summary>Reset the delay, so it starts from this moment.</summary>
    public void Reset()
    {
        this.LastApplied = DateTime.UtcNow;
    }
}
