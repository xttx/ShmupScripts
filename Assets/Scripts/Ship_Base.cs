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

    public GameObject Spawn_On_Death = null;

    float energy_recover_timer = 0f;
    
    Vector3 last_pos = Vector3.zero;
    Vector3 last_speed = Vector3.zero;

    protected Gun[] guns = null;
    protected AudioSource aud = null;
    
    // Start is called before the first frame update
    public void Start_Base()
    {
        aud = GetComponent<AudioSource>();
        guns = GetComponentsInChildren<Gun>();
    }

    // Update is called once per frame
    public void Update_base()
    {
        energy_recover_timer += Time.deltaTime;
        if (energy_recover_timer >= energy_recover_rate) {
            energy_recover_timer = 0;
            if (energy < energy_max) energy += 1;
        }

        last_speed = transform.position - last_pos;
        last_pos = transform.position;
    }

    public virtual void Damage(float d) {
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
    }

    public void Death() {
        if (SFX_Death != null && SFX_Death.Length > 0) {
            var n = Random.Range(0, SFX_Death.Length);
            Engine.Play_Sound_2D(SFX_Death[n]);
        }
        Destroy_Ship();
    }

    public virtual void Destroy_Ship() {
        if (Spawn_On_Death != null) {
            var g = Instantiate(Spawn_On_Death);
            g.transform.position = transform.position;

            var mover = g.GetComponent<Linear_Mover>();
            if (mover != null) { mover.Speed = last_speed / Time.deltaTime; }
        }
        Destroy(gameObject);
    }
}
