using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TelekinesisPickUp : MonoBehaviour
{
    [SerializeField]
    private GameObject emptySkillPlace;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name);
        AbilityManager abilityManager = collision.gameObject.GetComponent<AbilityManager>();
        if (abilityManager)
        {
            abilityManager.UnlockAbility(collision.gameObject.GetComponent<Telekinesis>());
            emptySkillPlace.SetActive(true);
            transform.parent.gameObject.SetActive(false);
        }
    }
}