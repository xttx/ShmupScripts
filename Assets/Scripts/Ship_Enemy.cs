using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Enemy : Ship_Base
{
    public float Fire_Delay = 1f;
    public Movement_Types Movement_Type = Movement_Types.direct;
    public Movement_Direct_Info Movement_Direct_Settings = null;
    public Movement_Spline_Info Movement_Spline_Settings = null;
    public Ship_Enemy[] activate_enemies = null;

    float fire_timer = 0f;
    float spline_timer = 0f;
    protected bool active = false;

    [System.Serializable]
    public class Movement_Direct_Info {
        public float Speed = 50f;
    }
    [System.Serializable]
    public class Movement_Spline_Info {
        public UnityEngine.Splines.SplineContainer spline = null;
        public float Speed = 10f;
    }

    public enum Movement_Types { none, direct, spline }

    // Start is called before the first frame update
    public void Start_Base_Enemy()
    {
        Start_Base();
    }

    // Update is called once per frame
    public void Update_base_enemy()
    {
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

        if (Movement_Type == Movement_Types.direct && Movement_Direct_Settings != null) {
            transform.position += Vector3.back * Movement_Direct_Settings.Speed * Time.deltaTime;
        }
        else if (Movement_Type == Movement_Types.spline && Movement_Spline_Settings != null) {
            var coord = Movement_Spline_Settings.spline.EvaluatePosition(spline_timer);
            transform.position = coord;
            spline_timer += Movement_Spline_Settings.Speed * (Time.deltaTime / 100f);
        }
        

        if (guns != null) {
            fire_timer += Time.deltaTime;
            if (fire_timer >= Fire_Delay) {
                fire_timer = 0f; 
                foreach (var g in guns) { g.Fire(); }
            }
        }

        var pos = transform.position;
        var camera_pos = Engine.inst.camera_main.transform.position;
        var x_min = camera_pos.x + Global_Settings.enemy_limits_x.x;
        var x_max = camera_pos.x + Global_Settings.enemy_limits_x.y;
        var y_min = camera_pos.z + Global_Settings.enemy_limits_y.x;
        var y_max = camera_pos.z + Global_Settings.enemy_limits_y.y;
        if (pos.x < x_min || pos.x > x_max) { Destroy_Ship(); return; }
        if (pos.z < y_min || pos.z > y_max) { Destroy_Ship(); return; }
    }

    public override void Destroy_Ship() {
        Destroy(gameObject);
        if (Movement_Type == Movement_Types.spline && Movement_Spline_Settings != null) {
            Destroy(Movement_Spline_Settings.spline.gameObject);
        }
    }
}

