using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCursor : MonoBehaviour
{
    RectTransform trans;

    private void Start()
    {
        trans = GetComponent<RectTransform>();
    }
    private void Update()
    {
        transform.position = Input.mousePosition;
    }
}
