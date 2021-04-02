using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetInterpolator : MonoBehaviour
{
    Vector3 startpos, targetpos;
    Quaternion startrot, targetrot;
    float time;
    float netDelta = 1.0f / 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time/netDelta<2)
        {
            gameObject.transform.position = Vector3.Lerp(startpos, targetpos, time / netDelta);
            gameObject.transform.rotation = Quaternion.Lerp(startrot, targetrot, time / netDelta);
        }
    }

    public void Set(Vector3 pos, Quaternion quaternion, float netSpeed)
    {
        startpos = gameObject.transform.position;
        startrot = gameObject.transform.rotation;
        time = 0;
        targetpos = pos;
        targetrot = quaternion;
        netDelta = netSpeed;
    }
}
