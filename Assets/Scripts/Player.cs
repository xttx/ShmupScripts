using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Ship_Base
{
    public int player_index = 0;
    public float fly_speed = 100f;

    Rigidbody rb = null;
    Transform camera_main = null;

    public float move_LR_rotation = 40f;
    public float move_LR_rotation_speed = 200f;
    public Vector3 scale_UD_min = new Vector3(1f, 1f, 0.4f);
    public Vector3 scale_UD_max = new Vector3(1f, 1f, 1.6f);
    public float scale_UD_speed = 10f;
    public GameObject[] scale_UD_objects = null;
    Vector3[] scale_UD_objects_defaults = null;

    // Start is called before the first frame update
    void Start()
    {
        Start_Base();

        rb = GetComponent<Rigidbody>();
        camera_main = Engine.inst.camera_main.transform;

        foreach (var g in guns) {
            g.fraction = Gun.Fractions.Player;
        }
        aud.volume = Global_Settings.Volume_SFX;

        //Save objects (thrusters) default scaling
        if (scale_UD_objects != null && scale_UD_objects.Length > 0) {
            scale_UD_objects_defaults = new Vector3[scale_UD_objects.Length];
            for (int n = 0; n < scale_UD_objects.Length; n++) {
                scale_UD_objects_defaults[n] = scale_UD_objects[n].transform.localScale;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Update_base();

        var controls = Global_Settings.Player_Controls[player_index];
        if (Input.GetKeyUp(controls.fire)) {
            foreach (var g in guns) { g.Fire_Stop(); }        
        }
        if (Input.GetKey(controls.fire)) {
            if (energy > 0) {
                bool fired = false;
                float energy_consumed = 0f;
                foreach (var g in guns) {
                    if (g.Fire()) { fired = true; energy_consumed += g.energy_consumed; }
                }
                if (fired) {
                    energy -= energy_consumed;
                    if (aud != null && SFX_Fire != null && SFX_Fire.Length > 0) {
                        var n = Random.Range(0, SFX_Fire.Length); aud.PlayOneShot(SFX_Fire[n].clip, SFX_Fire[n].VolumeScale);
                    }
                }
            }
        }


        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        var pos = transform.position;
        var rot = transform.rotation.eulerAngles;
        var target_z_rot = 0f;
        var target_scale = Vector3.one;
        var scroll_speed = Engine.inst.Scroll_Speed;

        var x_min = camera_main.position.x + Global_Settings.player_limits_x.x;
        var x_max = camera_main.position.x + Global_Settings.player_limits_x.y;
        var y_min = camera_main.position.z + Global_Settings.player_limits_y.x;
        var y_max = camera_main.position.z + Global_Settings.player_limits_y.y;

        //Vector3 velocity = new Vector3(0f, 0f, scroll_speed);
        if (Input.GetKey(controls.left) && pos.x > x_min) {
            //pos.x -= Time.deltaTime * fly_speed;
            //rb.AddForce(-Time.deltaTime * fly_speed * 10, 0f, 0f, ForceMode.Impulse);
            //velocity.x -= fly_speed;
            bool t = Sweep_Test(Vector3.left, fly_speed * Time.deltaTime);
            if (t) target_z_rot = move_LR_rotation;
        }
        if (Input.GetKey(controls.right) && pos.x < x_max) {
            //pos.x += Time.deltaTime * fly_speed;
            //rb.AddForce(Time.deltaTime * fly_speed * 10, 0f, 0f, ForceMode.Impulse);
            //velocity.x += fly_speed;
            bool t = Sweep_Test(Vector3.right, fly_speed * Time.deltaTime);
            if (t) target_z_rot = -move_LR_rotation;
        }
        if (Input.GetKey(controls.up) && pos.z < y_max) {
            //pos.z += Time.deltaTime * fly_speed;
            //rb.AddForce(0f, 0f, Time.deltaTime * fly_speed * 10, ForceMode.Impulse);
            //velocity.z += fly_speed;
            bool t = Sweep_Test(Vector3.forward, fly_speed * Time.deltaTime);
            if (t) target_scale = scale_UD_max;
        }
        if (Input.GetKey(controls.down) && pos.z > y_min) {
            //pos.z -= Time.deltaTime * fly_speed;
            //rb.AddForce(0f, 0f, -Time.deltaTime * fly_speed * 10, ForceMode.Impulse);
            //velocity.z -= fly_speed;
            bool t = Sweep_Test(Vector3.back, fly_speed * Time.deltaTime);
            if (t) target_scale = scale_UD_min;
        }

        //pos.z += scroll_speed * Time.deltaTime;
        Sweep_Test(Vector3.forward, scroll_speed * Time.deltaTime);

        //rb.velocity = velocity;
        //rb.Move(pos, rot);
        //rb.AddForce(0f, 0f, scroll_speed * Time.deltaTime * 50, ForceMode.Impulse);
        //transform.position = pos;

        //Z-Rotation
        var angle = Mathf.DeltaAngle(rot.z, target_z_rot);
        if (!Mathf.Approximately(angle, 0f)) {
            rot.z = Mathf.MoveTowardsAngle(rot.z, target_z_rot, move_LR_rotation_speed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, rot.z);
        }

        //Thruster scaling
        if (scale_UD_objects != null) {
            for (int n = 0; n < scale_UD_objects.Length; n++) {
                var scale_orig = scale_UD_objects_defaults[n];
                var scale_new = Vector3.Scale(target_scale, scale_orig);
                var s = Vector3.MoveTowards(scale_UD_objects[n].transform.localScale, scale_new, scale_UD_speed * Time.deltaTime);
                scale_UD_objects[n].transform.localScale = s;
            }
        }

        //Death limit
        var y_min_death = camera_main.position.z + Global_Settings.player_limits_death_y.x;
        var y_max_death = camera_main.position.z + Global_Settings.player_limits_death_y.y;
        if (transform.position.z < y_min_death) { Death(); return; }
        if (transform.position.z > y_max_death) { Death(); return; }
    }

    bool Sweep_Test(Vector3 dir, float length) {
        //RaycastHit hit;
        //if (rb.SweepTest(dir, out hit, length)) return;
        
        var hits = rb.SweepTestAll(dir, length);
        if (hits != null && hits.Length > 0) {
            foreach (var h in hits) {
                if (h.collider.gameObject.GetComponent<Bullet>() != null) continue;
                return false;
            }
        }

        var new_pos = transform.position + (dir * length);
        transform.position = new_pos;
        return true;
    }

}
