using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsTooltipManager : MonoBehaviour
{

    public static SettingsTooltipManager instance;

    private TMPro.TMP_Text tooltipText;

    private void Awake()
    {
        tooltipText = GetComponentInChildren<TMPro.TMP_Text>();
        instance = this;
    }

    public void ChangeTooltip(string text)
    {
        tooltipText.text = text;
    }
}
