using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Base : MonoBehaviour
{
    public float health = 10;
    public float energy = 10;
    public float energy_max = 10;
    public float energy_recover_rate = 1f;

    public Engine.Audio_Info[] SFX_Hit = null;
    public Engine.Audio_Info[] SFX_Death = null;

    public GameObject hit_VFX = null;
    public GameObject Spawn_On_Death = null;


    //float energy_recover_timer = 0f;
    
    Vector3 last_pos = Vector3.zero;
    Vector3 last_speed = Vector3.zero;

    protected Gun[] guns = null;
    protected AudioSource aud = null;
    protected Item_Spawner item_spawner = null;
    protected bool Waiting_For_Destroy = false;
    
    // Start is called before the first frame update
    public void Start_Base()
    {
        aud = GetComponent<AudioSource>();
        guns = GetComponentsInChildren<Gun>();
        item_spawner = GetComponent<Item_Spawner>();
    }

    // Update is called once per frame
    public void Update_base()
    {
        if (Waiting_For_Destroy) return;

        if (energy < energy_max) {
            energy += Time.deltaTime * energy_recover_rate;
            if (energy > energy_max) energy = energy_max;
        }

        last_speed = transform.position - last_pos;
        last_pos = transform.position;
    }

    public virtual void Damage(float d, Vector3 hit_point) {
        if (energy > 0) { 
            energy -= d;
            if (energy < 0) {
                health += energy; energy = 0;
            }
        } else {
            health -= d;
        }

        if (health < 0) { health = 0; Death(); }
        else { 
            if (SFX_Hit != null && SFX_Hit.Length > 0) { Engine.Play_Sound_2D(SFX_Hit); }
        }

        if (hit_VFX != null && hit_point != Vector3.zero) {
            Instantiate(hit_VFX, hit_point, Quaternion.identity);
        }
    }

    public void Death() {
        if (SFX_Death != null && SFX_Death.Length > 0) {
            var n = Random.Range(0, SFX_Death.Length);
            Engine.Play_Sound_2D(SFX_Death[n]);
        }
        Destroy_Ship();
    }

    public virtual void Destroy_Ship(bool dont_spawn_vfx = false) {
        Waiting_For_Destroy = true;

        if (!dont_spawn_vfx && Spawn_On_Death != null) {
            var g = Instantiate(Spawn_On_Death);
            g.transform.position = transform.position;

            var mover = g.GetComponent<Linear_Mover>();
            if (mover != null) { mover.Speed = last_speed / Time.deltaTime; }
        }

        if (item_spawner != null) item_spawner.Spawn();

        StartCoroutine( Waiting_For_Destroy_Coroutine() );
    }

    IEnumerator Waiting_For_Destroy_Coroutine() {
        while (true) {
            var wait = false;
            if (aud.isPlaying) {
                wait = true;
            } else {
                foreach (var g in guns) {
                    if (g.fraction == Gun.Fractions.Enemy && g.Is_Playing_Sound) {
                        wait = true; break;
                    }
                }
            }

            if (wait) { yield return null; }
            else { Destroy(gameObject); break; }
        }
    }
}
