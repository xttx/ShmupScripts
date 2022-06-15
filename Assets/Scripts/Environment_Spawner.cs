using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Environment_Spawner : MonoBehaviour
{
    public int count = 50;
    public Vector3 offset = new Vector3(0f, 0f, 10f);
    public Vector3 scaling = Vector3.one;
    public bool scaling_is_ratio = true;
    public Vector3 movement = new Vector3(0f, 0f, -25f);
    public float acceleration = 1.001f;
    public List<Override_Info> Override_Elements = new List<Override_Info>();
    
    public float Override_Activation_dist = 0f;
    public float Override_Death_dist = 0f;

    GameObject last_spawn = null;
    float activation_distance = 0f;
    float death_distance = 0f;

    bool active = false;
    GameObject first = null;
    GameObject last = null;
    List<GameObject> objects = new List<GameObject>();

    [System.Serializable]
    public class Override_Info {
        public int index = 0;
        public GameObject prefab = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Mathf.Approximately(Override_Activation_dist, 0f)) {
            activation_distance = Global_Settings.enemy_activation_distance;
        } else {
            activation_distance = Override_Activation_dist;
        }
        if (Mathf.Approximately(Override_Death_dist, 0f)) {
            death_distance = -Global_Settings.enemy_limits_y.x;
        } else {
            death_distance = Override_Death_dist;
        }

        var children = transform.childCount;
        if (children == 1) {
            objects.Add( transform.GetChild(0).gameObject );
        }
        else if (children == 2) {
            first = transform.GetChild(0).gameObject;
            objects.Add( transform.GetChild(1).gameObject );
        }
        else if (children == 3) {
            first = transform.GetChild(0).gameObject;
            objects.Add( transform.GetChild(1).gameObject );
            last = transform.GetChild(2).gameObject;
        }
        else if (children > 3) {
            first = transform.GetChild(0).gameObject;
            last = transform.GetChild(children - 1).gameObject;
            for (int n = 1; n < children - 1; n++) {
                objects.Add( transform.GetChild(n).gameObject );
            }
        }

        if (last != null) last.SetActive(false);
        if (objects.Count == 0) return;
        
        //var next_pos = objects[objects.Count - 1].transform.position + offset;
        var next_pos = objects[0].transform.position;
        //for (int n = objects.Count + 1; n <= count; n++) {
        for (int n = 0; n < count; n++) {
            var r = Random.Range(0, objects.Count);
            var obj = objects[r];
            if (Override_Elements != null && Override_Elements.Count > 0) {
                var over = Override_Elements.Where(e=> e.index == n).FirstOrDefault();
                if (over != null) obj = over.prefab;
            }

            var new_obj = Instantiate(obj, transform);
            new_obj.transform.position = next_pos;
            new_obj.transform.rotation = obj.transform.rotation;
            if (scaling_is_ratio) {
                new_obj.transform.localScale = Vector3.Scale(obj.transform.localScale, scaling);
            } else {
                new_obj.transform.localScale = scaling;
            }

            next_pos += offset;
            last_spawn = new_obj;
        }
        if (last != null) {
            last.transform.position = next_pos;
            last.SetActive(true);
            last_spawn = last;
        }

        foreach (var o in objects) { Destroy(o); }
    }

    // Update is called once per frame
    void Update()
    {
        var z_camera = Engine.inst.camera_main.position.z;
        if (!active) {
            var distance = Mathf.Abs(transform.position.z - z_camera);
            if (distance <= activation_distance) {
                active = true;
            }
            else {
                return;
            }
        }

        transform.position += movement * Time.deltaTime;
        movement = movement * acceleration;

        if (z_camera > last_spawn.transform.position.z + death_distance) {
            Destroy(gameObject);
        }
    }
}
