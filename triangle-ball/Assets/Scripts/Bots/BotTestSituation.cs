using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotTestSituation : MonoBehaviour
{
    public Transform ball;
    public Vector2 ballVelocity;
    public Transform bot;
    public Vector2 botVelocity;
    public Transform opponent;
    public Vector2 opponentVelocity;
    public int boostAmount = 1;

    private void Awake()
    {
        ball.gameObject.SetActive(false);
        bot.gameObject.SetActive(false);
        opponent.gameObject.SetActive(false);
    }
}
