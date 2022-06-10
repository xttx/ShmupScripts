using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment_Spawner : MonoBehaviour
{
    public int count = 50;
    public Vector3 offset = new Vector3(0f, 0f, 10f);
    public Vector3 scaling = Vector3.one;
    public bool scaling_is_ratio = true;
    public Vector3 movement = new Vector3(0f, 0f, -25f);
    public float acceleration = 1.001f;
    
    GameObject last_spawn = null;

    bool active = false;
    GameObject first = null;
    GameObject last = null;
    List<GameObject> objects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
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
        
        var next_pos = objects[objects.Count - 1].transform.position + offset;
        for (int n = objects.Count + 1; n <= count; n++) {
            var r = Random.Range(0, objects.Count);
            var obj = objects[r];
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
    }

    // Update is called once per frame
    void Update()
    {
        var z_camera = Engine.inst.camera_main.position.z;
        if (!active) {
            var distance = Mathf.Abs(transform.position.z - z_camera);
            if (distance <= Global_Settings.enemy_activation_distance) {
                active = true;
            }
            else {
                return;
            }
        }

        transform.position += movement * Time.deltaTime;
        movement = movement * acceleration;

        if (z_camera > last_spawn.transform.position.z + Global_Settings.enemy_activation_distance) {
            Destroy(gameObject);
        }
    }
}
