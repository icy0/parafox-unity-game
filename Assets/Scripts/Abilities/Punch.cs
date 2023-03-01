using UnityEngine;

public class Punch : MonoBehaviour, IAbility
{
    Player Player;

    [SerializeField]
    private float Distance = 5.0f;

    [SerializeField]
    private float PunchPower = 100.0f;

    [SerializeField]
    private const float CooldownTime = 1.0f;

    private float LastTimePunchExecuted = -CooldownTime;

    private void Start()
    {
        Player = GetComponent<Player>();
    }

    public void ExecuteAbility()
    {
        Forcepunch();
    }

    public string getAbilityName()
    {
        return "Punch";
    }

    public void AccumulateAbility()
    {
        // Don't do anything, Punch is intended to only be executed once.
    }

    public void ReleaseAbility()
    {
        // Don't do anything, Punch is intended to only be executed once.
    }

    private void Forcepunch()
    {
        if((Time.time - LastTimePunchExecuted) >= CooldownTime)
        {
            LastTimePunchExecuted = Time.time;

            Vector2 MousePosition = DetermineMousePosition();
            Vector2 HandPosition = Player.GetHandPosition();

            RaycastHit2D RaycastHit = Physics2D.Raycast(HandPosition, MousePosition - HandPosition, Distance);
            Debug.DrawRay(HandPosition, MousePosition - HandPosition, Color.red, 0.5f);

            if(RaycastHit.collider != null)
            {
                Destructible Destructible;
                Rigidbody2D Rigidbody;
                if((Destructible = RaycastHit.collider.gameObject.GetComponent<Destructible>()) && Destructible.WeakDestructible)
                {
                    Destroy(RaycastHit.collider.gameObject);
                }
                else if((Rigidbody = RaycastHit.collider.attachedRigidbody) && (RaycastHit.collider.tag != "Player"))
                {
                    Rigidbody.AddForce((MousePosition - HandPosition) * PunchPower);
                }
            }

            Animator animator = Player.transform.Find("Shoulder").Find("Hand").GetComponent<Animator>();
            animator.SetTrigger("Punch");
        }
    }

    private Vector2 DetermineMousePosition()
    {
        Vector3 Worldpoint = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        return new Vector2(Worldpoint.x, Worldpoint.y);
    }
}
