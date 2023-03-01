using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    Player Player;
    Transform PlayerTransform;
    void Start()
    {
        Player = FindObjectOfType<Player>();
        PlayerTransform = Player.transform;
    }

    void FixedUpdate()
    {
        transform.SetPositionAndRotation(new Vector3(PlayerTransform.position.x, PlayerTransform.position.y + 2.0f, transform.position.z), transform.rotation);
    }
}
