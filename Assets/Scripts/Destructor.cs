using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructor : MonoBehaviour
{
    public float Destroy_After_Time = 0f;

    float start_time = 0f;

    // Start is called before the first frame update
    void Start()
    {
        start_time = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Mathf.Approximately(Destroy_After_Time, 0f)) {
            if (Time.time > start_time + Destroy_After_Time) {
                Destroy(gameObject); return;
            }
        }
    }
}
