using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autodestruct : MonoBehaviour
{
    void Start()
    {
        StartCoroutine("AutodestructRoutine");
    }

    private IEnumerator AutodestructRoutine()
    {
        yield return new WaitForSeconds(1);

        Destroy(gameObject);
    }
}
