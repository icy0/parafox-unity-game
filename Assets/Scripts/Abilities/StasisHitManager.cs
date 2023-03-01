using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StasisHitManager : MonoBehaviour
{
    Rigidbody2D Rigidbody;

    [SerializeField]
    private const float StasisDuration = 2.0f;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
    }

    public void StasisHit()
    {
        StartCoroutine("Freeze");
    }

    private IEnumerator Freeze()
    {
        // TODO Preserve Motion
        Vector2 Velocity = Rigidbody.velocity;
        Rigidbody.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        yield return new WaitForSeconds(StasisDuration);

        // TODO Apply Preserved Motion
        Rigidbody.constraints = RigidbodyConstraints2D.None;
        Rigidbody.AddForce(Velocity * 90.0f);
    }
}
