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

        Check_Collision_2D();
    }

    void Check_Collision_2D() {
        if (col == null) return;

        var cam = Engine.inst.camera_main.GetComponent<Camera>();
        
        var bullet_pos_LU = new Vector3(col.bounds.min.x, col.transform.position.y, col.bounds.min.z);
        var bullet_pos_RD = new Vector3(col.bounds.max.x, col.transform.position.y, col.bounds.max.z);
        var bullet_pos_LU_2D = cam.WorldToScreenPoint(bullet_pos_LU);
        var bullet_pos_RD_2D = cam.WorldToScreenPoint(bullet_pos_RD);
        var r_bullet = new Rect(bullet_pos_LU_2D, bullet_pos_RD_2D - bullet_pos_LU_2D);
        
        foreach (var d in Destructible.destructibles_list) {
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
                    var capsule = (CapsuleCollider)c;
                    var pos = c.gameObject.transform.position + capsule.center;
                    var posU = c.gameObject.transform.position + (Vector3.forward * capsule.radius);
                    var pos_2D = cam.WorldToScreenPoint(pos);
                    var posU_2D = cam.WorldToScreenPoint(posU);
                    var radius_2D = posU_2D.y - pos_2D.y;
                    //TODO
                }
            }
            if (hit) { d.Hit(gun.directional_settings.damage); break; }
        }

        //Test 2D destructible system using raycast, and it works bad
        //Debug.DrawRay(camera_pos, transform.position - camera_pos, Color.red, 0.1f);
        //Debug.DrawRay(transform.position, transform.position - camera_pos, Color.red, 0.1f);
        //RaycastHit hit;
        //var b = Physics.Raycast(transform.position, transform.position - camera_pos, out hit, 1000f);
        //if (b) {
        //    var dstr = hit.transform.GetComponent<Destructible>();
        //    if (dstr != null) dstr.Hit(gun.directional_settings.damage);
        //}
    }

    void OnCollisionEnter(Collision collision) {
        Vector3 hit_point = Vector3.zero;
        if (collision.contactCount > 0) { hit_point = collision.GetContact(0).point; }

        var res = gun.Test_Hit(collision.gameObject, hit_point);
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
