using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGScrollerGameScript : MonoBehaviour
{
    private Transform ball;

    private Gradient gradient;

    private Vector3 blueGoalPos;
    private Vector3 redGoalPos;
    private Vector3 greenGoalPos;

    public RawImage img;

    public RawImage bar;

    Material[] mats;
    private Material mat;

    Color col;

    Vector3 directionOfGoal;

    public Texture testThing;

    private void Start()
    {
        img = GetComponentInChildren<RawImage>();
    }

    public void Setup()
    {
        mats = GameManager.instance.playerMaterials;

        if (GameManager.instance.menuGame)
        {
            img.material = mats[0];
            Color col = Color.white;
            col.a = 0.4f;
            //col.a = 1f;
            //img.texture = testThing;

            img.color = col;
            Destroy(this);
            //Destroy(gameObject);
        }
        else
        {
            GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        }

        gradient = new Gradient();

        GradientColorKey[] colorKey = new GradientColorKey[2];

        colorKey[0].color = Color.black;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.white;
        colorKey[1].time = 1f;

        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        for (int i = 0; i < alphaKey.Length; i++)
        {
            alphaKey[i].alpha = 0.2f;
            alphaKey[i].time = i;
        }

        //Material[] mats = GameManager.instance.playerMaterials;
        //GradientColorKey[] colorKey = new GradientColorKey[3];

        //colorKey[0].color = mats[1].color;
        //colorKey[0].time = 0.0f;
        //colorKey[1].color = Color.black;
        //colorKey[1].time = 0.5f;
        //colorKey[2].color = mats[0].color;
        //colorKey[2].time = 1f;

        //GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
        //for (int i = 0; i < alphaKey.Length; i++)
        //{
        //    alphaKey[i].alpha = 0.2f;
        //    alphaKey[i].time = 0.5f * i;
        //}

        gradient.SetKeys(colorKey, alphaKey);

        blueGoalPos = GameObject.Find("Blue Goal").transform.position;
        redGoalPos = GameObject.Find("Red Goal").transform.position;
        GameObject greenGoal = GameObject.Find("Green Goal");
        if (greenGoal != null) greenGoalPos = greenGoal.transform.position;

        col = gradient.Evaluate(0f);

        GameManager.OnGoalScore += OnGoalScore;
    }

    private void Update()
    {
        if (ball == null)
        {
            BallManager ballManager = FindObjectOfType<BallManager>();
            if (ballManager != null) ball = ballManager.transform;
        }
        else
        {
            if (redGoalPos.x == 0f || gradient == null)
            {
                Color col = Color.black;
                col.a = 0.2f;
                img.color = col;
            }
            else
            {
                float pos = ball.position.x / (2 * (redGoalPos.x - 1)) + 0.5f;

                if (!UltimateReplay.ReplayManager.IsReplayingAny)
                {
                    col = GetColour();
                }

                img.color = col;
            }
        }
    }

    private Color GetColour()
    {
        

        Vector2 closestNetPos = blueGoalPos;
        float closestNetDist = (ball.position - blueGoalPos).magnitude;
        mat = mats[1];
        float secondClosestNetDist;

        float distToRed = (ball.position - redGoalPos).magnitude;
        if (distToRed < closestNetDist)
        {
            secondClosestNetDist = closestNetDist;
            closestNetDist = distToRed;
            closestNetPos = redGoalPos;
            mat = mats[0];
        }
        else
        {
            secondClosestNetDist = distToRed;
        }

        if (GameInfo.numberOfPlayers == 3)
        {
            float distToGreen = (ball.position - greenGoalPos).magnitude;
            if (distToGreen < closestNetDist)
            {
                secondClosestNetDist = closestNetDist;
                closestNetDist = distToGreen;
                closestNetPos = greenGoalPos;
                mat = mats[2];
            }
            else if (distToGreen < secondClosestNetDist)
            {
                secondClosestNetDist = distToGreen;
            }
        }

        float difference = secondClosestNetDist - closestNetDist;

        float gradPos = difference / ((blueGoalPos - redGoalPos).magnitude / 1.5f);

        directionOfGoal = closestNetPos.normalized;

        //Debug.Log("difference:  " + difference + "  magnitude: " + ((blueGoalPos - redGoalPos).magnitude) + "  gradient pos: " + gradPos);

        img.material = mat;

        return gradient.Evaluate(gradPos);
    }

    

    private void OnGoalScore(GameManager.Teams scorer, GameManager.Teams scoredAgainst)
    {
        StartCoroutine(Wave());
    }

    private IEnumerator Wave()
    {
        bar.material = mat;

        float rotation = Mathf.Atan2(directionOfGoal.y, directionOfGoal.x) * Mathf.Rad2Deg;
        bar.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

        bar.rectTransform.localPosition = directionOfGoal * (Screen.width / 2f + bar.rectTransform.sizeDelta.x);
        while (-Mathf.Sign(directionOfGoal.x) * bar.rectTransform.localPosition.x < (Screen.width / 2f + bar.rectTransform.sizeDelta.x)
            /*&& Mathf.Sign(directionOfGoal.y) * bar.rectTransform.localPosition.x < (Screen.width / 2f + bar.rectTransform.sizeDelta.x)*/)
        {
            bar.rectTransform.localPosition += -directionOfGoal * Time.unscaledDeltaTime * 4000f;
            yield return null;
        }
    }

    private void OnDestroy()
    {
        GameManager.OnGoalScore -= OnGoalScore;
    }
}
