using UnityEngine;
using Cinemachine;

public class CameraZoomController : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera; // Free Look kamera referansı
    public float zoomSpeed = 2f; // Zoom hızı
    public float minFOV = 15f;   // Minimum FOV (en yakın)
    public float maxFOV = 60f;   // Maksimum FOV (en uzak)

    void Update()
    {
        // Fare tekerleği girişini al
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // FOV'u güncelle
        if (scrollInput != 0)
        {
            freeLookCamera.m_Lens.FieldOfView -= scrollInput * zoomSpeed;
            freeLookCamera.m_Lens.FieldOfView = Mathf.Clamp(freeLookCamera.m_Lens.FieldOfView, minFOV, maxFOV);
        }
    }
}
