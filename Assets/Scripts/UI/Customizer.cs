using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UC;

[RequireComponent(typeof(PlayerInput))]
public class Customizer : UIGroup
{
    [SerializeField] Image                      thumbnailImage;
    [SerializeField] UIImageEffect              uiEffect;
    [SerializeField] ColorPalette               originalPalette;
    [SerializeField] UIDiscreteSubDataSelector  modelSelector;
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

        modelSelector.onChange += OnModelChange;
        hullColorSelector.onChange += OnColorChange;
        stripeColorSelector.onChange += OnColorChange;
        cockpitColorSelector.onChange += OnColorChange;
        continueButton.onInteract += OnContinue;

        OnModelChange(null);
    }

    private void OnModelChange(BaseUIControl control)
    {
        thumbnailImage.sprite = modelSelector.value.primarySprite;
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
            submarine = modelSelector.value,
            hullColor = hullColorSelector.value,
            stripeColor = stripeColorSelector.value,
            cockpitColor = cockpitColorSelector.value,
        };

        GameManager.Instance.SetPlayerData(playerId, pd);
    }
}
