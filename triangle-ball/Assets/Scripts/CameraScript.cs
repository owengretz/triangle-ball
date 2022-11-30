using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Camera>().orthographicSize = GameInfo.mapInfo.cameraZoom;

        BGScroller scrollingBG = FindObjectOfType<BGScroller>();
        if (scrollingBG != null)
        {
            scrollingBG.GetComponent<Canvas>().worldCamera = GetComponent<Camera>();
        }
    }
}
