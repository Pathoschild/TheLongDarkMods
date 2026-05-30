using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using Pathoschild.TheLongDarkMods.Common;
using Pathoschild.TheLongDarkMods.FastTravel.Framework.DataModels;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework;

/// <summary>A pause menu panel which allows managing fast travel destinations.</summary>
[RegisterTypeInIl2Cpp]
internal class DestinationManagerPanel : MonoBehaviour
{
    /*********
    ** Fields
    *********/
    /****
    ** Theme constants
    ****/
    /// <summary>The width of the panel in pixels.</summary>
    private const float PanelWidth = 720;

    /// <summary>The height of the panel in pixels.</summary>
    private const float PanelHeight = 560;

    /// <summary>The game's UI font name.</summary>
    private const string TextFontName = "Nazhdak-Regular";

    /// <summary>The default text color.</summary>
    private static readonly Color TextColor = new(0.78f, 0.78f, 0.78f);

    /// <summary>The text color for 'grayed out' text.</summary>
    private static readonly Color TextDimColor = new(0.42f, 0.42f, 0.42f);

    /// <summary>The text color for a clickable item while the cursor is over it.</summary>
    private static readonly Color TextHoverColor = Color.white;

    /// <summary>The color for the panel background.</summary>
    private static readonly Color PanelBackgroundColor = new(0.06f, 0.06f, 0.06f, 0.97f);

    /// <summary>The default background color for a clickable button.</summary>
    private static readonly Color ButtonBackgroundColor = new(0.00f, 0.00f, 0.00f, 0.00f);

    /// <summary>The background color for a clickable button while the cursor is over it.</summary>
    private static readonly Color ButtonHoverBackgroundColor = new(1.00f, 1.00f, 1.00f, 0.08f);

    /// <summary>The background color for a clickable button while the player is clicking it.</summary>
    private static readonly Color ButtonActiveBackgroundColor = new(1.00f, 1.00f, 1.00f, 0.14f);

    /// <summary>The color for divider lines.</summary>
    private static readonly Color DividerColor = new(1.00f, 1.00f, 1.00f, 0.12f);

    /****
    ** Mod state
    **   (set in Initialize)
    ****/
    /// <summary>Provides utility methods for reading input and showing UI.</summary>
    private InteractionHelper InteractionHelper = null!;

    /// <summary>Load the save data from disk. This should create a fresh instance that isn't referenced anywhere else.</summary>
    private Func<SaveModel> LoadData = null!;

    /// <summary>Save data back to disk and refresh any visible overlays.</summary>
    private Action<SaveModel> SaveData = null!;

    /// <summary>Get the keybind for a destination index, if it has one.</summary>
    private Func<int, KeyCode?> GetSlotKey = null!;

    /// <summary>Interactively fast travel to a destination index.</summary>
    private Action<Destination> InteractivelyFastTravelImpl = null!;

    /// <summary>Get whether fast travel from the current location to a destination is allowed. This receives the departure, destination, and whether the departure is a saved destination.</summary>
    private Func<Destination, Destination, bool, bool> CanFastTravel = null!;

    /****
    ** GUI styles
    **   (initialized in the first OnGUI call)
    ****/
    /// <summary>Whether the styles and textures need to be reinitialized, even if they're still set.</summary>
    private bool MustReinitializeStyles = true;

    /// <summary>The GUI style for the panel title text.</summary>
    private GUIStyle? StyleTitle;

    /// <summary>The GUI style for a keybind in the list.</summary>
    private GUIStyle? StyleKeybind;

    /// <summary>The GUI style for a destination name in the list.</summary>
    private GUIStyle? StyleName;

    /// <summary>The GUI style for a clickable action button in the list.</summary>
    private GUIStyle? StyleButton;

    /// <summary>The 1x1 background texture for the panel.</summary>
    private Texture2D? PanelBackground;

    /// <summary>The 1x1 background texture for a clickable button.</summary>
    private Texture2D? ButtonBackground;

    /// <summary>The 1x1 background texture for a clickable button while the cursor is over it.</summary>
    private Texture2D? ButtonHoverBackground;

    /// <summary>The 1x1 background texture for a clickable button while the player is clicking it.</summary>
    private Texture2D? ButtonActiveBackground;

    /// <summary>The 1x1 texture for a divider line.</summary>
    private Texture2D? Divider;

    /****
    ** Panel state
    ****/
    /// <summary>The normalized destinations, including any changes made by the player after the panel was opened.</summary>
    private NormalizedDestinations Destinations = null!; // set in Show

    /// <summary>A serialized representation of the original destinations loaded from disk when the panel was opened, excluding any changes made by the player after the panel was opened.</summary>
    private string OriginalDestinationsSerialized = null!; // set in Show

    /// <summary>The player's location when the panel was opened.</summary>
    private Destination CurrentLocation = null!; // set in Show

    /// <summary>The scroll position for the destination list.</summary>
    private Vector2 ScrollPos;


    /*********
    ** Accessors
    *********/
    /// <summary>Whether the panel is currently visible.</summary>
    public bool IsVisible { get; private set; }


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public DestinationManagerPanel(IntPtr pointer)
        : base(pointer) { }

    /// <summary>Create and attach the component to a persistent game object.</summary>
    public static DestinationManagerPanel Create()
    {
        var anchor = new GameObject($"{ModInfo.UniqueId}_{nameof(DestinationManagerPanel)}");
        GameObject.DontDestroyOnLoad(anchor);
        return anchor.AddComponent<DestinationManagerPanel>();
    }

    /// <summary>Initialize the panel before its first use.</summary>
    /// <param name="interactionHelper"><inheritdoc cref="InteractionHelper" path="/summary"/></param>
    /// <param name="loadData"><inheritdoc cref="LoadData" path="/summary"/></param>
    /// <param name="saveData"><inheritdoc cref="SaveData" path="/summary"/></param>
    /// <param name="getSlotKey"><inheritdoc cref="GetSlotKey" path="/summary"/></param>
    /// <param name="interactivelyFastTravel"><inheritdoc cref="InteractivelyFastTravelImpl" path="/summary"/></param>
    /// <param name="canFastTravel"><inheritdoc cref="CanFastTravel" path="/summary"/></param>
    [HideFromIl2Cpp]
    public void Initialize(InteractionHelper interactionHelper, Func<SaveModel> loadData, Action<SaveModel> saveData, Func<int, KeyCode?> getSlotKey, Action<Destination> interactivelyFastTravel, Func<Destination, Destination, bool, bool> canFastTravel)
    {
        this.InteractionHelper = interactionHelper;
        this.LoadData = loadData;
        this.SaveData = saveData;
        this.GetSlotKey = getSlotKey;
        this.InteractivelyFastTravelImpl = interactivelyFastTravel;
        this.CanFastTravel = canFastTravel;
    }

    /// <summary>Reset and open the panel.</summary>
    [MemberNotNull(nameof(Destinations), nameof(OriginalDestinationsSerialized), nameof(CurrentLocation))]
    [HideFromIl2Cpp]
    public void Show(Destination currentLocation)
    {
        Dictionary<int, Destination> originalDestinations = this.LoadData().Destinations;

        this.Destinations = new NormalizedDestinations(originalDestinations);
        this.OriginalDestinationsSerialized = this.GetSerializedRepresentation(this.Destinations);
        this.CurrentLocation = currentLocation;
        this.ScrollPos = Vector2.zero;
        this.IsVisible = true;

        // hide pause menu behind panel
        InterfaceManager.GetPanel<Panel_PauseMenu>()?.m_BasicMenuRoot?.SetActive(false);
        this.MustReinitializeStyles = true; // textures seem to break between panel sessions in some cases (e.g. after a reload), so only cache them for the current one
    }

    /// <summary>Close the panel.</summary>
    [HideFromIl2Cpp]
    public void Hide()
    {
        this.IsVisible = false;

        // restore pause menu
        InterfaceManager.GetPanel<Panel_PauseMenu>()?.m_BasicMenuRoot?.SetActive(true);
    }

    /// <summary>Handle the player pressing the escape key.</summary>
    [HideFromIl2Cpp]
    public void HandleEscape()
    {
        if (this.InteractionHelper.IsAnyPromptOpen())
            return; // let prompt get cancelled

        this.InteractivelyExit();
    }

    /// <summary>Draw the panel. Called by Unity each frame when the GUI is rendered.</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.MethodReferencedByUnity)]
    public void OnGUI()
    {
        if (!this.IsVisible || this.InteractionHelper.IsAnyPromptOpen())
            return;

        // init styles
        if (this.StyleTitle is null || this.MustReinitializeStyles)
        {
            this.PanelBackground = CreatePixel(PanelBackgroundColor);
            this.ButtonBackground = CreatePixel(ButtonBackgroundColor);
            this.ButtonHoverBackground = CreatePixel(ButtonHoverBackgroundColor);
            this.ButtonActiveBackground = CreatePixel(ButtonActiveBackgroundColor);
            this.Divider = CreatePixel(DividerColor);

            Font? font = TryGetFont(TextFontName);

            this.StyleTitle = this.CreateLabel(font, 14, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            this.StyleKeybind = this.CreateLabel(font, 13, FontStyle.Normal, TextDimColor, TextAnchor.MiddleLeft);
            this.StyleName = this.CreateLabel(font, 13, FontStyle.Normal, TextColor, TextAnchor.MiddleLeft);
            this.StyleButton = this.CreateButton(font, 12);

            this.MustReinitializeStyles = false;
        }

        // dim background
        var prevColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = prevColor;

        // draw panel background
        float x = (Screen.width - PanelWidth) / 2f;
        float y = (Screen.height - PanelHeight) / 2f;
        GUI.DrawTexture(new Rect(x, y, PanelWidth, PanelHeight), this.PanelBackground!);

        // draw panel content
        GUILayout.BeginArea(new Rect(x + 16, y + 16, PanelWidth - 32, PanelHeight - 32));
        this.DrawContent();
        GUILayout.EndArea();

        // intercept escape to prevent IMGUI handling it
        // (It'll be handled by the PanelPauseMenu patches.)
        if (Event.current is { type: EventType.KeyDown, keyCode: KeyCode.Escape })
            Event.current.Use();
    }


    /*********
    ** Private methods
    *********/
    /****
    ** Drawing
    ****/
    /// <summary>Draw the contents within the panel area.</summary>
    private void DrawContent()
    {
        // draw title
        GUILayout.Label("FAST TRAVEL DESTINATIONS", this.StyleTitle!);

        // start scroll area
        this.DrawContentDivider();
        this.ScrollPos = GUILayout.BeginScrollView(this.ScrollPos);

        // draw favorite destinations (including empty slots)
        for (int i = 0; i < ModConstants.MaxFavorites; i++)
        {
            Destination? destination = this.Destinations.FavoriteDestinations.GetValueOrDefault(i);
            KeyCode? slotKey = this.GetSlotKey(i);

            int saveIndex = i;
            this.DrawDestinationRow(destination, slotKey, canMoveUp: i > 0, canMoveDown: true, overwriteWithCurrentLocation: () => this.Destinations.SaveFavorite(saveIndex, this.CurrentLocation));
        }

        // draw remaining destinations
        for (int i = 0, lastIndex = this.Destinations.OtherDestinations.Count - 1; i <= lastIndex; i++)
        {
            Destination destination = this.Destinations.OtherDestinations[i];
            this.DrawDestinationRow(destination, null, canMoveUp: true, canMoveDown: i < lastIndex);
        }

        // draw final 'add new' button
        GUILayout.BeginHorizontal(GUILayout.Height(28));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("ADD NEW", this.StyleButton!, GUILayout.Width(190)))
            this.Destinations.SaveOther(this.CurrentLocation);
        GUILayout.EndHorizontal();

        // end scroll area
        GUILayout.EndScrollView();
        this.DrawContentDivider();

        // draw save/cancel buttons
        GUILayout.BeginHorizontal(GUILayout.Height(28));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("CANCEL", this.StyleButton!, GUILayout.Width(85)))
            this.InteractivelyExit();
        GUILayout.Space(8);
        if (GUILayout.Button("SAVE", this.StyleButton!, GUILayout.Width(85)))
        {
            this.Save();
            this.Hide();
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>Draw a divider before or after the scrollable content area.</summary>
    private void DrawContentDivider()
    {
        GUILayout.Space(4);
        Rect dividerArea = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1), GUILayout.ExpandWidth(true));
        GUI.DrawTexture(dividerArea, this.Divider!);
        GUILayout.Space(6);
    }

    /// <summary>Draw a destination slot row.</summary>
    /// <param name="destination">The destination to draw, or <c>null</c> for an empty slot.</param>
    /// <param name="slotKey">The keybind to fast travel to this destination.</param>
    /// <param name="canMoveUp">Whether the destination can be moved up in the list.</param>
    /// <param name="canMoveDown">Whether the destination can be moved down in the list.</param>
    /// <param name="overwriteWithCurrentLocation">Save the current location to this slot, if supported.</param>
    [HideFromIl2Cpp]
    private void DrawDestinationRow(Destination? destination, KeyCode? slotKey, bool canMoveUp, bool canMoveDown, Action? overwriteWithCurrentLocation = null)
    {
        // start row
        GUILayout.BeginHorizontal(GUILayout.Height(26));

        // draw keybind
        if (slotKey != null)
            GUILayout.Label($"[{slotKey}]", this.StyleKeybind!, GUILayout.Width(84));
        else
            GUILayout.Space(84);

        // draw destination info + actions
        if (destination is not null)
        {
            // draw name
            GUILayout.Label(destination.GetDisplayName(showRegion: true), this.StyleName!, GUILayout.ExpandWidth(true));

            // draw 'travel' button
            GUI.enabled = this.CanFastTravel(this.CurrentLocation, destination, this.Destinations.IsSavedScene(this.CurrentLocation.Scene.Name));
            if (GUILayout.Button("TRAVEL", this.StyleButton!, GUILayout.Width(65)))
                this.InteractivelyFastTravel(destination);
            GUI.enabled = true;

            // draw 'rename' button
            if (GUILayout.Button("RENAME", this.StyleButton!, GUILayout.Width(65)))
                this.InteractivelyRename(destination);

            // draw 'forget' button
            if (GUILayout.Button("FORGET", this.StyleButton!, GUILayout.Width(60)))
                this.InteractivelyDelete(destination);

            // draw move-up arrow
            GUI.enabled = canMoveUp;
            if (GUILayout.Button("↑", this.StyleButton!, GUILayout.Width(26)))
                this.Destinations.MoveDestination(destination, -1);

            // draw move-down arrow
            GUI.enabled = canMoveDown;
            if (GUILayout.Button("↓", this.StyleButton!, GUILayout.Width(26)))
                this.Destinations.MoveDestination(destination, 1);
            GUI.enabled = true;
        }
        else if (overwriteWithCurrentLocation != null)
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("ADD NEW", this.StyleButton!, GUILayout.Width(95)))
                overwriteWithCurrentLocation();
        }

        GUILayout.EndHorizontal();
    }

    /****
    ** Actions
    ****/
    /// <summary>Delete a saved destination with player interaction.</summary>
    /// <param name="destination">The destination to delete.</param>
    [HideFromIl2Cpp]
    private void InteractivelyDelete(Destination destination)
    {
        string question = $"Forget fast travel point '{destination.GetDisplayName()}'?";

        this.InteractionHelper.ShowConfirmDialogue(
            question,
            () => this.Destinations.Remove(destination)
        );
    }

    /// <summary>Rename a destination with player interaction.</summary>
    /// <param name="destination">The destination to rename.</param>
    [HideFromIl2Cpp]
    private void InteractivelyRename(Destination destination)
    {
        this.InteractionHelper.ShowTextDialogue(
            question: "Rename destination (or blank to reset)",
            initialValue: destination.GetDisplayName(showRegion: true),
            onConfirm: newName =>
            {
                destination.CustomName = string.IsNullOrWhiteSpace(newName)
                    ? null
                    : newName.Trim();
            }
        );
    }

    /// <summary>Fast travel to a destination with player interaction.</summary>
    /// <param name="destination">The destination to travel to.</param>
    [HideFromIl2Cpp]
    private void InteractivelyFastTravel(Destination destination)
    {
        if (this.HasUnsavedChanges())
        {
            this.InteractionHelper.ShowConfirmDialogue(
                "You have unsaved changes. Do you want to discard them?",
                () =>
                {
                    this.InteractionHelper.ForceClosePrompt(); // exit prompt immediately, so we can show the fast travel prompt
                    this.InteractivelyFastTravelImpl(destination);
                }
            );
        }
        else
            this.InteractivelyFastTravelImpl(destination);
    }

    /// <summary>Close the panel, with player interaction if there are unsaved changes.</summary>
    [HideFromIl2Cpp]
    private void InteractivelyExit()
    {
        if (this.HasUnsavedChanges())
            this.InteractionHelper.ShowConfirmDialogue("Discard your unsaved changes?", this.Hide);
        else
            this.Hide();
    }

    /// <summary>Persist the destinations to disk.</summary>
    [HideFromIl2Cpp]
    private void Save()
    {
        var data = new SaveModel
        {
            ReturnPoint = this.LoadData().ReturnPoint,
            Version = ModInfo.Version
        };

        foreach ((int index, Destination destination) in this.Destinations.FavoriteDestinations)
            data.Destinations[index] = destination;

        for (int i = 0; i < this.Destinations.OtherDestinations.Count; i++)
            data.Destinations[i + ModConstants.MaxFavorites] = this.Destinations.OtherDestinations[i];

        this.SaveData(data);
    }

    /// <summary>Returns whether the working copy differs from the data that was loaded when the panel opened.</summary>
    [HideFromIl2Cpp]
    private bool HasUnsavedChanges()
    {
        return this.GetSerializedRepresentation(this.Destinations) != this.OriginalDestinationsSerialized;
    }

    /// <summary>Get a serialized representation of a destination list for change detection.</summary>
    [HideFromIl2Cpp]
    private string GetSerializedRepresentation(NormalizedDestinations destinations)
    {
        var data = new
        {
            Favorites = destinations.FavoriteDestinations.OrderBy(p => p.Key),
            Others = destinations.OtherDestinations
        };

        return JsonSerializer.Serialize(data);
    }


    /****
    ** Style logic
    ****/
    /// <summary>Create a label style.</summary>
    /// <param name="font">The font to apply.</param>
    /// <param name="size">The font size.</param>
    /// <param name="fontStyle">The font style.</param>
    /// <param name="color">The text color.</param>
    /// <param name="anchor">The text alignment within its bounding area.</param>
    private GUIStyle CreateLabel(Font? font, int size, FontStyle fontStyle, Color color, TextAnchor anchor)
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = size,
            fontStyle = fontStyle,
            alignment = anchor,
            normal =
            {
                textColor = color
            }
        };

        if (font != null)
            style.font = font;

        return style;
    }

    /// <summary>Create a clickable button style.</summary>
    /// <param name="font">The font to apply.</param>
    /// <param name="size">The font size.</param>
    private GUIStyle CreateButton(Font? font, int size)
    {
        var style = new GUIStyle(GUI.skin.button)
        {
            fontSize = size,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(6, 6, 4, 4),
            border = new RectOffset(1, 1, 1, 1),
            normal =
            {
                textColor = TextColor,
                background = this.ButtonBackground
            },
            hover =
            {
                textColor = TextHoverColor,
                background = this.ButtonHoverBackground
            },
            active =
            {
                textColor = TextHoverColor,
                background = this.ButtonActiveBackground
            },
            focused =
            {
                textColor = TextHoverColor,
                background = this.ButtonBackground
            }
        };

        if (font != null)
            style.font = font;

        return style;
    }

    /// <summary>Create a 1×1 solid-color texture.</summary>
    /// <param name="color">The texture color.</param>
    private static Texture2D CreatePixel(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    /// <summary>Get a text font, if it exists.</summary>
    /// <param name="name">The name of the font to get.</param>
    private static Font? TryGetFont(string name)
    {
        try
        {
            foreach (Font font in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (font.name == name)
                    return font;
            }
        }
        catch
        {
            // use default font
        }

        return null;
    }
}
