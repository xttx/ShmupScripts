using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Weapon_Info weapon = null;
    [System.Serializable]
    public class Weapon_Info {
        public Weapon_Types weapon_type = Weapon_Types.directional;
        public GameObject bullet_prefab = null;
        public Vector3 spawn_offset = Vector3.zero;
        public float fire_delay = 0.4f;
        public int damage = 1;
        public float speed = 100f;

        public Vector2Int bullet_grid = new Vector2Int(1, 1);
        public float bullet_grid_offset_H = 3f;
        public float bullet_grid_offset_V = 3f;

        public bool pass_through_enemies = false;
        public bool pass_through_environment = false;
    }
    public enum Weapon_Types { directional, laser };

    [HideInInspector]
    public Fractions fraction = Fractions.None;
    public enum Fractions { None, Player, Enemy };

    float fire_delay_timer = -1f;
    float bullet_col_half_offset = 0f;
    float bullet_row_half_offset = 0f;

    // Start is called before the first frame update
    void Start()
    {
        bullet_col_half_offset = ((weapon.bullet_grid.x - 1) * weapon.bullet_grid_offset_H) / 2f;
        bullet_row_half_offset = ((weapon.bullet_grid.y - 1) * weapon.bullet_grid_offset_V) / 2f;
    }

    // Update is called once per frame
    void Update()
    {
        fire_delay_timer -= Time.deltaTime;
    }

    public bool Fire() {
        if (fire_delay_timer <= 0f || fraction == Fractions.Enemy) {
            fire_delay_timer = weapon.fire_delay;

            for (int x = 0; x < weapon.bullet_grid.x; x++) {
                for (int y = 0; y < weapon.bullet_grid.y; y++) {
                    var b = Instantiate(weapon.bullet_prefab);

                    Vector3 offset = Vector3.zero;
                    offset.x = (weapon.bullet_grid_offset_H * x) - bullet_col_half_offset;
                    offset.z = (weapon.bullet_grid_offset_V * y) - bullet_row_half_offset;
                    b.transform.position = transform.position + offset;
            
                    var bullet = b.GetComponent<Bullet>();
                    if (bullet == null) return false;
                    bullet.speed = transform.forward * weapon.speed;
                    bullet.gun = this;
                }
            }

            return true;
        } else {
            return false;
        }
    }
}
