using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    Transform ShoulderTransform;
    Transform HandTransform;

    Vector2 CurrentMousePosition;
    Vector2 Shoulder;

    public void Start()
    {
        ShoulderTransform = transform.Find("Shoulder");
        HandTransform = transform.Find("Shoulder").Find("Hand");
        Shoulder = ShoulderTransform.position;
    }

    public void Update()
    {
        Shoulder = ShoulderTransform.position;
        CurrentMousePosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        ShoulderTransform.rotation = Quaternion.LookRotation(Vector3.forward, (Shoulder - CurrentMousePosition));
    }

    public Vector2 GetHandPosition()
    {
        return HandTransform.position;
    }
}
