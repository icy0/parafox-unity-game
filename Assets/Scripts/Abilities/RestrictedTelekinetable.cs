using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestrictedTelekinetable : MonoBehaviour
{
    [SerializeField]
    private bool verticalMovement;

    [SerializeField]
    private bool horizontalMovement;

    [SerializeField]
    private Vector2 path;

    private Vector2 originalPosition;

    public void Start()
    {
        originalPosition = transform.position;
    }

    public Vector2 GetOriginalPosition()
    {
        return originalPosition;
    }

    public bool IsHorizontallyMoveable()
    {
        return verticalMovement;
    }
    public bool IsVerticallyMoveable()
    {
        return horizontalMovement;
    }

    public Vector2 GetRestrictionPath()
    {
        return path;
    }

    public void SetRestrictionPath(Vector2 path) {
        this.path = path;
    }
}
