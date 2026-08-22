using UnityEngine;

public sealed class FallingPlatformRuntime : MonoBehaviour
{
    public Transform platform;
    private Vector3 start;
    private bool activated;
    private float activatedAt;

    private void Awake()
    {
        if (platform == null)
        {
            platform = transform;
        }

        start = platform.position;
    }

    public void Configure()
    {
        start = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activated = true;
            activatedAt = Time.time;
        }
    }

    private void Update()
    {
        if (!activated)
        {
            return;
        }

        float elapsed = Time.time - activatedAt;
        if (elapsed < 0.45f)
        {
            platform.position = start + Vector3.down * (elapsed * elapsed * 8f);
        }
        else if (elapsed > 3.5f)
        {
            platform.position = start;
            activated = false;
        }
    }
}
