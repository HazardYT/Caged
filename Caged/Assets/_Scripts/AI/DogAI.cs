using System.Collections;
using Photon.Pun;
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
    public int _DistractionDelayTime = 30;
    public int _chaseRange;
    public bool isOnTimer;
    public bool Distracted = false;
    public bool chasing = false;
    private bool waitingforwalkpoint = false;
    public float ChaseSpeed;
    public float PatrolSpeed;

    public Vector3 roomMinBounds;
    public Vector3 roomMaxBounds;

    public bool _walking;
    public bool _running;

    private void Update()
    {
        if (agent.velocity == Vector3.zero && !Distracted)
        {
            if (!walkPointSet && !waitingforwalkpoint){
                StartCoroutine(WaitForNewWalkPoint());
            }
            else{
                Patrolling();
            }
        }
        Collider[] playersInChaseRange = Physics.OverlapSphere(transform.position, SearchRadius, mask);
        if (!chasing && !Distracted)
        {
            Patrolling();
            foreach (Collider obj in playersInChaseRange)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < _chaseRange) {
                    if (obj.CompareTag("Player")){
                        target = obj.transform;
                        if (IsInBounds(target.position)){
                            chasing = true;
                            return;
                        }
                    }
                    else if (obj.CompareTag("Item") && !isOnTimer){
                        target = obj.transform;
                        if (IsInBounds(target.position)){
                            Distracted = true;
                        }
                        else{
                            StopCoroutine(GetMeat());
                            Distracted = false;
                            target = null;
                        }
                    }
                }
                else{
                    if (obj.CompareTag("Player")){
                        if (NavMesh.SamplePosition(target.position, out NavMeshHit navHit, 8f, NavMesh.AllAreas))
                        {
                            if (IsInBounds(navHit.position)){
                                agent.SetDestination(navHit.position);
                            }
                            else return;
                        }
                    }
                }
            }
        }
        if (chasing && !Distracted)
        {
            foreach (Collider obj in playersInChaseRange)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < _chaseRange) {
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
        if (Distracted){
            foreach (Collider player in playersInChaseRange){
                if (Vector3.Distance(transform.position, player.transform.position) < _chaseRange) {
                    if (player.CompareTag("Item")){
                        Distracted = true;
                        agent.SetDestination(target.position);
                        StartCoroutine(GetMeat());
                        
                    }
                }
            }
        }
    }
    private IEnumerator GetMeat(){
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target.position) <= 2f);
        agent.SetDestination(transform.position);
        target.gameObject.tag = "Untagged";
        PhotonView view = target.transform.GetComponent<PhotonView>();
        view.RequestOwnership();
        yield return new WaitForSeconds(5f);
        Distracted = false;
        isOnTimer = true;
        StartCoroutine(Timer());
        PhotonNetwork.Destroy(view);
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