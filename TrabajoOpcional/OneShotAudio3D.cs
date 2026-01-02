using UnityEngine;

public static class OneShotAudio3D
{
    public static void Play(AudioClip clip, Vector3 pos, float volume = 1f, float minDist = 0.3f, float maxDist = 15f)
    {
        if (!clip) return;

        var go = new GameObject("OneShotAudio3D_" + clip.name);
        go.transform.position = pos;

        var src = go.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        src.playOnAwake = false;
        src.volume = volume;
        src.minDistance = minDist;
        src.maxDistance = maxDist;
        src.dopplerLevel = 0f;
        src.rolloffMode = AudioRolloffMode.Logarithmic;

        src.clip = clip;
        src.Play();

        Object.Destroy(go, clip.length + 0.1f);
    }
}
