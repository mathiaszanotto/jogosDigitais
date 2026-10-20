using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public sealed class Collectible : MonoBehaviour
{
    private bool collected;

    public bool TryCollect()
    {
        if (collected) return false;
        collected = true;
        // Disable immediately: Destroy is deferred until the end of the frame.
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(gameObject);
        return true;
    }
}
