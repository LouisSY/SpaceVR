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
        float scaledIntensity = main_sun.intensity / (PlayerProxy.transform.position.sqrMagnitude);

        faked_sun.intensity = scaledIntensity * LightDistance * LightDistance;
        transform.SetPositionAndRotation(sun_dir * LightDistance, Quaternion.identity);
    }
}
