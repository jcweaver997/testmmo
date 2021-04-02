using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    public int start = 10;
    public float perSec = 2;
    private float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < start; i ++)
        {
            GameObject.Instantiate(prefab);
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer>=1/perSec)
        {
            GameObject.Instantiate(prefab);
            timer = 0;
        }
    }
}
