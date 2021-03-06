using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Ship_Base
{
    public int player_index = 0;
    public float fly_speed = 100f;

    public float move_LR_rotation = 40f;
    public float move_LR_rotation_speed = 200f;
    public Vector3 scale_UD_min = new Vector3(1f, 1f, 0.4f);
    public Vector3 scale_UD_max = new Vector3(1f, 1f, 1.6f);
    public float scale_UD_speed = 10f;
    public GameObject[] scale_UD_objects = null;
    Vector3[] scale_UD_objects_defaults = null;

    public Shield_Info shield_settings = new Shield_Info();
    [System.Serializable]
    public class Shield_Info {
        public GameObject shield_prefab = null;
        public Vector3 shield_offset = Vector3.zero;
        public float shield_energy_consumption_per_sec = 1f;
        public float shield_energy_consumption_per_hit = 0.5f;
        public Engine.Audio_Info[] SFX_ShieldOn = null;
        public Engine.Audio_Info[] SFX_ShieldOff = null;
        public Engine.Audio_Info[] SFX_ShieldLoop = null;
        public Engine.Audio_Info[] SFX_ShieldHit = null;
        public string animator_trigger_on = "";
        public string animator_trigger_off = "";
    }
    
    Rigidbody rb = null;
    Transform camera_main = null;
    AudioSource shield_aud = null;
    Animator shield_animator = null;
    List<Gun>[] guns_indexed = new List<Gun>[]{ new List<Gun>(), new List<Gun>(), new List<Gun>(), new List<Gun>() };

    // Start is called before the first frame update
    void Start()
    {
        Start_Base();

        rb = GetComponent<Rigidbody>();
        camera_main = Engine.inst.camera_main.transform;

        foreach (var g in guns) {
            g.fraction = Gun.Fractions.Player;
            if (g.fire_index <= 3) { guns_indexed[g.fire_index].Add(g); }
        }
        aud.volume = Global_Settings.Volume_SFX;

        //Save objects (thrusters) default scaling
        if (scale_UD_objects != null && scale_UD_objects.Length > 0) {
            scale_UD_objects_defaults = new Vector3[scale_UD_objects.Length];
            for (int n = 0; n < scale_UD_objects.Length; n++) {
                scale_UD_objects_defaults[n] = scale_UD_objects[n].transform.localScale;
            }
        }

        var ss = shield_settings;
        if (ss.shield_prefab != null) {
            if (ss.shield_prefab.scene.name == null) { ss.shield_prefab = Instantiate(ss.shield_prefab); }
            ss.shield_prefab.SetActive(false);
            ss.shield_prefab.transform.SetParent(transform);
            ss.shield_prefab.transform.position = transform.position;
            ss.shield_prefab.transform.rotation = Quaternion.identity;
            //ss.shield_prefab.transform.localScale = Vector3.one;
            shield_aud = ss.shield_prefab.AddComponent<AudioSource>();
            shield_aud.volume = Global_Settings.Volume_SFX;
            shield_aud.loop = true;
            shield_animator = ss.shield_prefab.GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Update_base();

        var controls = Global_Settings.Player_Controls[player_index];

        //Shield
        var ss = shield_settings;
        if (ss.shield_prefab != null) {
            if (Input.GetKeyDown(controls.shield)) {
                if (ss.shield_prefab.activeSelf) {
                    Shield_Disable();
                } else {
                    Shield_Enable();
                }
            }
            if (ss.shield_prefab.activeSelf) {
                var e = ss.shield_energy_consumption_per_sec * Time.deltaTime;
                if (energy >= e) { energy -= e; }
                else { ss.shield_prefab.SetActive(false); }
            }
        }
        
        //Fire
        var fire_buttons = new KeyCode[]{ controls.fire1, controls.fire2, controls.fire3, controls.fire4 };
        for (int n = 0; n < 3; n++) {
            if (guns_indexed[n].Count == 0) continue;
            if (Input.GetKeyUp(fire_buttons[n])) {
                foreach (var g in guns_indexed[n]) { g.Fire_Stop(); }        
            }
            if (Input.GetKey(fire_buttons[n])) {
                if (energy > 0) {
                    foreach (var g in guns_indexed[n]) {
                        if (g.Fire()) { energy -= g.energy_consumed; }
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
                var item = h.collider.gameObject.GetComponent<Item>();
                if (item) {
                    item.Collect(this); continue;
                }
                if (h.collider.gameObject.GetComponent<Bullet>() != null) continue;
                return false;
            }
        }

        var new_pos = transform.position + (dir * length);
        transform.position = new_pos;
        return true;
    }

    void Shield_Enable() {
        var ss = shield_settings;
        if (ss.shield_prefab == null) { return; }

        ss.shield_prefab.SetActive(true);
        Engine.Play_Sound_2D(ss.SFX_ShieldOn);

        if (ss.SFX_ShieldLoop != null && ss.SFX_ShieldLoop.Length > 0) {
            var r = Random.Range(0, ss.SFX_ShieldLoop.Length);
            shield_aud.clip = ss.SFX_ShieldLoop[r].clip;
            shield_aud.volume = Global_Settings.Volume_SFX * ss.SFX_ShieldLoop[r].VolumeScale;
            shield_aud.Play();
        }

        if (shield_animator != null && !string.IsNullOrWhiteSpace(ss.animator_trigger_on)) {
            shield_animator.SetTrigger(ss.animator_trigger_on);
        }
    }
    void Shield_Disable() {
        var ss = shield_settings;
        if (ss.shield_prefab == null) { return; }

        ss.shield_prefab.SetActive(false);
        Engine.Play_Sound_2D(ss.SFX_ShieldOff);
        shield_aud.Stop();

        if (shield_animator != null && !string.IsNullOrWhiteSpace(ss.animator_trigger_off)) {
            shield_animator.SetTrigger(ss.animator_trigger_off);
        }
    }
    public override void Damage(float d, Vector3 hit_point) {
        var ss = shield_settings;
        if (ss.shield_prefab != null && ss.shield_prefab.activeSelf) {
            Engine.Play_Sound_2D(ss.SFX_ShieldHit);
            energy -= ss.shield_energy_consumption_per_hit;
            if (energy < 0) { energy = 0; Shield_Disable(); }
            return;
        }
        base.Damage(d, hit_point);
    }
}
