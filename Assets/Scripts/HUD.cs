using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public int Player_Index = 0;

    Scrollbar Energy_bar = null;
    TMPro.TMP_Text HP_Text = null;

    // Start is called before the first frame update
    void Start()
    {
        Energy_bar = transform.GetChild(2).GetComponent<Scrollbar>();
        HP_Text = transform.GetChild(3).GetComponent<TMPro.TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.activeSelf) {
            if (Engine.inst.players_spawned[Player_Index]) gameObject.SetActive(true);
        }

        if (gameObject.activeSelf) {
            var p = Engine.inst.players_cmp[Player_Index];
            if (p != null) {
                HP_Text.text = p.health.ToString();
                Energy_bar.size = (float)p.energy / (float)p.energy_max;
            }
        }
    }
}
