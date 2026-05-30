namespace Pathoschild.TheLongDarkMods.Common;

/// <summary>The justifications for suppressing warnings.</summary>
public static class SuppressReasons
{
    /// <summary>Indicates that a method is referenced dynamically by Unity.</summary>
    public const string MethodReferencedByUnity = "Unity loads the method dynamically.";

    /// <summary>Indicates that patches are applied by MelonLoader automatically.</summary>
    public const string MethodsReferencedByHarmony = "Harmony patches are applied by MelonLoader automatically.";

    /// <summary>Indicates that parameters can't be renamed since they match Harmony conventions.</summary>
    public const string ParametersMatchHarmonyConventions = "Parameter names must match Harmony conventions.";
}
