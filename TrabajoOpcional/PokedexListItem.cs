using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PokedexListItem : MonoBehaviour
{
    public TMP_Text label;
    public Image lockIcon;      
    public Image typeIcon;      

    public void SetCaptured(int pokemonId, string name, bool captured, Sprite typeSprite = null)
    {
        if (label)
        {
            if (captured) label.text = $"{pokemonId:00}  {name}";
            else label.text = $"{pokemonId:00}  ???";
        }

        if (lockIcon) lockIcon.enabled = !captured;

        if (typeIcon)
        {
            typeIcon.sprite = (captured ? typeSprite : null);
            typeIcon.enabled = (captured && typeSprite != null);
        }
    }
}
