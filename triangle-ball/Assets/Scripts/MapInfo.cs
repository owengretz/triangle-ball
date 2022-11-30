using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "MapData", menuName = "MapInfo")]
public class MapInfo : ScriptableObject
{
    public SceneObject scene;
    public Sprite thumbnail;
    public bool botsAllowed;
    public string[] playersSupported;
    public float cameraZoom;
}
