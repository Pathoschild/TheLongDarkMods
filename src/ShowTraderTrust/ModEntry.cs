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
        TraderRadio? radio = SceneHelper.IsPlayableScene() && !InterfaceManager.IsPanelEnabled<Panel_PauseMenu>()
            ? (this.TraderRadio ??= new Lazy<TraderRadio?>(Object.FindObjectOfType<TraderRadio>)).Value
            : null;

        switch (radio?.m_CurrentState)
        {
            case RadioState.PlayingVoiceLine:     // trader speaking
            case RadioState.ShowingConversations: // showing conversation lists
            case RadioState.ShowingCancel:        // showing 'cancel trade' confirmation
            case RadioState.ShowingMain:          // showing main menu (trade, end call, etc.)
            case RadioState.Trading:              // selecting trades
                {
                    TraderManager trader = GameManager.GetTraderManager();
                    this.TrustOverlay.Show($"Trust level: {trader.CurrentTrust} / {trader.MaxTrust}");
                }
                break;

            default:
                this.Clear();
                break;
        }
    }

    /// <inheritdoc />
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        this.Clear();
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Clear all trader state and hide the overlay.</summary>
    private void Clear()
    {
        this.TraderRadio = null;
        this.TrustOverlay.Hide();
    }
}
