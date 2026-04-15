using System;
using System.Diagnostics.CodeAnalysis;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

/// <summary>Provides utility methods for reading input and showing UI.</summary>
internal static class InteractionHelper
{
    /*********
    ** Public methods
    *********/
    /// <summary>Get whether a keyboard button is currently pressed.</summary>
    /// <param name="key">The keyboard button to check.</param>
    public static bool IsKeyDown(KeyCode key)
    {
        return InputManager.GetKeyDown(InputManager.m_CurrentContext, key);
    }

    /// <summary>Show a message box which lets the player confirm, but not cancel.</summary>
    /// <param name="message">The text to display.</param>
    /// <param name="onConfirm">The action to perform when the player confirms.</param>
    public static void ShowMessageBox(string message, Action? onConfirm = null)
    {
        if (!TryGetUnusedConfirmationPanel(out Panel_Confirmation? panel))
            return;

        panel.ShowErrorMessage(
            text: message,
            confirmCallback: onConfirm
        );
    }

    /// <summary>Show a confirmation dialogue box which lets the player confirm or cancel.</summary>
    /// <param name="question">The question text to display.</param>
    /// <param name="onConfirm">The action to perform when the player confirms.</param>
    public static void ShowConfirmDialogue(string question, Action onConfirm)
    {
        if (!TryGetUnusedConfirmationPanel(out Panel_Confirmation? panel))
            return;

        panel.ShowConfirmPanel(
            locID: question,
            buttonPromptLocId1: "Yes",
            buttonPromptLocId2: "No",
            confirmCallback: onConfirm,
            cancelCallback: null
        );
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Try to get an unused confirmation panel.</summary>
    /// <param name="panel">The confirmation panel, if available.</param>
    private static bool TryGetUnusedConfirmationPanel([NotNullWhen(true)] out Panel_Confirmation? panel)
    {
        // get panel
        panel = InterfaceManager.GetPanel<Panel_Confirmation>();
        if (panel is null)
        {
            MelonLogger.Warning($"Can't show confirmation dialogue: {nameof(Panel_Confirmation)} not found.");
            return false;
        }

        // skip if it's already open
        if (panel.isActiveAndEnabled)
        {
            panel = null;
            return false;
        }

        // valid
        return true;
    }
}
