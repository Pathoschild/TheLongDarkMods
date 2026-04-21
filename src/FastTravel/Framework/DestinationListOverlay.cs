using System;
using System.Diagnostics.CodeAnalysis;
using MelonLoader;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework;

/// <summary>An overlay which lists available fast travel destinations in the bottom-right corner.</summary>
[RegisterTypeInIl2Cpp]
internal class DestinationListOverlay : MonoBehaviour
{
    /*********
    ** Fields
    *********/
    /// <summary>The pixel padding in the overlay box.</summary>
    private const float Padding = 8f;

    /// <summary>The pixel margins around the overlay box.</summary>
    private const float Margin = 10f;

    /// <summary>The text to display.</summary>
    private string Text = "";

    /// <summary>The label style to apply.</summary>
    private GUIStyle? LabelStyle;

    /// <summary>The pixel text size when drawn.</summary>
    private Vector2 TextSize;


    /*********
    ** Accessors
    *********/
    /// <summary>Whether the overlay is currently visible.</summary>
    public bool IsVisible { get; private set; }


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public DestinationListOverlay(IntPtr pointer)
        : base(pointer) { }

    /// <summary>Create and attach the component to a persistent GameObject.</summary>
    public static DestinationListOverlay Create()
    {
        var gameObj = new GameObject($"FastTravel_{nameof(DestinationListOverlay)}");
        GameObject.DontDestroyOnLoad(gameObj);
        return gameObj.AddComponent<DestinationListOverlay>();
    }

    /// <summary>Show the overlay.</summary>
    /// <param name="text">The text to display.</param>
    public void Show(string text)
    {
        this.Reset(text);
    }

    /// <summary>Hide the overlay.</summary>
    public void Hide()
    {
        this.Reset(null);
    }

    /// <summary>Draw the overlay.</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Unity loads the method dynamically.")]
    public void OnGUI()
    {
        if (!this.IsVisible)
            return;

        if (this.LabelStyle is null)
        {
            this.LabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                padding = new RectOffset(0, 0, 0, 0)
            };
            this.TextSize = this.LabelStyle.CalcSize(new GUIContent(this.Text));
        }

        Vector2 size = this.TextSize;

        float width = size.x + (2 * Padding);
        float height = size.y + (2 * Padding);
        float x = Screen.width - width - Margin;
        float y = Screen.height - height - Margin;

        float labelWidth = size.x + Padding; // deliberately draw text into right padding to avoid superfluous wrapping, since we calculated the size to fit

        GUI.Box(new Rect(x, y, width, height), GUIContent.none);
        GUI.Label(new Rect(x + Padding, y + Padding, labelWidth, size.y), this.Text, this.LabelStyle);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Reset the component for the given text.</summary>
    /// <param name="text">The text to show, or <c>null</c> to hide the component.</param>
    private void Reset(string? text)
    {
        this.Text = text ?? "";
        this.LabelStyle = null; // recalculate size on next draw
        this.IsVisible = text != null;
    }
}
