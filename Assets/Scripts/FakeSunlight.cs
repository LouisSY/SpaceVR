using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FakeSunlight : MonoBehaviour
{
    public GameObject Sun;
    public GameObject PlayerProxy;
    public GameObject PlayerHost;
    public float LightDistance;

    public float BaseIntensity;

    private Light faked_sun;
    private Light main_sun;
    // Start is called before the first frame update
    void Start()
    {
        faked_sun = GetComponent<Light>();
        faked_sun.intensity = BaseIntensity;

        main_sun = Sun.GetComponent<Light>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 sun_dir = PlayerProxy.transform.InverseTransformPoint(Sun.transform.position);
        sun_dir = PlayerHost.transform.TransformDirection(sun_dir.normalized);
        var q = Quaternion.FromToRotation(Vector3.down, sun_dir);
        
        float scaledIntensity = main_sun.intensity / (PlayerProxy.transform.position.sqrMagnitude);
        float scale = 5f * (Sun.transform.localScale.x / PlayerProxy.transform.position.magnitude);

        faked_sun.intensity = scaledIntensity * LightDistance * LightDistance;
        transform.localScale = new Vector3(scale,scale,scale);
        transform.SetPositionAndRotation(sun_dir * LightDistance, q);
    }
}
