using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public float HP = 1f;

    public static List<Destructible> destructibles_list = new List<Destructible>();

    // Start is called before the first frame update
    void Start()
    {
        destructibles_list.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hit(float d) {
        HP -= d;
        if (HP <= 0f) { 
            if (destructibles_list.Contains(this)) destructibles_list.Remove(this);
            Destroy(gameObject); 
        }
    }
}
