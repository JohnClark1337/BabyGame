using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceScript : MonoBehaviour
{
    public float initialSpeed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        var direction = (transform.right + transform.up).normalized;
        rb.linearVelocity = direction * initialSpeed;
    }
}
