using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class MultiGraphicColorHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Graphic[] graphicsToTint;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var g in graphicsToTint)
        {
            g.color = highlightColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (var g in graphicsToTint)
        {
            g.color = normalColor;
        }
    }
}
