using System.Collections;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DogAI : MonoBehaviourPun
{
    public Vector3 walkPoint;
    bool walkPointSet;
    public float SearchRadius;
    public Animator Anim;
    public NavMeshAgent agent;
    public LayerMask mask;
    public Transform target;
    [SerializeField] Collider[] CheckSphereResults;
    public int _DistractionDelayTime = 10;
    public int _chaseRange;
    public bool isOnTimer;
    public bool Distracted = false;
    public bool chasing = false;
    private bool waitingforwalkpoint = false;
    public float ChaseSpeed;
    public float PatrolSpeed;
    public bool canSeePlayer = true;
    [SerializeField] private Transform SensingPosition;
    public Vector3 roomMinBounds;
    public Vector3 roomMaxBounds;
    public bool _walking;
    public bool _running;
    public bool isSearchingForMeat = false;
    private void Start()
    {
        if (!photonView.IsMine){
            enabled = false;
            return;
        }
    }
    private void Update()
    {
        if (!canSeePlayer && !chasing && !Distracted) {StartCoroutine(WaitForNewWalkPoint()); }
        CheckSphereResults = new Collider[10];
        Physics.OverlapSphereNonAlloc(transform.position, SearchRadius, CheckSphereResults, mask);
        if (!chasing && !Distracted)
        {
            foreach (Collider obj in CheckSphereResults)
            {
                if (obj == null) {continue;}
                RaycastHit hit;
                if (Physics.Raycast(transform.position, obj.transform.position - transform.position, out hit))
                {
                    if (IsInBounds(hit.transform.position)){
                        if (hit.collider.CompareTag("Player"))
                        {
                            target = hit.transform;
                            chasing = true;
                            return;
                        }
                        else if (hit.collider.CompareTag("Item") && !isOnTimer)
                        {
                            if (hit.collider.CompareTag("Item"))
                            {
                                target = hit.transform;
                                Distracted = true;
                                return;
                            }
                            
                        }
                    }
                    else if (hit.collider.CompareTag("Player") && !Distracted)
                    {
                        target = hit.transform;
                        agent.SetDestination(SensingPosition.position);
                        Vector3 lookPos = target.position - transform.position;
                        Quaternion lookRot = Quaternion.LookRotation(lookPos, Vector3.up);
                        float eulerY = lookRot.eulerAngles.y;
                        Quaternion rotation = Quaternion.Euler(0, eulerY, 0);
                        transform.rotation = rotation;
                        canSeePlayer = true;
                        Debug.Log("YEEEHAWWWWW!!!!!!!!!!!!!!!!!!!!!!");
                        if (!isSearchingForMeat)
                        {
                            isSearchingForMeat = true;
                            StartCoroutine(CheckForMeat());
                        }
                    }
                    else
                    {
                        canSeePlayer = false;
                        chasing = false;
                    }
                }
            }
            if (target != null) { Debug.DrawLine(transform.position, target.position, Color.red);}
        }
        if (chasing && !Distracted)
        {
            foreach (Collider obj in CheckSphereResults)
            {
                if (obj == null) {continue;}
                if (Vector3.Distance(transform.position, obj.transform.position) < _chaseRange)
                {
                    if (obj.CompareTag("Player"))
                    {
                        target = obj.transform;
                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, target.position - transform.position, out hit))
                        {
                            if (target.CompareTag("Player") && IsInBounds(target.position))
                            {
                                Debug.DrawLine(transform.position, hit.point, Color.green);
                                walkPointSet = false;
                                agent.speed = ChaseSpeed;
                                ChasePlayer();
                            }
                            else
                            {
                                agent.speed = PatrolSpeed;
                                chasing = false;
                            }
                        }
                    }
                }
            }
        }
        if (Distracted)
        {
            foreach (Collider obj in CheckSphereResults)
            {
                if (obj == null) {continue;}
                if (IsInBounds(obj.transform.position))
                {
                    if (obj.CompareTag("Item"))
                    {
                        agent.SetDestination(target.position);
                        StartCoroutine(GetMeat());
                    }
                }
            }
        }
    }
    private IEnumerator CheckForMeat(){
        InventoryManager playerInv = target.GetComponent<InventoryManager>();
        yield return new WaitForSeconds(2f);

        if (playerInv.Equipped.childCount > 0 && playerInv.Equipped.GetChild(0).name == "Meat"){
                Debug.Log("Have MEAT!!!");
        }
        else{
            yield return new WaitForSeconds(0.5f);
            if (playerInv.Equipped.childCount !> 0 && playerInv.Equipped.GetChild(0).name != "Meat")
            Debug.Log("NO MEAT KILL");
        }
        isSearchingForMeat = false;
    }
    private IEnumerator GetMeat(){
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target.position) <= 2f && target.GetComponent<Rigidbody>().velocity == Vector3.zero);
        agent.speed = ChaseSpeed;
        agent.SetDestination(transform.position);
        target.gameObject.tag = "Untagged";
        PhotonView view = target.transform.GetComponent<PhotonView>();
        view.RequestOwnership();
        yield return new WaitForSeconds(5f);
        Distracted = false;
        isOnTimer = true;
        StartCoroutine(Timer());
        PhotonNetwork.Destroy(view);
        agent.speed = PatrolSpeed;
    }
    private IEnumerator Timer(){
        yield return new WaitForSeconds(_DistractionDelayTime);
        isOnTimer = false;
    }
    private void ChasePlayer()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        float predictionFactor = Mathf.Lerp(0, 0.5f, distanceToTarget / SearchRadius);
        CharacterController playerController = target.GetComponent<CharacterController>();
        Vector3 predictedPosition = target.position + playerController.velocity * predictionFactor;
        if (IsInBounds(predictedPosition)){ agent.SetDestination(predictedPosition); }
        else {agent.SetDestination(target.position);}
    }
    public bool IsInBounds(Vector3 targ){
        if (targ.x <= roomMinBounds.x && targ.x >= roomMaxBounds.x && targ.z <= roomMinBounds.z && targ.z >= roomMaxBounds.z){
            return true;}
            else return false;
    }
    private Vector3 GetClosestPointInBounds(Vector3 position)
        {
        Vector3 closestPoint = position;
        closestPoint.x = Mathf.Clamp(closestPoint.x, roomMinBounds.x, roomMaxBounds.x);
        closestPoint.z = Mathf.Clamp(closestPoint.z, roomMinBounds.z, roomMaxBounds.z);
        return closestPoint;
        }

    private void Patrolling()
    {
        if (!walkPointSet) return;

        agent.SetDestination(walkPoint);
        Debug.DrawLine(agent.transform.position, walkPoint, Color.green);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    public void SearchWalkPoint()
    {
        Vector3 randomPoint = new Vector3(Random.Range(roomMinBounds.x, roomMaxBounds.x), transform.position.y, Random.Range(roomMinBounds.z, roomMaxBounds.z));
        walkPoint = randomPoint;
        Debug.DrawLine(transform.position, randomPoint, Color.green);
        walkPointSet = true;
    }

    private IEnumerator WaitForNewWalkPoint()
    {
        waitingforwalkpoint = true;
        float waitTime = Random.Range(3f, 15f);
        yield return new WaitForSeconds(waitTime);
        SearchWalkPoint();
        waitingforwalkpoint = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, SearchRadius);
    }

    public bool Walking
    {
        get { return _walking; }
        set
        {
            if (value == _walking) return;
            _walking = value;
            Anim.SetBool("Walking", _walking);
        }
    }

    public bool Running
    {
        get { return _running; }
        set
        {
            if (value == _running) return;
            _running = value;
            Anim.SetBool("Running", _running);
        }
    }
}