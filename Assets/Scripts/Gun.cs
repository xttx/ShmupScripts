using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public int fire_index = 0;
    public Weapon_Info weapon = new Weapon_Info();
    public Directional_Info directional_settings = new Directional_Info();
    public Laser_Info laser_settings = new Laser_Info();

    public enum Weapon_Types { directional, laser };
    [System.Serializable]
    public class Weapon_Info {
        public Weapon_Types weapon_type = Weapon_Types.directional;
        public GameObject bullet_prefab = null;
        public Vector3 spawn_offset = Vector3.zero;

        public bool force_Y_0 = true;
        public bool project_Y_0 = false;
        public bool pass_through_enemies = false;
        public bool pass_through_environment = false;
    }
    [System.Serializable]
    public class Directional_Info {
        public float fire_delay = 0.4f;
        public int damage = 1;
        public int energy_consumption = 1;
        public float speed = 100f;
        public GameObject Fire_VFX = null;
        public Vector3 Fire_VFX_offset = Vector3.zero;
        public Engine.Audio_Info[] SFX_Fire = null;

        public Vector2Int bullet_grid = new Vector2Int(1, 1);
        public float bullet_grid_offset_H = 3f;
        public float bullet_grid_offset_V = 3f;
    }
    [System.Serializable]
    public class Laser_Info {
        public float damage_per_sec = 1;
        public float energy_consumption_per_sec = 1;
        public Engine.Audio_Info[] SFX_Fire = null;
        public Engine.Audio_Info[] SFX_Continous = null;

        public float laser_speed = 50f;
        public float laser_max_length = 300f;
        public float laser_prefab_length = 20f;
        public GameObject Laser_Hit_FX = null;
    }

    [HideInInspector]
    public Fractions fraction = Fractions.None;
    public enum Fractions { None, Player, Enemy };
    public enum Hit_test_result { Unknown, Ship_SameFraction, Ship_OtherFraction, Bullet_SameFraction, Bullet_OtherFraction, Destructible_Environment };

    float fire_delay_timer = -1f;
    float bullet_col_half_offset = 0f;
    float bullet_row_half_offset = 0f;
    float initial_gun_Y = 0f;

    Vector3 laser_initial_scaling = Vector3.one;

    AudioSource aud1 = null;
    AudioSource aud2 = null;

    // Start is called before the first frame update
    void Start()
    {
        aud1 = gameObject.AddComponent<AudioSource>();
        aud2 = gameObject.AddComponent<AudioSource>();
        aud1.volume = Global_Settings.Volume_SFX;
        aud2.volume = Global_Settings.Volume_SFX;
        aud2.loop = true;
        initial_gun_Y = transform.position.y;

        bullet_col_half_offset = ((directional_settings.bullet_grid.x - 1) * directional_settings.bullet_grid_offset_H) / 2f;
        bullet_row_half_offset = ((directional_settings.bullet_grid.y - 1) * directional_settings.bullet_grid_offset_V) / 2f;

        if (weapon.weapon_type == Weapon_Types.laser && laser_settings != null) {
            if (weapon.bullet_prefab.scene.name == null) { 
                weapon.bullet_prefab = Instantiate(weapon.bullet_prefab);
            }
            laser_initial_scaling = weapon.bullet_prefab.transform.localScale;
            weapon.bullet_prefab.transform.SetParent(transform);
            weapon.bullet_prefab.transform.localPosition = Vector3.zero;
            weapon.bullet_prefab.transform.localRotation = Quaternion.identity;
            weapon.bullet_prefab.transform.localScale = laser_initial_scaling;
            weapon.bullet_prefab.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        fire_delay_timer -= Time.deltaTime;

        if (weapon.weapon_type == Weapon_Types.laser && weapon.bullet_prefab.activeSelf) { Update_Laser(); }
    }

    public bool Fire() {
        bool res = false;
        if (weapon.weapon_type == Weapon_Types.directional) res = Fire_Directional();
        if (weapon.weapon_type == Weapon_Types.laser) res = Fire_Laser();
        return res;
    }
    public void Fire_Stop() {
        if (weapon.weapon_type == Weapon_Types.laser) {
            aud2.Stop();
            weapon.bullet_prefab.SetActive(false);
        }
    }

    public bool Fire_Directional() {
        if (fire_delay_timer <= 0f || fraction == Fractions.Enemy) {
            fire_delay_timer = directional_settings.fire_delay;

            for (int x = 0; x < directional_settings.bullet_grid.x; x++) {
                for (int y = 0; y < directional_settings.bullet_grid.y; y++) {
                    var b = Instantiate(weapon.bullet_prefab);

                    Vector3 offset = Vector3.zero;
                    offset.x = (directional_settings.bullet_grid_offset_H * x) - bullet_col_half_offset;
                    offset.z = (directional_settings.bullet_grid_offset_V * y) - bullet_row_half_offset;
                    b.transform.position = transform.position + offset;
                    b.transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
                    if (weapon.project_Y_0) {
                        var cam = Engine.inst.camera_main.GetComponent<Camera>();
                        var pos = cam.WorldToScreenPoint(transform.position);
                        pos.z = cam.transform.position.y; //Distance from camera
                        b.transform.position = cam.ScreenToWorldPoint(pos);
                    }
                    if (weapon.force_Y_0) {
                        b.transform.position = new Vector3(b.transform.position.x, initial_gun_Y, b.transform.position.z);
                    }

                    var bullet = b.GetComponent<Bullet>();
                    if (bullet == null) return false;
                    bullet.speed = transform.forward * directional_settings.speed;
                    bullet.speed.y = 0f;
                    bullet.gun = this;
                }
            }

            if (directional_settings.Fire_VFX != null) {
                var f_vfx = Instantiate(directional_settings.Fire_VFX);
                f_vfx.transform.position = transform.position + directional_settings.Fire_VFX_offset;
            }

            Play_Sound(0);
            return true;
        } else {
            return false;
        }
    }

    public bool Fire_Laser() {
        if (!weapon.bullet_prefab.activeSelf) { 
            weapon.bullet_prefab.SetActive(true);
            weapon.bullet_prefab.transform.localScale = new Vector3(laser_initial_scaling.x, laser_initial_scaling.y, 0f);
            Play_Sound(0); Play_Sound(1);
        }
        return true;
    }
    void Update_Laser() {
        var target_scaling = laser_initial_scaling;
        var laser_tr = weapon.bullet_prefab.transform;
        RaycastHit hit;
        GameObject hit_go = null;
        var b = Physics.Raycast(laser_tr.position, laser_tr.forward, out hit, laser_settings.laser_max_length);
        if (b) {
            if (hit.point.z > Engine.inst.camera_main.position.z + Global_Settings.bullet_limits_y.y) {
                //Too far, ignore hit
                b = false;
                hit = default(RaycastHit);
            } else {
                target_scaling.z = Vector3.Distance(laser_tr.position, hit.point);
                target_scaling.z = target_scaling.z / laser_settings.laser_prefab_length;
                target_scaling.z = target_scaling.z * laser_initial_scaling.z;
                hit_go = hit.transform.gameObject;
            }
        } else {
            target_scaling.z = laser_settings.laser_max_length / laser_settings.laser_prefab_length;
            target_scaling.z = target_scaling.z * laser_initial_scaling.z;
        }

        if (target_scaling.z < laser_tr.localScale.z) {
            laser_tr.localScale = target_scaling;
        } else {
            var new_scale = Vector3.MoveTowards(laser_tr.localScale, target_scaling, laser_settings.laser_speed * Time.deltaTime);
            laser_tr.localScale = new_scale;
        }

        //Hit detection
        if (b && hit_go != null) {
            var real_laser_length = laser_tr.localScale.z / laser_initial_scaling.z * laser_settings.laser_prefab_length;
            if (hit.distance <= real_laser_length) Test_Hit(hit_go, hit.point);
        } else {
            //Hit detection 2D
            var laser_length = laser_tr.localScale.z * laser_settings.laser_prefab_length / laser_initial_scaling.z;
            Vector3 laser_LU = laser_tr.position + (laser_tr.right * -0.5f);
            Vector3 laser_RB = laser_tr.position + (laser_tr.forward * laser_length) + (laser_tr.right * 0.5f);
            Test_Hit_2D(laser_LU, laser_RB);
        }

        laser_tr.rotation = Quaternion.identity;
        if (weapon.force_Y_0) {
            laser_tr.position = new Vector3(laser_tr.position.x, initial_gun_Y, laser_tr.position.z);
        }
        
    }

    public float energy_consumed {
        get {
            if (weapon.weapon_type == Weapon_Types.laser) {
                return Time.deltaTime * laser_settings.energy_consumption_per_sec;
            } else {
                return directional_settings.energy_consumption;
            }
        }
    }
    public float damage_inflicted {
        get {
            if (weapon.weapon_type == Weapon_Types.laser) {
                return Time.deltaTime * laser_settings.damage_per_sec;
            } else {
                return directional_settings.damage;
            }
        }
    }

    public Hit_test_result Test_Hit(GameObject colided_with, Vector3 hit_point) {
        var p = colided_with.GetComponent<Player>();
        if (p != null) {
            if (fraction == Gun.Fractions.Player) return Hit_test_result.Ship_SameFraction;

            p.Damage( damage_inflicted, hit_point );
            return Hit_test_result.Ship_OtherFraction;
        }
        
        var d = colided_with.GetComponent<Destructible3D>();
        if (d != null) {
            if (fraction == Gun.Fractions.Player) { d.Hit(damage_inflicted); }
            return Hit_test_result.Destructible_Environment;
        }

        var e = colided_with.GetComponent<Ship_Enemy>();
        if (e != null) {
            if (fraction == Gun.Fractions.Enemy) return Hit_test_result.Ship_SameFraction;

            e.Damage( damage_inflicted, hit_point );
            return Hit_test_result.Ship_OtherFraction;
        }

        var b = colided_with.GetComponent<Bullet>();
        if (b != null) {
            if (fraction == b.gun.fraction)
                return Hit_test_result.Bullet_SameFraction;
            else
                return Hit_test_result.Bullet_OtherFraction;
        }

        //Destroy(gameObject);
        return Hit_test_result.Unknown;
    }

    public bool Test_Hit_2D(Vector3 bullet_pos_LU, Vector3 bullet_pos_RD) {
        var cam = Engine.inst.camera_main.GetComponent<Camera>();
        
        var bullet_pos_LU_2D = cam.WorldToScreenPoint(bullet_pos_LU);
        var bullet_pos_RD_2D = cam.WorldToScreenPoint(bullet_pos_RD);
        var r_bullet = new Rect(bullet_pos_LU_2D, bullet_pos_RD_2D - bullet_pos_LU_2D);
        
        foreach (var d in Destructible2D.destructibles_list) {
            if (d.colliders == null || d.colliders.Length == 0) continue;

            bool hit = false;
            foreach (var c in d.colliders) {
                if (c.GetType() == typeof(BoxCollider)) {
                    var box_col = (BoxCollider)c;
                    var col_pos_LU = new Vector3(box_col.bounds.min.x, box_col.transform.position.y + box_col.center.y, box_col.bounds.min.z);
                    var col_pos_RD = new Vector3(box_col.bounds.max.x, box_col.transform.position.y + box_col.center.y, box_col.bounds.max.z);
                    var col_pos_LU_2D = cam.WorldToScreenPoint(col_pos_LU);
                    var col_pos_RD_2D = cam.WorldToScreenPoint(col_pos_RD);
                    var r_col = new Rect(col_pos_LU_2D, col_pos_RD_2D - col_pos_LU_2D);        
                    if (r_bullet.Overlaps(r_col)) {
                        hit = true; break;
                    }
                }
                else if (c.GetType() == typeof(CapsuleCollider)) {
                    var scale = c.gameObject.transform.localScale.y;
                    var capsule = (CapsuleCollider)c;
                    var pos = c.gameObject.transform.position + (capsule.center * scale);
                    var posU = c.gameObject.transform.position + (Vector3.forward * capsule.radius * scale);
                    var pos_2D = cam.WorldToScreenPoint(pos);
                    var posU_2D = cam.WorldToScreenPoint(posU);
                    var radius_2D = posU_2D.y - pos_2D.y;
                    var LT = new Vector2(r_bullet.xMin, r_bullet.yMin);
                    var LB = new Vector2(r_bullet.xMin, r_bullet.yMax);
                    var RT = new Vector2(r_bullet.xMax, r_bullet.yMin);
                    var RB = new Vector2(r_bullet.xMax, r_bullet.yMax);
                    if (Vector2.Distance(LT, pos_2D) <= radius_2D) { hit = true; break; }
                    if (Vector2.Distance(LB, pos_2D) <= radius_2D) { hit = true; break; }
                    if (Vector2.Distance(RT, pos_2D) <= radius_2D) { hit = true; break; }
                    if (Vector2.Distance(RB, pos_2D) <= radius_2D) { hit = true; break; }
                }
            }
            if (hit) { d.Hit(directional_settings.damage); return true; }
        }
        return false;
    }

    void Play_Sound(int n) {
        if (n == 0) {
            if (weapon.weapon_type == Weapon_Types.directional) {
                if (directional_settings.SFX_Fire != null && directional_settings.SFX_Fire.Length > 0) {
                    var r = Random.Range(0, directional_settings.SFX_Fire.Length);
                    aud1.PlayOneShot(directional_settings.SFX_Fire[r].clip, directional_settings.SFX_Fire[r].VolumeScale);
                }
            } else if (weapon.weapon_type == Weapon_Types.laser) {
                if (laser_settings.SFX_Fire != null && laser_settings.SFX_Fire.Length > 0) {
                    var r = Random.Range(0, laser_settings.SFX_Fire.Length);
                    aud1.PlayOneShot(laser_settings.SFX_Fire[r].clip, laser_settings.SFX_Fire[r].VolumeScale);
                }
            }
        } else if (n == 1) {
            if (weapon.weapon_type == Weapon_Types.laser) {
                if (laser_settings.SFX_Continous != null && laser_settings.SFX_Continous.Length > 0) {
                    var r = Random.Range(0, laser_settings.SFX_Continous.Length);
                    aud2.clip = laser_settings.SFX_Continous[r].clip;
                    aud2.volume = Global_Settings.Volume_SFX * laser_settings.SFX_Continous[r].VolumeScale;
                    aud2.Play();
                }
            }
        }
    }

    public bool Is_Playing_Sound {
        get {
            return aud1.isPlaying || aud2.isPlaying;
        }
    }
}
