using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible2D : MonoBehaviour
{
    public float HP = 1f;
    public Death_Info Death_Settings = new Death_Info();
    [System.Serializable]
    public class Death_Info {
        public string Anim_Trigger = "";
        public GameObject Spawn_Model = null;
        public bool Destroy_This_Object = true;
        public Engine.camera_shake_info Death_Camera_Shake = new Engine.camera_shake_info();
    }

    public static List<Destructible2D> destructibles_list = new List<Destructible2D>();

    [HideInInspector]
    public Collider[] colliders = null;

    bool dead = false;

    // Start is called before the first frame update
    void Start()
    {
        colliders = GetComponents<Collider>();
        destructibles_list.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hit(float d) {
        if (dead) return;

        HP -= d;
        if (HP <= 0f) {
            dead = true;
            if (destructibles_list.Contains(this)) destructibles_list.Remove(this);
            if (Death_Settings.Spawn_Model != null) {
                var g = Instantiate(Death_Settings.Spawn_Model);
                g.transform.position = transform.position;
                g.transform.rotation = transform.rotation;
                Death_Anim(g);
            } else {
                Death_Anim(gameObject);
            }

            if (Death_Settings.Death_Camera_Shake.time >= 0.1f) { Engine.camera_shake = Death_Settings.Death_Camera_Shake; }
            if (Death_Settings.Destroy_This_Object) Destroy(gameObject); 
        }
    }

    public void Death_Anim(GameObject g) {
        var trg = Death_Settings.Anim_Trigger.Trim();
        if (string.IsNullOrWhiteSpace(trg)) return;

        var a = g.GetComponent<Animator>();
        if (a == null) return;

        a.SetTrigger(trg);
    }
}
