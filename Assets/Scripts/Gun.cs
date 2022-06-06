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

        public bool pass_through_enemies = false;
        public bool pass_through_environment = false;
    }
    public enum Weapon_Types { directional, laser };

    [HideInInspector]
    public Fractions fraction = Fractions.None;
    public enum Fractions { None, Player, Enemy };

    float fire_delay_timer = -1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        fire_delay_timer -= Time.deltaTime;
    }

    public bool Fire() {
        if (fire_delay_timer <= 0f || fraction == Fractions.Enemy) {
            fire_delay_timer = weapon.fire_delay;

            var b = Instantiate(weapon.bullet_prefab);
            b.transform.position = transform.position;
            
            var bullet = b.GetComponent<Bullet>();
            if (bullet == null) return false;
            //bullet.speed = new Vector3(0f, 0f, weapon.speed);
            bullet.speed = transform.forward * weapon.speed;
            bullet.gun = this;
            return true;
        } else {
            return false;
        }
    }
}
