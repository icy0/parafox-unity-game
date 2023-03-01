using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stasis : MonoBehaviour, IAbility
{
    Player Player;

    [SerializeField]
    private const float Cooldown = 3.0f;

    void Start()
    {
        Player = GetComponent<Player>();
    }

    public void ExecuteAbility()
    {
        Vector2 WorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D Hit = Physics2D.Raycast(WorldPoint, Vector2.zero);
        if (Hit.collider != null)
        {
            Transform HitGameObject = Hit.transform;

            Debug.Log("Stasis-Hit on " + HitGameObject);

            StasisHitManager HitManager;
            if ((HitManager = HitGameObject.GetComponent<StasisHitManager>()) != null)
            {
                HitManager.StasisHit();
            }
        }
    }

    public string getAbilityName()
    {
        return "Stasis";
    }

    public void AccumulateAbility()
    {
        // Can't be executed accumulatively
    }

    public void ReleaseAbility()
    {
        // Can't be executed accumulatively
    }
}
