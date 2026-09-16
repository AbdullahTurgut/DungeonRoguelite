using UnityEngine;

namespace DungeonRoguelite.Audio
{
    public static class AudioHelper
    {
        public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f, float minPitch = 0.9f, float maxPitch = 1.1f)
        {
            if (clip == null) return;
            GameObject go = new GameObject("OneShotAudio");
            go.transform.position = position;
            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 0.5f; // mild 3D
            source.volume = volume;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.Play();
            Object.Destroy(go, clip.length / source.pitch + 0.1f);
        }

        public static void PlayClip(AudioSource source, AudioClip[] clips, float minPitch = 0.9f, float maxPitch = 1.1f, float volumeScale = 1f)
        {
            if (source == null || clips == null || clips.Length == 0) return;
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.PlayOneShot(clip, volumeScale);
        }
    }
}
