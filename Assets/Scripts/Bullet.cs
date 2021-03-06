using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 speed = Vector3.zero;

    [HideInInspector]
    public Gun gun = null;

    Collider col = null;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
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

        //Check collision 2D
        if (col != null) {
            var bullet_pos_LU = new Vector3(col.bounds.min.x, col.transform.position.y, col.bounds.min.z);
            var bullet_pos_RD = new Vector3(col.bounds.max.x, col.transform.position.y, col.bounds.max.z);
            gun.Test_Hit_2D(bullet_pos_LU, bullet_pos_RD);
        }
    }

    void OnCollisionEnter(Collision collision) {
        Vector3 hit_point = Vector3.zero;
        if (collision.contactCount > 0) { hit_point = collision.GetContact(0).point; }

        var res = gun.Test_Hit(collision.gameObject, hit_point);
        if (res == Gun.Hit_test_result.Ship_OtherFraction) {
            if (!gun.weapon.pass_through_enemies) { Destroy(gameObject); return; }
        }
        if (res == Gun.Hit_test_result.Destructible_Environment) {
            if (!gun.weapon.pass_through_environment) { Destroy(gameObject); return; }
        }
        
        if (res == Gun.Hit_test_result.Unknown) {
            //Probably environment
            if (!gun.weapon.pass_through_environment) { Destroy(gameObject); return; }
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
