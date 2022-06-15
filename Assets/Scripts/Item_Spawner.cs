using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Spawner : MonoBehaviour
{
    public float item_offset = 15f;
    public spawned_item[] spawned_items = new spawned_item[]{ };
    [System.Serializable]
    public class spawned_item {
        public GameObject item_prefab = null;
        public int override_quantity = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Spawn() {
        if (spawned_items == null) return;
        if (spawned_items.Length == 0) return;

        List<Vector3> positions = new List<Vector3>();
        if (spawned_items.Length == 1) {
            positions.Add( Vector3.zero );
        }
        else if (spawned_items.Length == 2) {
            positions.Add( new Vector3(-item_offset, 0f, 0f) );
            positions.Add( new Vector3(item_offset, 0f, 0f) );
        }
        else {
            float angle_step = 360f / (float)spawned_items.Length;
            for (int n = 0; n < spawned_items.Length; n++) {
                float angle = (float)n * angle_step;
                float rad = angle * Mathf.Deg2Rad;
                var x = Mathf.Sin(rad) * item_offset;
                var y = Mathf.Cos(rad) * item_offset;
                positions.Add( new Vector3(x, 0f, y) );
            }
        }

        for (int n = 0; n < spawned_items.Length; n++) {
            var item = spawned_items[n];
            var item_obj = Instantiate(item.item_prefab);
            item_obj.transform.position = transform.position + positions[n];
        }

    }
}
