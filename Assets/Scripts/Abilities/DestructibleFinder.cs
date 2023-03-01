using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleFinder : MonoBehaviour
{
    private List<GameObject> DestructiblesInRange;
    private List<GameObject> MoveablesInRange;

    private void Start()
    {
        DestructiblesInRange = new List<GameObject>();
        MoveablesInRange = new List<GameObject>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Destructible>())
        {
            DestructiblesInRange.Add(collision.gameObject);
        }
        else if((collision.gameObject.GetComponent<Rigidbody2D>()) && collision.gameObject.tag != "Player")
        {
            MoveablesInRange.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (DestructiblesInRange.Contains(collision.gameObject))
        {
            DestructiblesInRange.Remove(collision.gameObject);
        }
        else if (MoveablesInRange.Contains(collision.gameObject))
        {
            MoveablesInRange.Remove(collision.gameObject);
        }
    }

    public List<GameObject> GetDestructiblesInRange()
    {
        return DestructiblesInRange;
    }

    public List<GameObject> GetMoveablesInRange()
    {
        return MoveablesInRange;
    }
}
