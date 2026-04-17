using Il2Cpp;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Pathoschild.TheLongDarkMods.Common;

/// <summary>Provides utility methods for working with the game's Unity scenes.</summary>
internal static class SceneHelper
{
    /// <summary>Whether the save is loaded and ready.</summary>
    public static bool IsSaveLoaded()
    {
        return
            GameManager.m_Instance is not null
            && !GameManager.IsMainMenuActive()
            && SceneHelper.GetSceneName() is not (null or "" or "MainMenu");
    }

    /// <summary>Get the active scene.</summary>
    public static Scene GetScene()
    {
        return UnitySceneManager.GetActiveScene();
    }

    /// <summary>Get the active scene name.</summary>
    public static string GetSceneName()
    {
        return SceneHelper.GetScene().name;
    }

    /// <summary>Get the localized name for a scene.</summary>
    /// <param name="name">The internal scene name.</param>
    public static string GetSceneDisplayName(string name)
    {
        return InterfaceManager.GetNameForScene(name);
    }
}
