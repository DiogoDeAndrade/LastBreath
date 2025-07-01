using UnityEngine;
using UnityEngine.UI;

public class StatDisplay : MonoBehaviour
{
    [SerializeField] Sprite     activeImage;
    [SerializeField] Sprite     inactiveImage;
    [SerializeField] Image[]    images;

    int _value = 0;

    public int Value
    {
        get { return _value; }
        set { _value = value; }
    }

    void Update()
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (i < _value)
            {
                images[i].sprite = activeImage;
                images[i].gameObject.SetActive(true);
            }
            else
            {
                images[i].sprite = inactiveImage;
                if (inactiveImage == null)
                {
                    images[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
