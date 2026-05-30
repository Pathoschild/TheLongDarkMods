using System;
using System.Diagnostics.CodeAnalysis;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.Common;

/// <summary>Provides utility methods for reading input and showing UI.</summary>
internal class InteractionHelper
{
    /*********
    ** Fields
    *********/
    /// <summary>The log instance.</summary>
    private readonly MelonLogger.Instance Log;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="log">The log instance.</param>
    public InteractionHelper(MelonLogger.Instance log)
    {
        this.Log = log;
    }

    /// <summary>Get whether a keyboard button is currently pressed.</summary>
    /// <param name="key">The keyboard button to check.</param>
    public bool IsKeyDown(KeyCode key)
    {
        return Input.GetKey(key);
    }

    /// <summary>Get whether a keyboard button was first pressed this tick. This returns false if the key was held down since a previous tick.</summary>
    /// <param name="key">The keyboard button to check.</param>
    public bool IsKeyJustPressed(KeyCode key)
    {
        return InputManager.GetKeyDown(InputManager.m_CurrentContext, key);
    }

    /// <summary>Show a message box which lets the player confirm, but not cancel.</summary>
    /// <param name="message">The text to display.</param>
    /// <param name="onConfirm">The action to perform when the player confirms.</param>
    public void ShowMessageBox(string message, Action? onConfirm = null)
    {
        if (!this.TryGetUnusedConfirmationPanel(out Panel_Confirmation? panel))
            return;

        panel.ShowErrorMessage(
            text: message,
            confirmCallback: onConfirm
        );
    }

    /// <summary>Show a confirmation dialogue box which lets the player confirm or cancel.</summary>
    /// <param name="question">The question text to display.</param>
    /// <param name="onConfirm">The action to perform when the player confirms.</param>
    public void ShowConfirmDialogue(string question, Action onConfirm)
    {
        if (!this.TryGetUnusedConfirmationPanel(out Panel_Confirmation? panel))
            return;

        panel.ShowConfirmPanel(
            locID: question,
            buttonPromptLocId1: "Yes",
            buttonPromptLocId2: "No",
            confirmCallback: onConfirm,
            cancelCallback: null
        );
    }

    /// <summary>Show a textbox dialogue box which lets the user enter text, and confirm or cancel.</summary>
    /// <param name="question">The question to display.</param>
    /// <param name="initialValue">The initial textbox value.</param>
    /// <param name="onConfirm">Handle the player clicking accept. This receives the textbox content.</param>
    /// <param name="allowLowerCase">Whether to let the player enter lowercase text, instead of the ALL CAPS format preferred by the game.</param>
    /// <param name="maxLength">The maximum text length to allow.</param>
    public void ShowTextDialogue(string question, string initialValue, Action<string> onConfirm, bool allowLowerCase = true, uint maxLength = 100)
    {
        if (!this.TryGetUnusedConfirmationPanel(out Panel_Confirmation? panel))
            return;

        panel.SetupInputField(virtualKeyboardDescriptionLocId: "", maxLength: maxLength, capsLock: !allowLowerCase);
        panel.ShowRenamePanel(
            locID: question,
            currentName: initialValue,
            buttonPromptLocId1: "GAMEPLAY_Accept",
            buttonPromptLocId2: "GAMEPLAY_Cancel",
            confirmCallback: (Action)(() =>
            {
                string newText = panel.GetInputFieldText();
                onConfirm(newText);
            }),
            enableCallback: null
        );
    }

    /// <summary>Force the current message/confirmation/textbox prompt to exit, if any.</summary>
    public void ForceClosePrompt()
    {
        var panel = InterfaceManager.GetPanel<Panel_Confirmation>();
        panel?.CloseSelf();
    }

    /// <summary>Get whether a message/confirmation/textbox prompt is currently displayed.</summary>
    public bool IsAnyPromptOpen()
    {
        return InterfaceManager.GetPanel<Panel_Confirmation>()?.isActiveAndEnabled is true;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Try to get an unused confirmation panel.</summary>
    /// <param name="panel">The confirmation panel, if available.</param>
    private bool TryGetUnusedConfirmationPanel([NotNullWhen(true)] out Panel_Confirmation? panel)
    {
        // get panel
        panel = InterfaceManager.GetPanel<Panel_Confirmation>();
        if (panel is null)
        {
            this.Log.Warning($"Can't show confirmation dialogue: {nameof(Panel_Confirmation)} not found.");
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
