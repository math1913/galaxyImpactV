using UnityEngine;

public class BackgroundDebug : MonoBehaviour
{
    void Update()
    {
        Camera cam = Camera.main;
        Vector3 screenPos = cam.WorldToViewportPoint(transform.position);

        Debug.Log($"Z:{transform.position.z}, Visible:{(screenPos.z > cam.nearClipPlane && screenPos.z < cam.farClipPlane)}, Scale:{transform.localScale}");
    }
}
