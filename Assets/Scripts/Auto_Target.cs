using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auto_Target : MonoBehaviour
{
    public float Auto_Target_Speed = 40f;
    public float Auto_Target_Tilt_Speed = 0f;
    public float Auto_Target_Tilt_Max = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Auto_Rotate(transform, Auto_Target_Speed, Auto_Target_Tilt_Speed, Auto_Target_Tilt_Max);
    }

    public static void Auto_Rotate(Transform tr, float rotate_speed, float tilt_speed = 0f, float tilt_max = 0f) {
        if (Mathf.Approximately(rotate_speed, 0f)) return;

        if (Engine.inst.players[0] == null) return;
        var target = Engine.inst.players[0].transform;
        //TODO: check for other players

        var dir = target.position - tr.position;
        dir.y = 0f;
        var rotation = Quaternion.LookRotation(dir);
        rotation = Quaternion.RotateTowards(tr.rotation, rotation, rotate_speed * Time.deltaTime);
        
        float target_rot_z = 0f;
        if (tr.rotation.eulerAngles.y > rotation.eulerAngles.y) { target_rot_z = tilt_max; } 
        else { target_rot_z = -tilt_max; }
        var rotation_tilted = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, target_rot_z);
        rotation = Quaternion.RotateTowards(rotation, rotation_tilted, tilt_speed * Time.deltaTime);

        tr.rotation = rotation;
    }
}
