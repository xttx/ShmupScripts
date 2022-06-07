using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Enemy_001 : Ship_Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        Start_Base_Enemy();

        foreach (var g in guns) {
            g.fraction = Gun.Fractions.Enemy;
        }
        aud.volume = Global_Settings.Volume_SFX;
    }

    // Update is called once per frame
    void Update()
    {
        Update_base_enemy();
    }
}
