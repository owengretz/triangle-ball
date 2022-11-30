using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsTooltip : MonoBehaviour, IPointerEnterHandler
{
    public string tooltipText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SettingsTooltipManager.instance.ChangeTooltip(tooltipText);
    }
}
