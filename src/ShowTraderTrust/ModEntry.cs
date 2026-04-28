using System;
using Il2Cpp;
using Il2CppTLD.Trader;
using MelonLoader;
using Pathoschild.TheLongDarkMods.Common;
using Pathoschild.TheLongDarkMods.ShowTraderTrust.Framework;
using Object = UnityEngine.Object;
using RadioState = Il2CppTLD.Trader.TraderRadio.RadioState;

namespace Pathoschild.TheLongDarkMods.ShowTraderTrust;

/// <inheritdoc />
public class ModEntry : MelonMod
{
    /*********
    ** Fields
    *********/
    /// <summary>The trader radio in the current scene, if any.</summary>
    private Lazy<TraderRadio?>? TraderRadio;

    /// <summary>An overlay which shows the current trust level.</summary>
    private TraderTrustOverlay TrustOverlay = null!; // set in OnInitializeMelon

    /// <summary>The last radio state for which the overlay was updated.</summary>
    private RadioState LastRadioState = RadioState.Off;

    /// <summary>The last trust level for which the overlay was updated.</summary>
    private int? LastTrust;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void OnInitializeMelon()
    {
        this.TrustOverlay = TraderTrustOverlay.Create();
    }

    /// <inheritdoc />
    public override void OnUpdate()
    {
        // skip if not applicable
        if (!SceneHelper.IsPlayableScene())
        {
            this.Clear(forgetRadio: true);
            return;
        }
        if (InterfaceManager.IsPanelEnabled<Panel_PauseMenu>())
        {
            this.Clear(forgetRadio: false);
            return;
        }

        // update overlay
        RadioState radioState = this.GetRadioState();
        switch (radioState)
        {
            case RadioState.PlayingVoiceLine:     // trader speaking
            case RadioState.ShowingConversations: // showing conversation lists
            case RadioState.ShowingCancel:        // showing 'cancel trade' confirmation
            case RadioState.ShowingMain:          // showing main menu (trade, end call, etc.)
            case RadioState.Trading:              // selecting trades
                {
                    TraderManager trader = GameManager.GetTraderManager();
                    int trust = trader.CurrentTrust;

                    if (radioState != this.LastRadioState || trust != this.LastTrust)
                    {
                        this.TrustOverlay.Show($"Trust level: {trust} / {trader.MaxTrust}");

                        this.LastRadioState = radioState;
                        this.LastTrust = trust;
                    }
                }
                break;

            default:
                this.Clear(forgetRadio: false);
                break;
        }
    }

    /// <inheritdoc />
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        this.Clear(forgetRadio: true);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the radio state in the current scene.</summary>
    private RadioState GetRadioState()
    {
        TraderRadio? radio = (this.TraderRadio ??= new Lazy<TraderRadio?>(Object.FindObjectOfType<TraderRadio>)).Value;
        return radio?.m_CurrentState ?? RadioState.Off;
    }

    /// <summary>Clear all trader state and hide the overlay.</summary>
    /// <param name="forgetRadio">Whether to reset the detected radio reference.</param>
    private void Clear(bool forgetRadio)
    {
        if (forgetRadio)
            this.TraderRadio = null;

        if (this.TrustOverlay.IsVisible)
            this.TrustOverlay.Hide();

        this.LastRadioState = RadioState.Off;
        this.LastTrust = null;
    }
}
