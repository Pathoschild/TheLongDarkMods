using System;
using System.Diagnostics.CodeAnalysis;
using MelonLoader;
using Pathoschild.TheLongDarkMods.Common;
using Pathoschild.TheLongDarkMods.Common.Overlays;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework;

/// <summary>An overlay which lists available fast travel destinations in the bottom-right corner.</summary>
[RegisterTypeInIl2Cpp]
internal class DestinationListOverlay : MonoBehaviour
{
    /*********
    ** Fields
    *********/
    /// <summary>The overlay renderer.</summary>
    private readonly SimpleTextOverlay Overlay = new();


    /*********
    ** Accessors
    *********/
    /// <summary>Whether the overlay is currently visible.</summary>
    public bool IsVisible => this.Overlay.IsVisible;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public DestinationListOverlay(IntPtr pointer)
        : base(pointer) { }

    /// <summary>Create and attach the component to a persistent game object.</summary>
    public static DestinationListOverlay Create()
    {
        var anchor = new GameObject($"{ModInfo.UniqueId}_{nameof(DestinationListOverlay)}");
        GameObject.DontDestroyOnLoad(anchor);
        return anchor.AddComponent<DestinationListOverlay>();
    }

    /// <summary>Show the overlay.</summary>
    /// <param name="text">The text to display.</param>
    public void Show(string text)
    {
        this.Overlay.Show(text, OverlayPosition.BottomRight);
    }

    /// <summary>Hide the overlay.</summary>
    public void Hide()
    {
        this.Overlay.Hide();
    }

    /// <summary>Draw the overlay.</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.MethodReferencedByUnity)]
    public void OnGUI()
    {
        this.Overlay.Draw();
    }
}
