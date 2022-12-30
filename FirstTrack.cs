using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstTrack : MonoBehaviour
{
    public class Layer
    {
        enum LayerState
        {
            UNMUTED,
            FADING,
            MUTED
        }

        const float fadeTime = 1;
        readonly AudioSource audioSource;
        LayerState layerState;

        public delegate Coroutine Callback(IEnumerator coroutine);
        readonly Callback startCoroutine;

        public Layer(AudioSource audioSource, Callback startCoroutine)
        {
            this.audioSource = audioSource;
            this.audioSource.volume = 0;
            this.startCoroutine = startCoroutine;
            layerState = LayerState.MUTED;
        }

        IEnumerator FadeIn()
        {
            if (layerState == LayerState.FADING)
                yield break;

            layerState = LayerState.FADING;

            while (audioSource.volume < 1)
            {
                audioSource.volume += 0.01f;
                yield return new WaitForSeconds(fadeTime / 100);
            }

            layerState = LayerState.UNMUTED;
        }

        IEnumerator FadeOut()
        {
            if (layerState == LayerState.FADING)
                yield break;

            layerState = LayerState.FADING;

            while (audioSource.volume > 0)
            {
                audioSource.volume -= 0.01f;
                yield return new WaitForSeconds(fadeTime / 100);
            }

            layerState = LayerState.MUTED;
        }

        IEnumerator FadeAndStop()
        {
            startCoroutine(FadeOut());

            while (layerState == LayerState.FADING)
                yield return null;

            if (layerState == LayerState.MUTED)
                audioSource.Stop();
        }

        void Mute()
        {
            startCoroutine(FadeOut());
        }

        void Unmute()
        {
            startCoroutine(FadeIn());
        }

        public void Play(bool startUnmuted)
        {
            if (layerState == LayerState.FADING)
                return;

            audioSource.Play();

            if (startUnmuted)
                Unmute();
        }

        public void Stop()
        {
            startCoroutine(FadeAndStop());
        }

        public void ToggleState()
        {
            if (layerState == LayerState.MUTED)
                Unmute();

            if (layerState == LayerState.UNMUTED)
                Mute();
        }
    }

    public readonly Dictionary<int, Layer> layers =
    new Dictionary<int, Layer>();

    public void Play()
    {
        layers[1].Play(true);
        layers[2].Play(false);
        layers[3].Play(false);
        layers[4].Play(false);
    }

    public void Stop()
    {
        foreach (var layer in layers)
        {
            layer.Value.Stop();
        }
    }

    void Awake()
    {
        var audioSources = gameObject.GetComponents<AudioSource>();

        for (int i = 0; i < audioSources.Length; i++)
        {
            layers.Add(i + 1, new Layer(audioSources[i], StartCoroutine));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("p"))
            Play();

        if (Input.GetKeyDown("s"))
            Stop();

        if (Input.GetKeyDown("1"))
            layers[1].ToggleState();

        if (Input.GetKeyDown("2"))
            layers[2].ToggleState();

        if (Input.GetKeyDown("3"))
            layers[3].ToggleState();

        if (Input.GetKeyDown("4"))
            layers[4].ToggleState();
    }
}
