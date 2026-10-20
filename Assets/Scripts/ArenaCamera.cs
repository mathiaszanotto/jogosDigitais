using UnityEngine;
[RequireComponent(typeof(Camera))]
public sealed class ArenaCamera : MonoBehaviour
{
    private void LateUpdate() { var camera = GetComponent<Camera>(); camera.orthographicSize = 5 * Mathf.Max(1, 1.6f / camera.aspect); }
}
