using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformPickUp : MonoBehaviour
{
    [SerializeField]
    private GameObject emptySkillPlace;
  
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name);
        AbilityManager abilityManager = collision.gameObject.GetComponent<AbilityManager>();
        if (abilityManager)
        {
            
            abilityManager.UnlockAbility(collision.gameObject.GetComponent<PlatformSpawn>());
            emptySkillPlace.SetActive(true);
            transform.parent.gameObject.SetActive(false);
        }
    }
}
