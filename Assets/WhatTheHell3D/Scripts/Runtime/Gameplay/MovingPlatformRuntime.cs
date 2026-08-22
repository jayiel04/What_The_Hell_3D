using UnityEngine;

public sealed class MovingPlatformRuntime : MonoBehaviour
{
    public Vector3 travel;
    public float duration = 2f;
    private Vector3 start;

    private void Awake()
    {
        start = transform.position;
    }

    public void Configure(Vector3 movement, float seconds)
    {
        start = transform.position;
        travel = movement;
        duration = Mathf.Max(0.1f, seconds);
    }

    private void Update()
    {
        float progress = Mathf.PingPong(Time.time / duration, 1f);
        transform.position = start + travel * progress;
    }
}
