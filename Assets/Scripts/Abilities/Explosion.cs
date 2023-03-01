using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    CircleCollider2D RangeTrigger;

    [SerializeField]
    float ExplosionPower = 2000.0f;

    private void Start()
    {
        RangeTrigger = transform.Find("Range").GetComponent<CircleCollider2D>();
        StartCoroutine("BombExplosion");
    }

    private IEnumerator BombExplosion()
    {
        yield return new WaitForSeconds(2.5f);

        List<GameObject> ListOfDestructibles = RangeTrigger.GetComponent<DestructibleFinder>().GetDestructiblesInRange();
        List<GameObject> ListOfMoveables = RangeTrigger.GetComponent<DestructibleFinder>().GetMoveablesInRange();

        for (int DestructibleIndex = 0; DestructibleIndex < ListOfDestructibles.Count; DestructibleIndex++)
        {
            Destroy(ListOfDestructibles[DestructibleIndex]);
        }

        for(int MoveableIndex = 0; MoveableIndex < ListOfMoveables.Count; MoveableIndex++)
        {
            ListOfMoveables[MoveableIndex].GetComponent<Rigidbody2D>().AddForce((ListOfMoveables[MoveableIndex].transform.position - transform.position).normalized * ExplosionPower);
        }
        Destroy(gameObject);
    }
}

