#pragma warning disable 0618
using UnityEngine;
using UnityEngine.InputSystem;

public class LiDAR : MonoBehaviour
{
    [SerializeField] ParticleSystem liDAR_Obj;
    [SerializeField] ParticleSystem liDAR_Enemy;
    [SerializeField] Transform rayGunnnnnn;
    Camera cam;

    [SerializeField] GameObject line;
    private byte mode = 0;
    public int numberOfRays = 10;
    public float coneAngle = 45f;
    public float maxDistance = 10f;
    public LayerMask layerMask;

    Vector3 noVelocity = new Vector3(0, 0, 0);

    void Awake(){
        cam = GetComponent<Camera>();
    }
    void Update(){
        if (Mouse.current.leftButton.isPressed)
        {
            if (mode == 0) { ConeScan(); }
            else if (mode == 1) { LineScan(); }
            else if (mode == 2) { SpiralScan(); }
        }

        if (UserInput.instance.FlashlightModePressed)
        {
            if (mode >= 2)
            {
                mode = 0;
            }
            else
            {
                mode++;
            }
        }
    }
    void SpiralScan()
    {
        Vector3 forward = cam.transform.forward;
        Quaternion startRotation = Quaternion.AngleAxis(-coneAngle / 2, transform.up);

        for (int i = 0; i < numberOfRays; i++)
        {
            float inclination = Mathf.Acos(1 - i / (numberOfRays - 1f) * (1 - Mathf.Cos(coneAngle * Mathf.Deg2Rad)));
            float azimuth = 2 * Mathf.PI * i / numberOfRays;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);

            Vector3 direction = new Vector3(x, y, z);
            direction = transform.rotation * startRotation * direction;

            Ray ray = new Ray(transform.position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
            {
                DrawLine(hit.point);
                Debug.DrawLine(ray.origin, hit.point, Color.green);
                PlaceDot(hit.point, false);
            }
            else
            {
                Debug.DrawRay(ray.origin, direction * maxDistance, Color.red);
            }
        }
    }
    void LineScan(){
         Vector3 forward = cam.transform.forward;
         Quaternion startRotation = Quaternion.AngleAxis(-coneAngle / 2, transform.up);

         for (int i = 0; i < numberOfRays; i++)
         {
             float angleStep = coneAngle / (numberOfRays - 1);
             Quaternion currentRotation = Quaternion.AngleAxis(angleStep * i, transform.up);
             Vector3 direction = currentRotation * startRotation * forward;

             Ray ray = new Ray(transform.position, direction);
             RaycastHit hit;

             if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
             {
                DrawLine(hit.point);
                Debug.DrawLine(ray.origin, hit.point, Color.green);
                PlaceDot(hit.point, false);
            }
             else
             {
                 Debug.DrawRay(ray.origin, direction * maxDistance, Color.red);
             }
         }
    }
    void ConeScan(){
        Vector3 forward = cam.transform.forward;

        for (int i = 0; i < numberOfRays; i++)
        {
            float randomInclination = Random.Range(0, coneAngle * Mathf.Deg2Rad);
            float randomAzimuth = Random.Range(0, 2 * Mathf.PI);

            float x = Mathf.Sin(randomInclination) * Mathf.Cos(randomAzimuth);
            float y = Mathf.Sin(randomInclination) * Mathf.Sin(randomAzimuth);
            float z = Mathf.Cos(randomInclination);

            Vector3 direction = new Vector3(x, y, z);
            direction = transform.rotation * direction;

            Ray ray = new Ray(transform.position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
            {
                DrawLine(hit.point);
                Debug.DrawLine(ray.origin, hit.point, Color.green);
                PlaceDot(hit.point, false);
            }
            else
            {
                Debug.DrawRay(ray.origin, direction * maxDistance, Color.red);
            }
        }
    }

    void DrawLine(Vector3 end)
    {
        GameObject lineObject = Instantiate(line, rayGunnnnnn);
        LineRenderer lineRend = lineObject.GetComponent<LineRenderer>();
        lineRend.SetPosition(0, rayGunnnnnn.position);
        lineRend.SetPosition(1, end);

        Destroy(lineObject, 0.008f);
    }

    void PlaceDot(Vector3 pos, bool isEnemy){
        float size = 0.05f;
        Color32 chosenColor = new Color32(255, 255, 255, 255);
        if(isEnemy){
            liDAR_Enemy.Emit(pos, noVelocity, size, 120, chosenColor);
        } else{
            liDAR_Obj.Emit(pos, noVelocity, size, 120, chosenColor);
        }
    }
}
#pragma warning restore 0618