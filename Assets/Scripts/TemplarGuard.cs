using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class TemplarGuard : MonoBehaviour
{
    public Vector2[] patrolPoints;
    public float patrolSpeed = 1.35f, chaseSpeed = 2.15f, detectionRadius = 3.4f;
    public bool IsChasing { get; private set; }
    private Rigidbody2D body;
    private PlayerMovement player;
    private SpriteRenderer visual;
    private int targetIndex;
    private float stuckTime;
    private Vector2 previous;
    private void Awake() { body = GetComponent<Rigidbody2D>(); visual = GetComponent<SpriteRenderer>(); }
    private void Start() { player = FindAnyObjectByType<PlayerMovement>(); previous = body.position; }
    private void FixedUpdate()
    {
        if (!GameManager.IsPlaying || player == null) { body.linearVelocity = Vector2.zero; return; }
        Vector2 playerPosition = player.transform.position;
        IsChasing = Vector2.Distance(body.position, playerPosition) < detectionRadius
            && !Physics2D.Linecast(body.position, playerPosition, 1 << 8);
        Vector2 target = IsChasing ? playerPosition : patrolPoints[targetIndex];
        if (!IsChasing && Vector2.Distance(body.position, target) < .18f) targetIndex = (targetIndex + 1) % patrolPoints.Length;
        var direction = (target - body.position).normalized;
        body.MovePosition(body.position + direction * (IsChasing ? chaseSpeed : patrolSpeed) * Time.fixedDeltaTime);
        visual.color = IsChasing ? new Color(1, .55f, .4f) : Color.white;
        if (Vector2.Distance(previous, body.position) < .002f) stuckTime += Time.fixedDeltaTime; else stuckTime = 0;
        if (stuckTime > 1.2f) { targetIndex = (targetIndex + 1) % patrolPoints.Length; stuckTime = 0; }
        previous = body.position;
    }
}
