using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;

public class Telekinesis : MonoBehaviour, IAbility
{
    private Player player;
    private Transform currentTelekinetable;

    [SerializeField]
    private GameObject rayPrefab;

    private Transform ray;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    public String getAbilityName()
    {
        return "Telekinesis";
    }

    private bool IsTelekinetable(bool isFirstQuery)
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        List<RaycastHit2D> playerHits = new List<RaycastHit2D>();
        RaycastHit2D telekinetableHit = new RaycastHit2D();
        bool telekinetableWasHit = false;
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.layerMask = LayerMask.GetMask();
        Vector2 WorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Physics2D.Raycast(player.GetHandPosition(), (WorldPoint - player.GetHandPosition()), contactFilter, hits, (WorldPoint - player.GetHandPosition()).magnitude);
        Debug.DrawRay(player.GetHandPosition(), (WorldPoint - player.GetHandPosition()), Color.red);

        foreach (RaycastHit2D hit in hits)
        {
            if(hit.collider.tag == "Player")
            {
                playerHits.Add(hit);
            }
            if (hit.collider.tag == "Telekinetable")
            {
                telekinetableHit = hit;
                telekinetableWasHit = true;
            }
        }

        foreach(RaycastHit2D hit in playerHits)
        {
            hits.Remove(hit);
        }

        if(isFirstQuery && telekinetableWasHit)
        {
            if (hits[0].collider.tag != "Telekinetable")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else if(!isFirstQuery)
        {
            if(telekinetableWasHit && hits.IndexOf(telekinetableHit) != 0)
            {
                return false;
            }
            else if(!telekinetableWasHit && hits.Count > 0)
            {
                return false;
            }
        }

        return true;
    }

    public void ExecuteAbility()
    {
        RaycastHit2D Hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (Hit.collider != null && IsTelekinetable(true) && Hit.collider.tag == "Telekinetable")
        {
            currentTelekinetable = Hit.transform;
            currentTelekinetable.gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;
        }
    }

    public void AccumulateAbility()
    {
        Vector2 WorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!IsTelekinetable(false))
        {
            DropTelekinetable();
        }

        if (currentTelekinetable)
        {
            RestrictedTelekinetable restriction = null;
            if (restriction = currentTelekinetable.GetComponent<RestrictedTelekinetable>())
            {
                if (restriction.IsHorizontallyMoveable()
                    && (restriction.GetRestrictionPath() == Vector2.zero))
                {
                    currentTelekinetable.position = new Vector3(currentTelekinetable.position.x, WorldPoint.y, 0);
                }
                if (restriction.IsVerticallyMoveable()
                    && (restriction.GetRestrictionPath() == Vector2.zero))
                {
                    currentTelekinetable.position = new Vector3(WorldPoint.x, currentTelekinetable.position.y, 0);
                }
                Vector2 restrictionPath = Vector2.zero;
                if((restrictionPath = restriction.GetRestrictionPath()) != Vector2.zero)
                {
                    Vector2 translation = Vector2.zero;
                    translation.x = Mathf.Clamp(WorldPoint.x, restriction.GetOriginalPosition().x, restriction.GetOriginalPosition().x + restrictionPath.x);
                    translation.y = Mathf.Clamp(WorldPoint.y, restriction.GetOriginalPosition().y, restriction.GetOriginalPosition().y + restrictionPath.y);

                    currentTelekinetable.position = translation;
                }
            }
            else
            {
                currentTelekinetable.position = new Vector3(WorldPoint.x, WorldPoint.y, 0);
            }
            currentTelekinetable.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
    }
    public void ReleaseAbility()
    {
        DropTelekinetable();
    }

    public void DropTelekinetable()
    {
        if (currentTelekinetable)
        {
            currentTelekinetable.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            currentTelekinetable.gameObject.GetComponent<Rigidbody2D>().freezeRotation = false;
            currentTelekinetable = null;
        }
    }
}
