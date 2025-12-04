using UnityEngine;
using UnityEngine.Video;

public class IntroLoopToVideo : MonoBehaviour
{
    public VideoPlayer introPlayer;
    public VideoPlayer loopPlayer;
    public UIFader uiFader;

    void Start()
    {
        // Intro: reproducimos normal
        introPlayer.gameObject.SetActive(true);
        introPlayer.loopPointReached += OnIntroEnd;

        // Loop: lo dejamos activo PERO pausado (esto es clave)
        loopPlayer.gameObject.SetActive(true);
        loopPlayer.playOnAwake = false;
        loopPlayer.Pause();

        // Preparamos el loop
        loopPlayer.Prepare();

        introPlayer.Play();
    }

    void OnIntroEnd(VideoPlayer vp)
    {
        // Esperamos a que el loop esté preparado
        StartCoroutine(ActivateLoopWhenReady());
    }

    private System.Collections.IEnumerator ActivateLoopWhenReady()
    {
        // Si aún no está listo, esperamos
        while (!loopPlayer.isPrepared)
            yield return null;

        // Ahora cambiamos:
        introPlayer.gameObject.SetActive(false);

        // Reproducimos el loop
        loopPlayer.Play();

        // Fade de UI
        uiFader.FadeIn();
    }
}
