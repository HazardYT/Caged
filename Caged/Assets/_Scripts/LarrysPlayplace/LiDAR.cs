#pragma warning disable 0618
using mudz;
using UnityEngine;
using UnityEngine.InputSystem;

public class LiDAR : MonoBehaviour
{
    [SerializeField] ParticleSystem liDAR;
    [SerializeField] Transform rayGunnnnnn;
    Camera cam;

    float choice;

    [SerializeField] GameObject line;
    private byte mode = 0;
    public int numberOfRays = 10;
    public float coneAngle = 45f;
    public float maxDistance = 10f;
    public LayerMask layerMask;

    Vector3 noVelocity = new Vector3(0, 0, 0);

    [SerializeField] private float lineScanMinAngle = -35f;
    [SerializeField] private float lineScanMaxAngle = 35f;
    [SerializeField] private float lineScanSpeed = 10f;
    public int lineScanNumberOfRays = 10;
    public float lineScanConeAngle = 45f;

    void Awake(){
        cam = GetComponent<Camera>();
    }

    void Update(){
        choice = Random.Range(0, 11);
        Debug.Log(choice);
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
                PlaceDot(hit.point, choice >= 5f ? new Color(0, 151, 255, 255) : new Color(0, 255, 242, 255));
            }
            else
            {
                Debug.DrawRay(ray.origin, direction * maxDistance, Color.red);
            }
        }
    }

    void LineScan() {
        float currentAngle = Mathf.Lerp(lineScanMinAngle, lineScanMaxAngle, Mathf.PingPong(Time.time * lineScanSpeed, 1f));
        Vector3 forward = cam.transform.forward;
        Quaternion startRotation = Quaternion.AngleAxis(-lineScanConeAngle / 2, cam.transform.right);
        Quaternion currentRotation = Quaternion.AngleAxis(currentAngle, cam.transform.right);

        for (int i = 0; i < lineScanNumberOfRays; i++) {
            float angleStep = lineScanConeAngle / (lineScanNumberOfRays - 1);
            Quaternion rayRotation = Quaternion.AngleAxis(angleStep * i - (lineScanConeAngle / 2), cam.transform.up) * currentRotation;
            Vector3 direction = rayRotation * startRotation * forward;

            Ray ray = new Ray(transform.position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance, layerMask)) {
                DrawLine(hit.point);
                Debug.DrawLine(ray.origin, hit.point, Color.green);
                PlaceDot(hit.point, choice >= 5f ? Color.blue : Color.cyan);
            }
            else {
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
                PlaceDot(hit.point, choice >= 5f ? Color.blue : Color.cyan);
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

    void PlaceDot(Vector3 pos, Color32 color){
        float size = 0.05f;
        liDAR.Emit(pos, noVelocity, size, 120, color);
    }
}
#pragma warning restore 0618
