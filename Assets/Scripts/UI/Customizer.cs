using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Customizer : UIGroup
{
    [SerializeField] UIImageEffect              uiEffect;
    [SerializeField] ColorPalette               originalPalette;
    [SerializeField] UIDiscreteColorSelector    hullColorSelector;
    [SerializeField] UIDiscreteColorSelector    stripeColorSelector;
    [SerializeField] UIDiscreteColorSelector    cockpitColorSelector;
    [SerializeField] UIButton                   continueButton;

    ColorPalette palette;

    protected override void Start()
    {
        if (GameManager.Instance.numPlayers <= playerId)
        {
            Destroy(gameObject);
            return;
        }

        base.Start();

        palette = originalPalette.Clone();

        hullColorSelector.onChange += OnColorChange;
        stripeColorSelector.onChange += OnColorChange;
        cockpitColorSelector.onChange += OnColorChange;
        continueButton.onInteract += OnContinue;

        OnColorChange(null);
    }

    private void OnColorChange(BaseUIControl control)
    {
        palette = SubCustomization.BuildPalette(originalPalette, hullColorSelector.value, stripeColorSelector.value, cockpitColorSelector.value);
        palette.name = $"Palette Player {playerId}";

        uiEffect.SetRemap(palette);
    }

    void OnContinue(BaseUIControl control)
    {
        selectedControl = null;
        SetUI(false);
        continueButton.gameObject.SetActive(false);

        PlayerInput playerInput = GetComponent<PlayerInput>();
        var devices = playerInput.devices[0];        

        var pd = new GameManager.PlayerData
        {
            hullColor = hullColorSelector.value,
            stripeColor = stripeColorSelector.value,
            cockpitColor = cockpitColorSelector.value,
        };

        GameManager.Instance.SetPlayerData(playerId, pd);
    }
}
