using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Input is sampled once per frame; movement is integrated by the 2D physics clock.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(AudioSource))]
public sealed class PlayerMovement : MonoBehaviour
{
    [Min(0f)] public float speed = 5f;
    private Rigidbody2D body;
    private AudioSource pickupAudio;
    private Vector2 movement;
    public Vector2 Movement => movement;
    public float Stamina { get; private set; } = 1f;
    public bool Sprinting { get; private set; }
    private SpriteRenderer visual;
    private float hurtUntil;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        pickupAudio = GetComponent<AudioSource>();
        visual = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        Vector2 input = Vector2.zero;
        if (keyboard != null)
        {
            input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0)
                    - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
            input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0)
                    - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
        }
        if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > input.sqrMagnitude)
            input = Gamepad.current.leftStick.ReadValue();
        if (Gamepad.current != null && Gamepad.current.dpad.ReadValue().sqrMagnitude > input.sqrMagnitude)
            input = Gamepad.current.dpad.ReadValue();
        SetMovement(input);
        bool boost = (keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed))
                     || (Gamepad.current != null && Gamepad.current.rightShoulder.isPressed);
        Sprinting = GameManager.IsPlaying && boost && movement.sqrMagnitude > .01f && Stamina > .02f;
        Stamina = Mathf.Clamp01(Stamina + (Sprinting ? -.42f : boost ? 0f : .22f) * Time.deltaTime);
        visual.color = Time.time < hurtUntil && Mathf.Sin(Time.time * 30f) > 0 ? new Color(1, .35f, .3f) : Color.white;
    }

    public void SetMovement(Vector2 input) => movement = Vector2.ClampMagnitude(input, 1f);

    private void FixedUpdate()
    {
        if (!GameManager.IsPlaying)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }
        body.MovePosition(body.position + movement * speed * (Sprinting ? 1.6f : 1f) * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameManager.IsPlaying || !other.CompareTag("Coletavel")) return;
        var collectible = other.GetComponent<Collectible>();
        if (collectible == null || !collectible.TryCollect()) return;
        if (pickupAudio.clip != null) pickupAudio.PlayOneShot(pickupAudio.clip);
        GameManager.Instance.RegisterCollection();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<TemplarGuard>() != null && GameManager.IsPlaying)
        {
            if (GameManager.Instance.TakeDamage()) hurtUntil = Time.time + 1.5f;
        }
    }
}
