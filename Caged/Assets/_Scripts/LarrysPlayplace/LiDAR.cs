#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mudz;

public class LiDAR : MonoBehaviour
{
    [SerializeField] ParticleSystem liDAR_Obj;
    [SerializeField] ParticleSystem liDAR_Enemy;
    Camera cam;

    Vector3 noVelocity = new Vector3(0, 0, 0);

    void Awake(){
        cam = GetComponent<Camera>();
    }

    void Start(){
        PlaceDot(transform.position, false);
        PlaceDot(new Vector3(
            transform.position.x + 2, 
            transform.position.y, 
            transform.position.z
            ), true);
    }

    void Scan(){
        //use a lot of physics.raycast to find an object and place a dot on the raycast hit.point
    }

    void PlaceDot(Vector3 pos, bool isEnemy){
        float size = Random.Range(0.06f, 0.1f);
        Color32 chosenColor = new Color32(255, 255, 255, 255);
        if(isEnemy){
            liDAR_Enemy.Emit(pos, noVelocity, size, 120, chosenColor);
        } else{
            liDAR_Obj.Emit(pos, noVelocity, size, 120, chosenColor);
        }
    }
}
#pragma warning restore 0618