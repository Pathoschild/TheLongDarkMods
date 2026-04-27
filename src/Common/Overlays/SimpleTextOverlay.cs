using UnityEngine;

namespace Pathoschild.TheLongDarkMods.Common.Overlays;

/// <summary>A minimal overlay which shows text with a light background in the bottom-right corner of the screen.</summary>
internal class SimpleTextOverlay
{
    /*********
    ** Fields
    *********/
    /// <summary>The default overlay position.</summary>
    private const OverlayPosition DefaultPosition = OverlayPosition.BottomCenter;

    /// <summary>The default font size.</summary>
    private const int DefaultFontSize = 13;

    /// <summary>The pixel padding in the overlay box.</summary>
    private const float Padding = 8f;

    /// <summary>The pixel margins around the overlay box.</summary>
    private const float Margin = 10f;

    /// <summary>The position at which to draw the overlay.</summary>
    private OverlayPosition Position;

    /// <summary>The font size for the displayed text.</summary>
    private int FontSize;

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
    /// <summary>Show the overlay.</summary>
    /// <param name="text">The text to display.</param>
    /// <param name="position">The position at which to draw the overlay.</param>
    /// <param name="fontSize">The font size for the displayed text.</param>
    public void Show(string text, OverlayPosition position = DefaultPosition, int fontSize = DefaultFontSize)
    {
        this.Reset(text, position, fontSize);
    }

    /// <summary>Hide the overlay.</summary>
    public void Hide()
    {
        this.Reset(null, DefaultPosition, DefaultFontSize);
    }

    /// <summary>Draw the overlay on screen. This should be called from the host component's <c>OnGUI</c> method.</summary>
    public void Draw()
    {
        if (!this.IsVisible)
            return;

        if (this.LabelStyle is null)
        {
            this.LabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = this.FontSize,
                padding = new RectOffset(0, 0, 0, 0)
            };
            this.TextSize = this.LabelStyle.CalcSize(new GUIContent(this.Text));
        }

        Vector2 size = this.TextSize;

        float width = size.x + (2 * Padding);
        float height = size.y + (2 * Padding);
        float y = Screen.height - height - Margin;

        float x = this.Position switch
        {
            OverlayPosition.BottomRight => Screen.width - width - Margin,
            _ => (Screen.width / 2) - (width / 2)
        };

        float labelWidth = size.x + Padding; // deliberately draw text into right padding to avoid superfluous wrapping, since we calculated the size to fit

        GUI.Box(new Rect(x, y, width, height), GUIContent.none);
        GUI.Label(new Rect(x + Padding, y + Padding, labelWidth, size.y), this.Text, this.LabelStyle);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Reset the component for the given text.</summary>
    /// <param name="text">The text to show, or <c>null</c> to hide the component.</param>
    /// <param name="position">The position at which to draw the overlay.</param>
    /// <param name="fontSize">The font size for the displayed text.</param>
    private void Reset(string? text, OverlayPosition position, int fontSize)
    {
        if (text != this.Text || fontSize != this.FontSize)
        {
            this.Text = text ?? "";
            this.FontSize = fontSize;
            this.LabelStyle = null; // recalculate size on next draw
            this.IsVisible = text != null;
        }

        this.Position = position;
    }
}
