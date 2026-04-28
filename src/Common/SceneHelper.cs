using Il2Cpp;
using Il2CppTLD.Scenes;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Pathoschild.TheLongDarkMods.Common;

/// <summary>Provides utility methods for working with the game's Unity scenes.</summary>
internal static class SceneHelper
{
    /// <summary>Get whether the save is loaded and ready.</summary>
    public static bool IsSaveLoaded()
    {
        return
            GameManager.m_Instance is not null
            && !GameManager.IsMainMenuActive();
    }

    /// <summary>Get whether the save is loaded and ready, and the player isn't in the transitional 'empty' scene.</summary>
    public static bool IsPlayableScene()
    {
        return
            SceneHelper.IsSaveLoaded()
            && SceneHelper.GetSceneName() is not "Empty"; // note: `GameManager.IsEmptySceneActive()` returns false when the scene is 'Empty'
    }

    /// <summary>Get the active scene.</summary>
    public static Scene GetScene()
    {
        return UnitySceneManager.GetActiveScene();
    }

    /// <summary>Try to get the region containing the current scene.</summary>
    public static RegionSpecification? TryGetRegion()
    {
        return GameManager.TryGetCurrentRegion();
    }

    /// <summary>Get the active scene name.</summary>
    public static string GetSceneName()
    {
        return SceneHelper.GetScene().name;
    }

    /// <summary>Get the localized name for a scene.</summary>
    /// <param name="name">The internal scene name.</param>
    public static string GetDisplayName(string name)
    {
        return InterfaceManager.GetNameForScene(name);
    }

    /// <summary>Get whether a scene is outdoors.</summary>
    /// <param name="name">The internal scene name.</param>
    public static bool IsOutdoors(string name)
    {
        return GameManager.IsOutDoorsScene(name);
    }

    /// <summary>Get whether the current scene is a customizable safehouse location.</summary>
    public static bool IsCustomizableSafehouse()
    {
        return GameManager.GetSafehouseManager().InCustomizableSafehouse();
    }
}
