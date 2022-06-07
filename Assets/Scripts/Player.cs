using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Ship_Base
{
    public int player_index = 0;
    public float fly_speed = 100f;

    Rigidbody rb = null;
    Transform camera_main = null;

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
    }

    // Update is called once per frame
    void Update()
    {
        Update_base();

        var controls = Global_Settings.Player_Controls[player_index];
        if (Input.GetKey(controls.fire)) {
            if (energy > 0) {
                bool fired = false;
                foreach (var g in guns) {
                    if (g.Fire()) fired = true;
                }
                if (fired) { 
                    energy -= 1;
                    if (aud != null && SFX_Fire != null && SFX_Fire.Length > 0) {
                        var n = Random.Range(0, SFX_Fire.Length); aud.PlayOneShot(SFX_Fire[n].clip, SFX_Fire[n].VolumeScale);
                    }
                }
            }
        }


        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        var pos = transform.position;
        var rot = transform.rotation;
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
            Sweep_Test(Vector3.left, fly_speed * Time.deltaTime);
        }
        if (Input.GetKey(controls.right) && pos.x < x_max) {
            //pos.x += Time.deltaTime * fly_speed;
            //rb.AddForce(Time.deltaTime * fly_speed * 10, 0f, 0f, ForceMode.Impulse);
            //velocity.x += fly_speed;
            Sweep_Test(Vector3.right, fly_speed * Time.deltaTime);
        }
        if (Input.GetKey(controls.up) && pos.z < y_max) {
            //pos.z += Time.deltaTime * fly_speed;
            //rb.AddForce(0f, 0f, Time.deltaTime * fly_speed * 10, ForceMode.Impulse);
            //velocity.z += fly_speed;
            Sweep_Test(Vector3.forward, fly_speed * Time.deltaTime);
        }
        if (Input.GetKey(controls.down) && pos.z > y_min) {
            //pos.z -= Time.deltaTime * fly_speed;
            //rb.AddForce(0f, 0f, -Time.deltaTime * fly_speed * 10, ForceMode.Impulse);
            //velocity.z -= fly_speed;
            Sweep_Test(Vector3.back, fly_speed * Time.deltaTime);
        }

        //pos.z += scroll_speed * Time.deltaTime;
        Sweep_Test(Vector3.forward, scroll_speed * Time.deltaTime);

        //rb.velocity = velocity;
        //rb.Move(pos, rot);
        //rb.AddForce(0f, 0f, scroll_speed * Time.deltaTime * 50, ForceMode.Impulse);
        //transform.position = pos;

        //Death limit
        var y_min_death = camera_main.position.z + Global_Settings.player_limits_death_y.x;
        var y_max_death = camera_main.position.z + Global_Settings.player_limits_death_y.y;
        if (transform.position.z < y_min_death) { Death(); return; }
        if (transform.position.z > y_max_death) { Death(); return; }
    }

    void Sweep_Test(Vector3 dir, float length) {
        RaycastHit hit;
        if (rb.SweepTest(dir, out hit, length)) return;
        
        var new_pos = transform.position + (dir * length);
        transform.position = new_pos;
    }

}
