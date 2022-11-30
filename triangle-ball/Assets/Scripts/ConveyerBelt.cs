using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerBelt : MonoBehaviour
{
    [Range(-1, 1)]
    public int direction;

    public float moveSpeed = 1f;

    private Dictionary<Rigidbody2D, Vector3> positions = new Dictionary<Rigidbody2D, Vector3>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

        if (rb == null)
            return;

        positions[rb] = rb.transform.position;
    }

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

    //    if (rb == null)
    //        return;

    //    rb.transform.position = rb.transform.position + Vector3.right * moveSpeed * direction * Time.deltaTime;
    //}

    private void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

        if (rb == null)
            return;

        rb.velocity = (rb.transform.position - positions[rb]) / Time.fixedDeltaTime;
        positions.Remove(rb);
    }


    private void FixedUpdate()
    {
        foreach (Rigidbody2D rb in new List<Rigidbody2D>(positions.Keys))
        {
            positions[rb] = rb.transform.position;
            rb.transform.position = rb.transform.position + Vector3.right * moveSpeed * direction * Time.fixedDeltaTime;
        }





        transform.position = transform.position + Vector3.right * moveSpeed * direction * Time.fixedDeltaTime;
        if (transform.position.x <= 3f * direction)
        {
            transform.position = new Vector2(0f, transform.position.y);
        }
    }
}
