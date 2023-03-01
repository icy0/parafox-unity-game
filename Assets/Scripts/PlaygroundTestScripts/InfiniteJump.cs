using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteJump : MonoBehaviour
{
    Rigidbody2D Rigidbody;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        StartCoroutine("Jump");
    }

    private IEnumerator Jump()
    {
        while(true)
        {
            yield return new WaitForSeconds(3.0f);
            Rigidbody.AddForce(new Vector2(0.0f, 4000.0f));
        }
    }
}
