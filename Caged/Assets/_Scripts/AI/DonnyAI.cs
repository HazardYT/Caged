using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class DonnyAI : MonoBehaviourPun
{
    public Dictionary<int, bool> DoorStates = new Dictionary<int, bool>();

    [Header("LayerMasks")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private LayerMask whatIsDoors;
    [SerializeField] private LayerMask allMask;
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;

    [SerializeField] private Animator anim;
    [SerializeField] private Transform agentEyes;
    [SerializeField] private DonnyDoorOpener doorOpener;
    [SerializeField] private ProceduralAnimation proceduralAnim;
    [Header("Settings")]
    [SerializeField] private float attackRange;
    [SerializeField] private float sightRange;
    [SerializeField] private float maxRange;
    [SerializeField] private float gracePeriodLength;
    [SerializeField] private float predictionAmount;
    [SerializeField] private float chanceOfSearch;
    private float timeSinceLastHeard = 0f;
    private float hearingCooldown = 0.2f;
    public bool isListening = true;
    public bool isRaged;
    public float rageChance;
    public bool _running;
    public bool _walking;
    public float agentWalkSpeed;
    public float agentRunSpeed;

    [Header("Info")]
    private float stuckTimer = 0f;
    private Vector3 previousPosition;

    private Vector3 animationPos;
    public bool isGraceMode = false;
    [SerializeField] private Transform Target = null;
    [SerializeField] private bool hasFoundDoor;
    [SerializeField] private bool isChasing;
    [SerializeField] private bool isMovingToPos;
    [SerializeField] private Vector3 MovePos;
    [SerializeField] private Vector3 walkPoint;
    [SerializeField] private float walkPointRange;
    [SerializeField] private bool isWalkPointSet;
    [SerializeField] private int maxWalkPointAttempts = 3;
    [SerializeField] private float hearingDistance;
    [SerializeField] private Vector3 jailMinBounds;
    [SerializeField] private Vector3 jailMaxBounds;
    [SerializeField] private Vector3 playerCagePos;
    [SerializeField] private Vector3 playerDropPos;

    private void Start()
    {
        SetDifficulty();
        if (!photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            gameObject.GetComponent<AudioListener>().enabled = false;
            return;
        }
    }
    private void Update()
    {
        if (isGraceMode)
        {
            isChasing = false;
            isMovingToPos = false;
            isWalkPointSet = false;
            if (isMovingToPos) { GoToPosition(); }
            else { Patrolling(); }
            return;
        }
        AttackRange();
        SightRange();
        DoorMemory();
        MaxRange();
        Animation();
        RageMode();
        if (isListening && !isMovingToPos && !isChasing) { Listening(); }
        if (isChasing && !isMovingToPos) { Chase(); agent.speed = agentRunSpeed; }
        if (!isChasing && isMovingToPos) { GoToPosition(); agent.speed = agentRunSpeed; }
        if (!isChasing && !isMovingToPos) { Patrolling(); SightRangeDoors(); agent.speed = agentWalkSpeed; }
    }
    public void RageMode()
    {
        if (Random.value < rageChance){
            isRaged = true;
            float i = Random.Range(10f, 30f);
            Invoke(nameof(RageTimerReset), i);
        }
    }
    public void RageTimerReset(){
        isRaged = false;
    }
    public void AttackRange()
    {
        Collider[] InAttackRange = Physics.OverlapSphere(transform.position, attackRange, allMask);
        foreach (Collider obj in InAttackRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(agentEyes.position, (obj.transform.position - agentEyes.position), out hit))
            {
                if (isChasing && hit.collider.CompareTag("StaticDoor"))
                {
                    StaticDoorOpen(obj, hit);
                }
                if (obj.CompareTag("Player"))
                {
                    StartCoroutine(Attack(obj));
                }
            }
        }
    }
    public void SightRange()
    {
        Debug.Log("SightRange");
        Collider[] InSightRange = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);
        bool playerInSight = false;
        foreach (Collider obj in InSightRange)
        {
            if (obj.CompareTag("Player"))
            {
                RaycastHit hit;
                if (Physics.Raycast(agentEyes.position, obj.transform.position - agentEyes.position, out hit))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.DrawRay(agentEyes.position, obj.transform.position - agentEyes.position, Color.red);
                        Debug.Log("chasing");
                        Target = obj.transform;
                        isMovingToPos = false;
                        isChasing = true;
                        playerInSight = true;
                    }
                    if (!hit.collider.CompareTag("Player") && !playerInSight && isChasing)
                    {
                        Debug.Log("GoToPos after chase");
                        isChasing = false;
                        MovePos = Target.position;
                        isMovingToPos = true;
                        Target = null;
                    }
                }
            }
        }
    }
    public void SightRangeDoors()
    {
        Debug.Log("SightRangeDoors");
        Collider[] InSightRange = Physics.OverlapSphere(transform.position, sightRange, allMask);

        foreach (Collider obj in InSightRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(agentEyes.position, obj.transform.position - agentEyes.position, out hit))
            {
                if (hit.collider.CompareTag("StaticDoor"))
                {
                    Debug.Log("randomopen check");
                    PhotonView doorview = obj.gameObject.GetComponent<PhotonView>();
                    StaticDoorInfo SDI = doorview.gameObject.GetComponent<StaticDoorInfo>();

                    if (SDI.isOpen == false)
                    {
                        Debug.DrawRay(agentEyes.position, obj.transform.position, Color.grey);

                        if (Random.value < chanceOfSearch / 100 && !hasFoundDoor)
                        {
                            hasFoundDoor = true;
                            OpenDoor(hit,doorview, SDI);
                        }
                        else
                        {
                            Collider[] nearbyObjects = Physics.OverlapSphere(obj.transform.position, 1f);
                            foreach (Collider nearbyObj in nearbyObjects)
                            {
                                if (nearbyObj.CompareTag("Player"))
                                {
                                    bool flashlightActive = CanSeePlayerLight(nearbyObj.transform);
                                    if (flashlightActive)
                                    {
                                        hasFoundDoor = true;
                                        OpenDoor(hit, doorview, SDI);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void OpenDoor(RaycastHit hit, PhotonView doorview, StaticDoorInfo SDI)
    {
        Vector3 center = hit.collider.bounds.center;
        RaycastHit floorHit;

        if (Physics.Raycast(center, Vector3.down, out floorHit))
        {
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(floorHit.point, out navMeshHit, 1.0f, NavMesh.AllAreas))
            {
                if (doorview.Owner != PhotonNetwork.LocalPlayer)
                {
                    doorview.RequestOwnership();
                }
                walkPoint = navMeshHit.position;
                isWalkPointSet = true;
                Debug.DrawLine(floorHit.point, navMeshHit.position, Color.cyan);
                StartCoroutine(CheckDistanceFromDoor(navMeshHit.position, doorview, SDI));
            }
        }
    }
    public void DoorMemory()
    {
        Collider[] InSightRange = Physics.OverlapSphere(transform.position, sightRange, whatIsDoors);
        foreach (Collider obj in InSightRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(agentEyes.position, obj.transform.position, out hit))
            {
                // Door Memory
                if (hit.collider.CompareTag("StaticDoor") || hit.collider.CompareTag("Door"))
                {
                    Debug.Log("door memory");
                    GameObject doorObj = obj.transform.root.gameObject;
                    PhotonView doorView = doorObj.GetComponent<PhotonView>();
                    bool currentIsOpen = hit.collider.CompareTag("StaticDoor") ? doorObj.GetComponent<StaticDoorInfo>().isOpen : doorObj.GetComponent<DoorInfo>().isOpen;
                    if (DoorStates.ContainsKey(doorView.ViewID))
                    {
                        if (DoorStates[doorView.ViewID] != currentIsOpen)
                        {
                            walkPoint = hit.point;
                            isWalkPointSet = true;
                            DoorStates[doorView.ViewID] = currentIsOpen;
                            if (doorOpener.StaticDoorCooldown)
                            {
                                hasFoundDoor = true;
                                if (doorView.Owner != PhotonNetwork.LocalPlayer) { doorView.RequestOwnership(); }
                                StartCoroutine(CheckDistanceFromDoor(hit.point, doorView, doorView.gameObject.GetComponent<StaticDoorInfo>()));
                            }
                        }
                    }
                    else { DoorStates.Add(doorObj.GetComponent<PhotonView>().ViewID, currentIsOpen); return; }
                }
            }
        }
    }
    
    public void MaxRange()
    {
        Collider[] playersInChaseRange = Physics.OverlapSphere(transform.position, maxRange, whatIsPlayer);
        foreach (Collider player in playersInChaseRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(agentEyes.position, player.transform.position - agentEyes.position, out hit))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    Debug.DrawRay(transform.position, player.transform.position, Color.white);
                    bool flashlightActive = CanSeePlayerLight(player.transform);
                    if (flashlightActive)
                    {
                        isMovingToPos = false;
                        Target = player.transform;
                        isChasing = true;
                        return;
                    }
                }
                if (!hit.collider.CompareTag("Player") && !isMovingToPos && !CanSeePlayerLight(player.transform) && isChasing)
                {
                    Debug.Log("GoToPos after chase");
                    isChasing = false;
                    MovePos = Target.position;
                    isMovingToPos = true;
                    Target = null;
                }
            }
        }
    }
    public void Chase()
    {
        Debug.Log("Chasing");
        float distanceToTarget = Vector3.Distance(transform.position, Target.position);

        if (distanceToTarget <= maxRange)
        {
            Debug.DrawLine(transform.position, Target.position, Color.red);
            CharacterController playerController = Target.GetComponent<CharacterController>();
            float predictionFactor = Mathf.Lerp(0.05f, predictionAmount, distanceToTarget / maxRange);
            Vector3 predictedPosition = Target.position + (playerController.velocity * predictionFactor);
            agent.SetDestination(predictedPosition);
        }
    }
    public void Patrolling()
    {
        if (!isWalkPointSet) SearchWalkPoint();

        Debug.Log("Patrolling");
        if (isWalkPointSet)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(walkPoint);
                Debug.DrawLine(agentEyes.position, walkPoint, Color.yellow);
            }
            else { isWalkPointSet = false; }
        }
        if (Vector3.Distance(transform.position, walkPoint) < 2f) { isWalkPointSet = false; }
    }
    private void GoToPosition()
    {
        Debug.DrawLine(agentEyes.position, MovePos, Color.blue);

        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(MovePos);
        }
        else
        {
            Debug.Log("PathNotComplete");
            isMovingToPos = false;
            SearchWalkPoint();
        }
        float distanceToLastKnown = Vector3.Distance(transform.position, MovePos);
        if (distanceToLastKnown < 2f)
        {
            Debug.Log("Completed--isMovingToPos");
            agent.speed = agentWalkSpeed;
            isMovingToPos = false;
            SearchWalkPointFocused();
        }
    }
    public void SearchWalkPoint()
    {
        if (isMovingToPos) return;

        bool validWalkPoint = false;
        int attempts = 0;

        do
        {
            attempts++;

            float randomAngle = Random.Range(-Mathf.PI / 4, Mathf.PI / 4);
            float x = walkPointRange * Mathf.Cos(randomAngle);
            float z = walkPointRange * Mathf.Sin(randomAngle);

            Vector3 rotatedDirection = Quaternion.Euler(0, doorOpener.DonnyCam.transform.eulerAngles.y, 0) * new Vector3(x, 0, z);
            Vector3 walkPointDirection = new Vector3(transform.position.x + rotatedDirection.x, transform.position.y, transform.position.z + rotatedDirection.z);
            NavMesh.SamplePosition(walkPointDirection, out NavMeshHit hit, walkPointRange, NavMesh.AllAreas);

            agent.SetDestination(hit.position);

            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                validWalkPoint = true;
            }
            if (validWalkPoint)
            {
                walkPoint = hit.position;
                isWalkPointSet = true;
                Debug.Log("Found walkpoint");
            }
            else
            {
                Debug.Log("finding Random Position");
                Vector3 randomDirection = Random.insideUnitSphere * walkPointRange;
                randomDirection += transform.position;
                NavMesh.SamplePosition(randomDirection, out hit, walkPointRange, NavMesh.AllAreas);

                agent.SetDestination(hit.position);

                if (agent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    walkPoint = hit.position;
                    isWalkPointSet = true;
                }
            }
        } while (!validWalkPoint && attempts < maxWalkPointAttempts);
    }
    public void SearchWalkPointFocused()
    {
        if (isMovingToPos) return;
        bool validWalkPoint = false;
        int attempts = 0;
        do
        {
            attempts++;
            float randomAngle = Random.Range(-25f, 25f);
            float x = walkPointRange * Mathf.Cos(randomAngle * Mathf.Deg2Rad);
            float z = walkPointRange * Mathf.Sin(randomAngle * Mathf.Deg2Rad);
            Vector3 rotatedDirection = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(x, 0, z);
            Vector3 walkPointDirection = new Vector3(transform.position.x + rotatedDirection.x, transform.position.y, transform.position.z + rotatedDirection.z);
            NavMesh.SamplePosition(walkPointDirection, out NavMeshHit hit, walkPointRange, NavMesh.AllAreas);
            agent.SetDestination(hit.position);
            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                validWalkPoint = true;
            }
            if (validWalkPoint)
            {
                walkPoint = hit.position;
                isWalkPointSet = true;
                Debug.Log("Found walkpoint FOCUSED");
            }
            else
            {
                Debug.Log("finding Random Position on FOCUSED");
                Vector3 randomDirection = Random.insideUnitSphere * walkPointRange;
                randomDirection += transform.position;
                NavMesh.SamplePosition(randomDirection, out hit, walkPointRange, NavMesh.AllAreas);
                agent.SetDestination(hit.position);
                if (agent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    walkPoint = hit.position;
                    isWalkPointSet = true;
                }
            }
        } while (!validWalkPoint && attempts < maxWalkPointAttempts);
    }
    private void Listening()
    {
        Debug.Log("Listening");
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source.isPlaying)
            {
                if (source.transform.position.x <= jailMinBounds.x && source.transform.position.x >= jailMaxBounds.x && source.transform.position.z <= jailMinBounds.z && source.transform.position.z >= jailMaxBounds.z)
                    continue;
                if (source.gameObject.CompareTag("Listenable") || source.gameObject.CompareTag("Door") || source.gameObject.CompareTag("StaticDoor") || source.gameObject.CompareTag("Item"))
                {
                    float distance = Vector3.Distance(source.transform.position, agentEyes.position);
                    if (distance <= hearingDistance)
                    {
                        float hearingVolume = Mathf.Lerp(0.2f, 0.8f, (distance - 2) / (hearingDistance - 2));
                        if (source.volume > hearingVolume)
                        {
                            Target = source.transform.root.transform;
                            Vector3 randomPoint = source.transform.position + Random.insideUnitSphere * 5f;
                            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                            {
                                Debug.Log("FoundSound");
                                MovePos = hit.position;
                                isMovingToPos = true;
                            }
                        }
                    }
                }
            }
        }
    }
    private bool CanSeePlayerLight(Transform playerTransform)
    {
        Debug.Log("CanSeePlayerLight");
        Light[] lights = playerTransform.GetComponentsInChildren<Light>();
        foreach (Light light in lights)
        {
            if (light.enabled && light.type == LightType.Spot)
            {
                Vector3 lightToAgentDirection = (transform.position - light.transform.position).normalized;
                float angleBetween = Vector3.Dot(light.transform.forward, lightToAgentDirection);

                if (angleBetween > Mathf.Cos(light.spotAngle * 0.5f * Mathf.Deg2Rad))
                {
                    Vector3 playerToAgentDirection = (transform.position - playerTransform.position).normalized;
                    float playerToAgentAngle = Vector3.Angle(playerTransform.forward, playerToAgentDirection);
                    float maxAllowedAngle = 65f;

                    if (playerToAgentAngle <= maxAllowedAngle)
                    {
                        Debug.Log("FOOUNDPLAYERLIGHT");
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private void StaticDoorOpen(Collider player, RaycastHit hit)
    {
        PhotonView doorview = player.gameObject.GetComponent<PhotonView>();
        StaticDoorInfo SDI = doorview.gameObject.GetComponent<StaticDoorInfo>();
        if (doorOpener.StaticDoorCooldown)
        {
            if (doorview.Owner != PhotonNetwork.LocalPlayer)
            {
                doorview.RequestOwnership();
            }
            StartCoroutine(doorOpener.StaticDoor(SDI, doorview.ViewID));
        }
    }

    IEnumerator CheckDistanceFromDoor(Vector3 pos, PhotonView doorview, StaticDoorInfo SDI)
    {
        while (Vector3.Distance(transform.position, pos) >= 2f)
        {
            if (isChasing || isMovingToPos == true)
            {
                yield break;
            }
            yield return null;
        }
        agent.velocity = Vector3.zero;
        Debug.DrawLine(agentEyes.position, walkPoint, Color.cyan);
        if (!doorOpener.StaticDoorCooldown) { StartCoroutine(doorOpener.StaticDoor(SDI, doorview.ViewID)); }
        yield return new WaitForSeconds(1f);
        isMovingToPos = false;
        hasFoundDoor = false;
        isWalkPointSet = false;
    }
    public IEnumerator Attack(Collider player)
    {
        isChasing = false;
        PhotonView view = player.gameObject.GetComponent<PhotonView>();
        AttackInitial(player);
        while (Vector3.Distance(transform.position, MovePos) > 2f)
        {
            yield return null;
        }
        AttackRelease(view);
    }
    public void AttackInitial(Collider player)
    {
        PhotonView view = player.gameObject.GetComponent<PhotonView>();
        view.TransferOwnership(PhotonNetwork.LocalPlayer);
        photonView.RPC(nameof(DonnyRPC.DonnyCatching), RpcTarget.AllViaServer, view.ViewID, photonView.ViewID);
        view.transform.position = transform.GetChild(0).transform.GetChild(0).position;
        Vector3 Pos = new Vector3(transform.position.x, view.transform.position.y, transform.position.z);
        view.transform.LookAt(Pos, Vector3.up);
        MovePos = playerCagePos;
        isMovingToPos = true;
        StartCoroutine(nameof(GracePeriod));
    }
    public void AttackRelease(PhotonView view)
    {
        view.transform.position = playerDropPos;
        view.TransferOwnership(view.Owner);
        photonView.RPC(nameof(DonnyRPC.DonnyRelease), RpcTarget.AllViaServer, view.ViewID);
        agent.speed = agentWalkSpeed;
        isWalkPointSet = false;
        isMovingToPos = false;
        Running = false;
    }
    private IEnumerator GracePeriod()
    {
        isGraceMode = true;
        yield return new WaitForSeconds(gracePeriodLength);
        isGraceMode = false;
    }
    private void Animation()
    {
        if (Vector3.Distance(animationPos, agent.transform.position) <= 0.5f)
        {
            Walking = false;
            Running = false;
        }
        if (agent.speed == agentRunSpeed)
        {
            Running = true;
            Walking = false;
            proceduralAnim.smoothness = 3;
            proceduralAnim.stepHeight = 0.5f;
            proceduralAnim.stepLength = 2.35f;
            proceduralAnim.angularSpeed = 10;
            proceduralAnim.bounceAmplitude = 0.2f;
        }
        if (agent.speed == agentWalkSpeed)
        {
            Walking = true;
            Running = false;
            proceduralAnim.smoothness = 3;
            proceduralAnim.stepHeight = 0.5f;
            proceduralAnim.stepLength = 2.25f;
            proceduralAnim.angularSpeed = 7;
            proceduralAnim.bounceAmplitude = 0.2f;
        }

        animationPos = agent.transform.position;
    }
    private void SetDifficulty()
    {
        switch (LobbyManager.Difficulty)
        {
            case 0:
                agentWalkSpeed = 2f;
                agentRunSpeed = 4f;
                sightRange = 8;
                maxRange = 18;
                hearingDistance = 25;
                gracePeriodLength = 60;
                break;
            case 1:
                agentWalkSpeed = 3f;
                agentRunSpeed = 5f;
                sightRange = 10;
                maxRange = 22;
                hearingDistance = 30;
                gracePeriodLength = 45;
                break;
            case 2:
                agentWalkSpeed = 4;
                agentRunSpeed = 8;
                sightRange = 12;
                maxRange = 25;
                hearingDistance = 35;
                gracePeriodLength = 30;
                break;
            case 3:
                agentWalkSpeed = 4;
                agentRunSpeed = 10;
                sightRange = 15;
                maxRange = 30;
                hearingDistance = 50;
                gracePeriodLength = 15;
                break;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, maxRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    public bool Walking
    {
        get { return _walking; }
        set
        {
            if (value == _walking) return;
            _walking = value;
            anim.SetBool("Walking", _walking);
        }
    }
    public bool Running
    {
        get { return _running; }
        set
        {
            if (value == _running) return;
            _running = value;
            anim.SetBool("Running", _running);
        }
    }
}
 