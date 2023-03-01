using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnNewOnImpact : MonoBehaviour
{
    private BoxCollider2D Collider;
    private Vector2 SpawnPosition;

    [SerializeField]
    private GameObject Prefab;

    void Start()
    {
        Collider = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        SpawnPosition = transform.position;
        SpawnPosition.y += 5.0f;

        Instantiate(Prefab, SpawnPosition, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
