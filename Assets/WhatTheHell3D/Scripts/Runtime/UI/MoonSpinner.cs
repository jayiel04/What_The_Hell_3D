using UnityEngine;

/// <summary>Rota la luna del menú como en main_menu.gd::_process (0.35°/s).</summary>
public sealed class MoonSpinner : MonoBehaviour
{
    public float degreesPerSecond = 0.35f;

    private void Update()
    {
        transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.World);
    }
}
