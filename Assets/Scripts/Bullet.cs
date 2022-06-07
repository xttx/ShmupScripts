using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 speed = Vector3.zero;

    [HideInInspector]
    public Gun gun = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += speed * Time.deltaTime;

        //Check limits
        var pos = transform.position;
        var camera_pos = Engine.inst.camera_main.transform.position;
        var x_min = camera_pos.x + Global_Settings.bullet_limits_x.x;
        var x_max = camera_pos.x + Global_Settings.bullet_limits_x.y;
        var y_min = camera_pos.z + Global_Settings.bullet_limits_y.x;
        var y_max = camera_pos.z + Global_Settings.bullet_limits_y.y;
        if (pos.x < x_min) { Destroy(gameObject); return; }
        if (pos.x > x_max) { Destroy(gameObject); return; }
        if (pos.z < y_min) { Destroy(gameObject); return; }
        if (pos.z > y_max) { Destroy(gameObject); return; }

        //Debug.DrawRay(camera_pos, transform.position - camera_pos, Color.red, 0.1f);
        //Debug.DrawRay(transform.position, transform.position - camera_pos, Color.red, 0.1f);
    }

    void OnCollisionEnter(Collision collision) {
        var g = collision.gameObject;
        
        var res = gun.Test_Hit(g);
        if (res == Gun.Hit_test_result.Ship_OtherFraction) {
            if (!gun.weapon.pass_through_enemies) { Destroy(gameObject); return; }
        }

        if (res == Gun.Hit_test_result.Unknown) {
            //Probably environment
            Destroy(gameObject);
        }

        //var p = g.GetComponent<Player>();
        //if (p != null) {
        //    if (gun.fraction == Gun.Fractions.Player) return;

        //    p.Damage(gun.directional_settings.damage);
        //    if (!gun.weapon.pass_through_enemies) { Destroy(gameObject); return; }
        //}

        //var e = g.GetComponent<Ship_Enemy>();
        //if (e != null) {
        //    if (gun.fraction == Gun.Fractions.Enemy) return;

        //    e.Damage(gun.directional_settings.damage);
        //    if (!gun.weapon.pass_through_enemies) { Destroy(gameObject); return; }
        //}

        //var b = g.GetComponent<Bullet>();
        //if (b != null) {
        //    //TODO: We have yet to handle bullet-to-bullet collision
        //    return;
        //}

        //Destroy(gameObject);
    }
}
