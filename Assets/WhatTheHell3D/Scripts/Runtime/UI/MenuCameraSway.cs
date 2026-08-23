using UnityEngine;

/// <summary>
/// Vaivén senoidal de la cámara del menú (main_menu.gd::_process):
/// posición base (0,5,14) + sin(t·0.22)·0.8 en X y sin(t·0.35)·0.18 en Y,
/// mirando a (0,2.5,−7.5).
/// </summary>
public sealed class MenuCameraSway : MonoBehaviour
{
    public Vector3 basePosition = new Vector3(0f, 5f, 14f);
    public Vector3 lookTarget = new Vector3(0f, 2.5f, -7.5f);

    private float time;

    private void Start()
    {
        time = 0f;
    }

    private void LateUpdate()
    {
        time += Time.deltaTime;
        transform.position = basePosition
            + new Vector3(Mathf.Sin(time * 0.22f) * 0.8f, Mathf.Sin(time * 0.35f) * 0.18f, 0f);
        transform.LookAt(lookTarget, Vector3.up);
    }
}
