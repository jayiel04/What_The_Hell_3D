using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CameraController : MonoBehaviour
{
    public float distance = 8f;
    public float maximumDistance = 10f;
    public float height = 3f;
    public float shoulderOffset;
    public float lookAheadDistance = 1f;
    public float sensitivity = 0.12f;
    public float followSmoothing = 12f;
    public float collisionRadius = 0.25f;

    private Transform target;
    private Transform lockTarget;
    private InputReader input;
    private float yaw;
    private float pitch = 15f;
    private float requestedDistance;

    public void Configure(Transform followTarget, InputActionAsset inputActions, CampaignLevelConfig config)
    {
        target = followTarget;
        input = new InputReader();
        input.Configure(inputActions);
        if (config != null)
        {
            distance = config.cameraDistance;
            maximumDistance = config.cameraMaximumDistance;
            height = config.cameraHeight;
            shoulderOffset = config.cameraShoulderOffset;
            lookAheadDistance = config.cameraLookAheadDistance;
        }

        requestedDistance = distance;
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x > 180f ? angles.x - 360f : angles.x;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (input != null)
        {
            Vector2 look = input.Look;
            yaw += look.x * sensitivity;
            pitch = Mathf.Clamp(pitch - look.y * sensitivity, -15f, 55f);
            requestedDistance = Mathf.Clamp(requestedDistance - MouseScroll() * 0.5f, 3.5f, maximumDistance);
            if (input.LockOnPressed)
            {
                lockTarget = lockTarget == null ? FindNearestEnemy() : null;
            }
        }

        if (lockTarget != null)
        {
            Vector3 lookDirection = lockTarget.position - target.position;
            if (lookDirection.sqrMagnitude > 0.1f)
            {
                yaw = Mathf.LerpAngle(yaw, Quaternion.LookRotation(lookDirection).eulerAngles.y, 8f * Time.deltaTime);
            }
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * height * 0.55f + target.forward * lookAheadDistance;
        Vector3 desired = focus - rotation * Vector3.forward * requestedDistance + rotation * Vector3.right * shoulderOffset;
        Vector3 direction = desired - focus;
        float safeDistance = requestedDistance;
        if (Physics.SphereCast(focus, collisionRadius, direction.normalized, out RaycastHit hit, requestedDistance, LayerMask.GetMask("Ground", "CameraCollision"), QueryTriggerInteraction.Ignore))
        {
            safeDistance = Mathf.Max(1.1f, hit.distance - collisionRadius);
            desired = focus - rotation * Vector3.forward * safeDistance + rotation * Vector3.right * shoulderOffset;
        }

        transform.position = Vector3.Lerp(transform.position, desired, followSmoothing * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, followSmoothing * Time.deltaTime);
    }

    private float MouseScroll()
    {
        return Mouse.current == null ? 0f : Mouse.current.scroll.ReadValue().y * 0.01f;
    }

    private Transform FindNearestEnemy()
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>();
        Transform nearest = null;
        float nearestDistance = float.MaxValue;
        foreach (EnemyController enemy in enemies)
        {
            float currentDistance = Vector3.SqrMagnitude(enemy.transform.position - target.position);
            if (currentDistance < nearestDistance)
            {
                nearestDistance = currentDistance;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }
}
