using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class AnimatedBallIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltip;
    public TMP_Text text;

    [HideInInspector] public int ballIndex;

    private readonly string[] tooltipText = { "Looks at the nearest player", "Changes colour on contact", "Displays the time" };


    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.SetActive(true);

        text.text = tooltipText[ballIndex];
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.SetActive(false);
    }
}
