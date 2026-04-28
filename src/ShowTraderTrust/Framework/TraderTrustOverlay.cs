using System;
using System.Diagnostics.CodeAnalysis;
using MelonLoader;
using Pathoschild.TheLongDarkMods.Common.Overlays;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.ShowTraderTrust.Framework;

/// <summary>An overlay which shows trader trust at the bottom of the screen.</summary>
[RegisterTypeInIl2Cpp]
internal class TraderTrustOverlay : MonoBehaviour
{
    /*********
    ** Fields
    *********/
    /// <summary>The overlay renderer.</summary>
    private readonly SimpleTextOverlay Overlay = new();


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public TraderTrustOverlay(IntPtr pointer)
        : base(pointer) { }

    /// <summary>Create and attach the component to a persistent GameObject.</summary>
    public static TraderTrustOverlay Create()
    {
        var gameObj = new GameObject($"{ModInfo.UniqueId}_{nameof(TraderTrustOverlay)}");
        GameObject.DontDestroyOnLoad(gameObj);
        return gameObj.AddComponent<TraderTrustOverlay>();
    }

    /// <summary>Show the overlay.</summary>
    /// <param name="text">The text to display.</param>
    public void Show(string text)
    {
        this.Overlay.Show(text, fontSize: 20);
    }

    /// <summary>Hide the overlay.</summary>
    public void Hide()
    {
        this.Overlay.Hide();
    }

    /// <summary>Draw the overlay.</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Unity loads the method dynamically.")]
    public void OnGUI()
    {
        this.Overlay.Draw();
    }
}
