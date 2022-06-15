using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Enemy : Ship_Base
{
    public float Fire_Delay = 1f;
    public Vector2 Fire_Delay_Random = Vector2.zero;
    public Movement_Types Movement_Type = Movement_Types.direct;
    public Movement_Direct_Info Movement_Direct_Settings = null;
    public Movement_Spline_Info Movement_Spline_Settings = null;
    public float Auto_Target_Speed = 0f;
    public float Auto_Target_Tilt_Max = 30f;
    public float Auto_Target_Tilt_Speed = 10f;
    public Ship_Enemy[] activate_enemies = null;
    public Fire_Burst_Info Fire_Burst = new Fire_Burst_Info();

    float fire_timer = 0f;
    float spline_timer = 0f;
    protected bool active = false;
    protected bool fire_burst_active = false;

    [System.Serializable]
    public class Movement_Direct_Info {
        public float Speed = 50f;
    }
    [System.Serializable]
    public class Movement_Spline_Info {
        public UnityEngine.Splines.SplineContainer spline = null;
        public float Speed = 10f;
    }
    [System.Serializable]
    public class Fire_Burst_Info {
        public bool enabled = false;
        public float burst_interval = 3f;
        public int fire_count = 3;
        public float fire_interval = 0.3f;
    }

    public enum Movement_Types { none, direct, spline }

    // Start is called before the first frame update
    public void Start_Base_Enemy()
    {
        Start_Base();

        foreach (var g in guns) {
            g.fraction = Gun.Fractions.Enemy;
        }
        aud.volume = Global_Settings.Volume_SFX;
    }

    // Update is called once per frame
    public void Update_base_enemy()
    {
        if (Waiting_For_Destroy) return;

        if (!active) {
            var z_camera = Engine.inst.camera_main.position.z;
            var distance = Mathf.Abs(transform.position.z - z_camera);
            if (distance <= Global_Settings.enemy_activation_distance) {
                active = true;
                if (activate_enemies != null) {
                    foreach (var e in activate_enemies) { e.active = true; }
                }

                //Clone spline
                if (Movement_Type == Movement_Types.spline && Movement_Spline_Settings != null) {
                    var s = Movement_Spline_Settings.spline.gameObject;
                    s.transform.position = transform.position;
                    Movement_Spline_Settings.spline = Instantiate(s).GetComponent<UnityEngine.Splines.SplineContainer>();
                }
            }
            else
                return;
        }

        Update_base();
        Auto_Rotate();

        if (Fire_Burst.enabled) {
            if (active && !fire_burst_active) {
                fire_burst_active = true;
                StartCoroutine( Fire_Burst_Coroutine() );
            }
            else if (!active && fire_burst_active) {
                fire_burst_active = false;
                this.StopAllCoroutines();
            }
        }

        if (Movement_Type == Movement_Types.direct && Movement_Direct_Settings != null) {
            transform.position += Vector3.back * Movement_Direct_Settings.Speed * Time.deltaTime;
        }
        else if (Movement_Type == Movement_Types.spline && Movement_Spline_Settings != null) {
            var coord = Movement_Spline_Settings.spline.EvaluatePosition(spline_timer);
            transform.position = coord;
            spline_timer += Movement_Spline_Settings.Speed * (Time.deltaTime / 100f);
        }
        

        if (guns != null && !Fire_Burst.enabled) {
            fire_timer += Time.deltaTime;
            if (fire_timer >= Fire_Delay) {
                fire_timer = 0f; 
                foreach (var g in guns) { g.Fire(); }
                if (Fire_Delay_Random != Vector2.zero) { Fire_Delay = Random.Range(Fire_Delay_Random.x, Fire_Delay_Random.y); }
            }
        }

        var pos = transform.position;
        var camera_pos = Engine.inst.camera_main.transform.position;
        var x_min = camera_pos.x + Global_Settings.enemy_limits_x.x;
        var x_max = camera_pos.x + Global_Settings.enemy_limits_x.y;
        var y_min = camera_pos.z + Global_Settings.enemy_limits_y.x;
        var y_max = camera_pos.z + Global_Settings.enemy_limits_y.y;
        if (pos.x < x_min || pos.x > x_max) { Destroy_Ship(true); return; }
        if (pos.z < y_min || pos.z > y_max) { Destroy_Ship(true); return; }
    }

    IEnumerator Fire_Burst_Coroutine() {
        while (true) {
            yield return new WaitForSeconds(Fire_Burst.burst_interval);
            for (int n = 0; n < Fire_Burst.fire_count; n++) {
                foreach (var g in guns) { g.Fire(); }
                yield return new WaitForSeconds(Fire_Burst.fire_interval);
            }
        }
    }

    public override void Destroy_Ship(bool dont_spawn_vfx = false) {
        this.StopAllCoroutines();
        base.Destroy_Ship(dont_spawn_vfx);
        if (Movement_Type == Movement_Types.spline && Movement_Spline_Settings != null) {
            Destroy(Movement_Spline_Settings.spline.gameObject);
        }
    }

    void Auto_Rotate() {
        if (Mathf.Approximately(Auto_Target_Speed, 0f)) return;

        var target = Engine.inst.players[0].transform;
        //TODO: check for other players

        var dir = target.position - transform.position;
        dir.y = 0f;
        var rotation = Quaternion.LookRotation(dir);
        rotation = Quaternion.RotateTowards(transform.rotation, rotation, Auto_Target_Speed * Time.deltaTime);
        
        float target_rot_z = 0f;
        if (transform.rotation.eulerAngles.y > rotation.eulerAngles.y) { target_rot_z = Auto_Target_Tilt_Max; } 
        else { target_rot_z = -Auto_Target_Tilt_Max; }
        var rotation_tilted = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, target_rot_z);
        rotation = Quaternion.RotateTowards(rotation, rotation_tilted, Auto_Target_Tilt_Speed * Time.deltaTime);

        transform.rotation = rotation;
    }
}

