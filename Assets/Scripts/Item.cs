using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Global_Settings.item_types item_type = Global_Settings.item_types.Money;
    public int quantity = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Collect(Player p) {
        Destroy(gameObject);
    }
}
