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
        public bool pass_through_enemies = false;
        public bool pass_through_environment = false;
    }
    [System.Serializable]
    public class Directional_Info {
        public float fire_delay = 0.4f;
        public int damage = 1;
        public int energy_consumption = 1;
        public float speed = 100f;
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
    public enum Hit_test_result { Unknown, Ship_SameFraction, Ship_OtherFraction, Bullet_SameFraction, Bullet_OtherFraction };

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

        //Update laser
        if (weapon.weapon_type == Weapon_Types.laser && weapon.bullet_prefab.activeSelf) {
            var target_scaling = laser_initial_scaling;
            var laser_tr = weapon.bullet_prefab.transform;
            RaycastHit hit;
            GameObject hit_go = null;
            var b = Physics.Raycast(laser_tr.position, laser_tr.forward, out hit, laser_settings.laser_max_length);
            if (b) {
                target_scaling.z = Vector3.Distance(laser_tr.position, hit.point);
                target_scaling.z = target_scaling.z / laser_settings.laser_prefab_length;
                target_scaling.z = target_scaling.z * laser_initial_scaling.z;
                hit_go = hit.transform.gameObject;
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
                if (hit.distance <= real_laser_length) Test_Hit(hit_go);
            }

            laser_tr.rotation = Quaternion.identity;
            if (weapon.force_Y_0) {
                laser_tr.position = new Vector3(laser_tr.position.x, initial_gun_Y, laser_tr.position.z);
            }
        }
    }

    public bool Fire() {
        if (weapon.weapon_type == Weapon_Types.directional) return Fire_Directional();
        if (weapon.weapon_type == Weapon_Types.laser) return Fire_Laser();
        return false;
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
                    if (weapon.force_Y_0) {
                        b.transform.position = new Vector3(b.transform.position.x, initial_gun_Y, b.transform.position.z);
                    }

                    var bullet = b.GetComponent<Bullet>();
                    if (bullet == null) return false;
                    bullet.speed = transform.forward * directional_settings.speed;
                    bullet.gun = this;
                }
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

    public Hit_test_result Test_Hit(GameObject colided_with) {
        var p = colided_with.GetComponent<Player>();
        if (p != null) {
            if (fraction == Gun.Fractions.Player) return Hit_test_result.Ship_SameFraction;

            p.Damage( damage_inflicted );
            return Hit_test_result.Ship_OtherFraction;
        }

        var e = colided_with.GetComponent<Ship_Enemy>();
        if (e != null) {
            if (fraction == Gun.Fractions.Enemy) return Hit_test_result.Ship_SameFraction;

            e.Damage( damage_inflicted );
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
}
