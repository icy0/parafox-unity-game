using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bombthrow : MonoBehaviour, IAbility
{
    [SerializeField]
    Transform BombPrefab;

    [SerializeField]
    Transform BombTrajectoryPrefab;

    [SerializeField]
    float ThrowPower = 3.0f;

    [SerializeField]
    private const float CooldownTime = 3.0f;

    private float LastTimeBombThrown = -CooldownTime;

    Player Player;

    public void Start()
    {
        Player = GetComponent<Player>();
    }

    public void ExecuteAbility()
    {
        // Don't do anything, Bombthrow is only supposed to be executed continuously.
    }

    public string getAbilityName()
    {
        return "Bombthrow";
    }

    public void ReleaseAbility()
    {
        if ((Time.time - LastTimeBombThrown) >= CooldownTime)
        {
            LastTimeBombThrown = Time.time;

            ThrowBomb();
            ThrowPower = 3.0f;
        }
    }

    public void AccumulateAbility()
    {
        if ((Time.time - LastTimeBombThrown) >= CooldownTime)
        {
            if (ThrowPower <= 5.0f)
            {
                ThrowPower += 0.01f;
            }
            DrawTrajectoryPrediction();
        }
    }

    private void ThrowBomb()
    {
        Vector2 HandPosition = Player.GetHandPosition();
        Vector2 CurrentMousePosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        Vector2 ThrowingDirection = (CurrentMousePosition - HandPosition) * 40;

        Transform Bomb = Instantiate(BombPrefab, HandPosition, Quaternion.identity);
        Bomb.GetComponent<Rigidbody2D>().AddForce(ThrowingDirection * ThrowPower);
    }

    private void DrawTrajectoryPrediction()
    {
        Vector2 HandPosition = Player.GetHandPosition();
        Vector2 CurrentMousePosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        Vector2 ThrowingDirection = (CurrentMousePosition - HandPosition) * 40;

        Transform InvisibleBomb = Instantiate(BombTrajectoryPrefab, HandPosition, Quaternion.identity);
        InvisibleBomb.GetComponent<Rigidbody2D>().AddForce(ThrowingDirection * ThrowPower);
    }
}
