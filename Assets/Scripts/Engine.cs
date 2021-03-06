using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    public Transform camera_main = null;
    public Transform[] players = new Transform[]{ null, null, null, null };

    [HideInInspector]
    public Player[] players_cmp = new Player[]{ null, null, null, null };
    [HideInInspector]
    public bool[] players_spawned = new bool[]{ false, false, false, false };

    public float Scroll_Speed = 5f;

    public static int Money = 0;

    public static Engine inst = null;
    public static AudioSource[] audio_sources = null;
    static int next_aud_source = 0;

    [System.Serializable]
    public class Audio_Info {
        public AudioClip clip = null;
        public float VolumeScale = 1f;
    }

    Vector3 camera_position = Vector3.zero;
    [HideInInspector]
    public static camera_shake_info camera_shake = new camera_shake_info();

    [System.Serializable]
    public class camera_shake_info {
        public float time = 0f;
        public float speed = 10f;
        public float amplification = 3f;
    }

    // Start is called before the first frame update
    void Start()
    {
        inst = this;
        Global_Settings.Init();

        camera_position = camera_main.position;

        var audio_source_mus = camera_main.GetComponent<AudioSource>();
        if (audio_source_mus != null) audio_source_mus.volume = Global_Settings.Volume_Music;

        audio_sources = camera_main.GetComponentsInChildren<AudioSource>();
        foreach (var aud in audio_sources) {
            aud.volume = Global_Settings.Volume_SFX;
        }

        for (int n = 0; n < players.Length; n++) {
            if (players[n] == null) { players_spawned[n] = false; continue; }
            
            if (players[n].gameObject.scene.name == null) players_spawned[n] = false;
            else { 
                players_spawned[n] = true;
                players_cmp[n] = players[n].GetComponent<Player>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        camera_position += new Vector3(0f, 0f, Scroll_Speed * Time.deltaTime);
        camera_main.position = camera_position;
        if (camera_shake.time > 0f) {
            Vector3 noise_offset = Vector3.zero;
            noise_offset.x = Mathf.PerlinNoise(camera_shake.time * camera_shake.speed, 0f) - 0.5f;
            noise_offset.z = Mathf.PerlinNoise(0f, camera_shake.time * camera_shake.speed) - 0.5f;
            noise_offset *= camera_shake.amplification;
            camera_main.position += noise_offset;

            camera_shake.time -= Time.deltaTime;
        }

        for (int n = 0; n < players.Length; n++) {
            if (players_spawned[n]) continue;
            if (Global_Settings.Player_Controls[n] == null) continue;

            if (Input.GetKeyDown(Global_Settings.Player_Controls[n].spawn)) {
                players[n] = Instantiate(players[n]).transform;
                players[n].transform.position = new Vector3(0, 0, camera_main.position.z - 50);

                players_cmp[n] = players[n].GetComponent<Player>();
                players_cmp[n].player_index = n;
                players_spawned[n] = true;
            }
        }
    }

    public static void Play_Sound_2D(Audio_Info clip) {
        if (audio_sources == null) return;
        if (audio_sources.Length == 0) return;
        if (next_aud_source >= audio_sources.Length) next_aud_source = 0;

        audio_sources[next_aud_source].PlayOneShot(clip.clip, clip.VolumeScale);
        next_aud_source += 1;
    }
    public static void Play_Sound_2D(Audio_Info[] clips) {
        if (clips == null) return;
        if (clips.Length == 0) return;
        var n = Random.Range(0, clips.Length); Engine.Play_Sound_2D(clips[n]);
    }
}


 static class ExtensionMethods
 {
    /// <summary>
    /// Rounds Vector3.
    /// </summary>
    /// <param name="vector3"></param>
    /// <param name="decimalPlaces"></param>
    /// <returns></returns>
    public static Vector3 Ceil(this Vector3 vector3)
    {
        vector3.x = Mathf.Ceil(vector3.x);
        vector3.y = Mathf.Ceil(vector3.y);
        vector3.z = Mathf.Ceil(vector3.z);
        return vector3;
    }
 }