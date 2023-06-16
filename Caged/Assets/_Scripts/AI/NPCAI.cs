using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
public class NPCAI : MonoBehaviourPun
{
    public int searchRadius;
    public NavMeshAgent agent;
    public Transform Target;
    public Collider[] SphereCheck;
    public LayerMask playerMask;
    public bool saved = false;

    public void Start(){
        if (!photonView.IsMine){
            enabled = false;
            return;
        }
    }
    public void Update(){
        if (!saved){
            SphereCheck = new Collider[5];
            Physics.OverlapSphereNonAlloc(transform.position, searchRadius, SphereCheck, playerMask);
            foreach(Collider player in SphereCheck){
                if (player == null) {continue;}
                if (player.CompareTag("Van")) {
                    Target = player.transform;
                    if (!saved) { saved = true; StartCoroutine(Saved()); }
                    return; 
                }
                if (Physics.Raycast(transform.position, player.transform.position - transform.position, out RaycastHit hit, searchRadius, playerMask)){
                    Target = hit.transform;
                    FollowPlayer();
                }
            }
        }
    }
    public void FollowPlayer(){
        if (Vector3.Distance(transform.position, Target.position) > 2){
            agent.SetDestination(Target.position);
        }
    }
    public IEnumerator Saved(){
        agent.SetDestination(Target.position);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, Target.position) < 3f);
        photonView.RPC(nameof(SaveNPC),RpcTarget.AllBufferedViaServer);
        yield return new WaitForSeconds(0.5f);
        PhotonNetwork.Destroy(photonView);
    }
    [PunRPC]
    public void SaveNPC(){
        GameManager manager = GameObject.FindObjectOfType<GameManager>();
        manager._savedNPCS++;
        manager._moneyCollected += 1000;
        manager.moneyText.text = "$" + manager._moneyCollected;
    }
}
