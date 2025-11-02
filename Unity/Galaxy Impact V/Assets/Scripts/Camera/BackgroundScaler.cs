using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        FitToCamera();
    }

    void FitToCamera()
    {
        Camera cam = Camera.main;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Tamaño del sprite
        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;

        // Tamaño visible de la cámara (en unidades del mundo)
        float worldScreenHeight = cam.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * cam.aspect;

        // Calcula la escala necesaria para cubrir la cámara
        Vector3 scale = transform.localScale;
        scale.x = worldScreenWidth / width;
        scale.y = worldScreenHeight / height;

        transform.localScale = scale;
    }
}
