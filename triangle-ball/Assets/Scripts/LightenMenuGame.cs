using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightenMenuGame : MonoBehaviour
{
    [HideInInspector] public GameObject copy;

    private void Start()
    {
        if (GameManager.instance.menuGame)
        {
            copy = Instantiate(gameObject);
            Destroy(copy.GetComponent<LightenMenuGame>());

            SpriteRenderer rend = copy.GetComponent<SpriteRenderer>();
            
            Color col = rend.color;
            col.a = 0.4f;
            rend.color = col;

            rend.sortingLayerName = "Front";

            Shader textShader = Shader.Find("GUI/Text Shader");
            rend.material.shader = textShader;
        }
    }
}
