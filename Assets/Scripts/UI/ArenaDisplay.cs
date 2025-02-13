using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArenaDisplay : MonoBehaviour
{
    public Arena arenaDef;

    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private Image           thumbnailImage;
    [SerializeField]
    private TextMeshProUGUI descriptionText;

    void Start()
    {
        if (arenaDef == null)
        {
            gameObject.SetActive(false);
            return;
        }
        titleText.text = arenaDef.displayName;
        descriptionText.text = arenaDef.description;
        thumbnailImage.sprite = arenaDef.thumbnail;
    }
}
