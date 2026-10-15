using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform a, b;
    public float minX, maxX, y = 3.5f;

    void LateUpdate() { Snap(); }

    public void Snap()
    {
        if (a == null || b == null) return;
        var cam = GetComponent<Camera>();
        float half = cam.orthographicSize * cam.aspect;
        float x = (a.position.x + b.position.x) * 0.5f;
        float lo = minX + half, hi = maxX - half;
        x = (lo > hi) ? (minX + maxX) * 0.5f : Mathf.Clamp(x, lo, hi);
        transform.position = new Vector3(x, y, -10f);
    }
}
