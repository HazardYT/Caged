using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

public class DonnyAI : MonoBehaviourPun
{
    public Dictionary<int, bool> doorStates = new Dictionary<int, bool>();
    [Header("Bool Info")]
    [SerializeField] private bool isChasing = false;
    [SerializeField] private bool isWalkPointSet;
    [SerializeField] private bool moveToLastKnown = false;
    [SerializeField] private bool foundDoor = false;
    public bool isListening = true;
    public bool _running;
    public bool _walking;
    [Header("Info")]
    [SerializeField] private Vector3 lastKnownPosition;
    [SerializeField] private Transform Target = null;
    [SerializeField] private Vector3 walkPoint;
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform agentEyes;
    [SerializeField] private DonnyDoorOpener doorOpener;
    [SerializeField] private ProceduralAnimation proceduralAnim;
    [Header("LayerMasks")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private LayerMask whatIsDoor;
    [Header("Settings")]
    public float agentWalkSpeed;
    public float agentRunSpeed;
    [SerializeField] private float maxPredictionFactor = 1.5f;
    [SerializeField] private float chanceOfSearch;
    [SerializeField] private float hearingDistance;
    [SerializeField] private float walkPointRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float sightRange;
    [SerializeField] private float maxRange;
    [SerializeField] private float hearingVolume;
    [SerializeField] private int maxWalkPointAttempts = 3;
    [SerializeField] private float GracePeriodTime = 30;
    private float stuckTimer = 0;
    private float stuckThreshold = 3f; // Adjust this value as needed
    public Vector3 jailMinBounds;
    public Vector3 jailMaxBounds;
    //other variables
    private bool isGrace;
    private float timeSinceLastHeard = 0f;
    private float hearingCooldown = 0.5f;
    [SerializeField] private Vector3 playerCagePos;
    [SerializeField] private Vector3 playerDropPos;


    private void Start()
    {
        DifficultySet();
        if (!photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            gameObject.GetComponent<AudioListener>().enabled = false;
            return;
        }
    }
    private void Update()
    {
        if (!isGrace)
        {
            Movement();
            Animation();
            CheckAndUpdateDoorStates();
            if (isListening && !moveToLastKnown)
            {
                timeSinceLastHeard += Time.deltaTime;
                if (timeSinceLastHeard >= hearingCooldown)
                {
                    Listening();
                    timeSinceLastHeard = 0f;
                }
            }
        }
        else { Movement(); }
        if (moveToLastKnown && !isChasing)
        {
            GoToLastKnownPosition();
        }

    }
    private void Movement()
    {
        if (agent.velocity == Vector3.zero)
        {
            if (!moveToLastKnown && !foundDoor)
            {
                SearchWalkPoint();
            }
        }
        if (!isChasing)
        {
            if (!moveToLastKnown)
            {
                agent.speed = agentWalkSpeed;
                CheckForPlayersInChaseRange();
                CheckForHidingSpots();
                Patrolling();
            }
            else
            {
                GoToLastKnownPosition();
                foundDoor = false;
            }
        }
        if (isChasing)
        {
            agent.speed = agentRunSpeed;
            foundDoor = false;
            isListening = false;
            ChasePlayer();
            CheckAttackRange();
            OpenCloseHidingSpots();
        }
        CheckForPlayers();
    }
    IEnumerator GracePeriod()
    {
        Debug.Log("grace");
        yield return new WaitForSeconds(0.5f);
        isGrace = true;
        yield return new WaitForSeconds(GracePeriodTime);
        isGrace = false;
    }
    private void CheckAndUpdateDoorStates()
    {
        Collider[] doorsInSightRange = Physics.OverlapSphere(transform.position, sightRange, whatIsDoor);

        foreach (Collider doorCollider in doorsInSightRange)
        {
            if (doorCollider.CompareTag("StaticDoor") || doorCollider.CompareTag("Door"))
            {
                GameObject doorObj = doorCollider.gameObject;
                PhotonView doorView = doorObj.GetComponent<PhotonView>();
                int doorViewID = doorView.ViewID;
                bool isStaticDoor = doorObj.CompareTag("StaticDoor");

                RaycastHit hit;
                if (Physics.Raycast(agentEyes.position, (doorObj.transform.position - agentEyes.position), out hit))
                {
                    if (hit.collider.CompareTag("Door") || hit.collider.CompareTag("StaticDoor"))
                    {
                        Debug.Log("PEE Door or Static DOor IN Line of sight");
                        bool currentIsOpen = isStaticDoor ? doorObj.GetComponent<StaticDoorInfo>().isOpen : doorObj.GetComponent<DoorInfo>().isOpen;
                        if (doorStates.ContainsKey(doorViewID))
                        {
                            if (doorStates[doorViewID] != currentIsOpen)
                            {
                                walkPoint = hit.point;
                                isWalkPointSet = true;
                                Debug.Log("PEE Updating and moving state =!!");
                                doorStates[doorViewID] = currentIsOpen;
                                if (doorOpener.StaticDoorCooldown)
                                {
                                    foundDoor = true;
                                    if (doorView.Owner != PhotonNetwork.LocalPlayer) { doorView.RequestOwnership(); }
                                    StartCoroutine(CheckDistanceFromDoor(hit.point, doorView, doorView.gameObject.GetComponent<StaticDoorInfo>()));
                                }
                            }
                        }
                        else { doorStates.Add(doorObj.GetComponent<PhotonView>().ViewID, currentIsOpen); return; }
                    }
                }
            }
        }
    }
    private void Animation()
    {
        if (agent.velocity == Vector3.zero)
        {
            isChasing = false;
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
    }
    public void OpenCloseHidingSpots()
    {
        Collider[] playersInAttackRange = Physics.OverlapSphere(transform.position, attackRange, whatIsDoor);
        foreach (Collider door in playersInAttackRange)
        {
            if (door.CompareTag("StaticDoor"))
            {
                RaycastHit hit;
                if (Physics.Raycast(agentEyes.position, (door.transform.position - agentEyes.position), out hit))
                {
                    if (hit.collider.CompareTag("StaticDoor"))
                    {
                        StaticDoorOpen(door, hit);
                    }
                }
            }
        }
    }
    private void CheckAttackRange()
    {
        Debug.Log("CheckForPlayersInAttackRange");
        Collider[] playersInAttackRange = Physics.OverlapSphere(transform.position, attackRange, whatIsPlayer);
        foreach (Collider player in playersInAttackRange)
        {
            if (player.CompareTag("Player"))
            {
                RaycastHit hit;
                if (Physics.Raycast(agentEyes.position, (player.transform.position - agentEyes.position), out hit))
                {
                    if (player.CompareTag("Player"))
                    {
                        Attack(player);
                        StartCoroutine(nameof(GracePeriod));
                    }
                }
            }
        }
    }
    public void Attack(Collider player)
    {
        PhotonView view = player.gameObject.GetComponent<PhotonView>();
        view.TransferOwnership(PhotonNetwork.LocalPlayer);
        view.transform.position = transform.GetChild(0).transform.GetChild(0).position;
        Vector3 Pos = new Vector3(transform.position.x, view.transform.position.y, transform.position.z);
        view.transform.LookAt(Pos, Vector3.up);
        photonView.RPC(nameof(DonnyRPC.DonnyCatching), RpcTarget.AllViaServer, view.ViewID, photonView.ViewID);
        isChasing = false;
        lastKnownPosition = playerCagePos;
        moveToLastKnown = true;
        Running = true;
        StartCoroutine(ReleaseAttack(view));
    }
    IEnumerator ReleaseAttack(PhotonView view)
    {
        while (Vector3.Distance(transform.position, lastKnownPosition) > 2f)
        {
            Debug.Log(Vector3.Distance(transform.position, lastKnownPosition));
            yield return null;
        }
        view.transform.position = playerDropPos;
        view.TransferOwnership(view.Owner);
        photonView.RPC(nameof(DonnyRPC.DonnyRelease), RpcTarget.AllViaServer, view.ViewID);
        agent.speed = agentWalkSpeed;
        isWalkPointSet = false;
        moveToLastKnown = false;
        Running = false;
        Walking = true;
    }
    private void CheckForPlayers()
    {
        Debug.Log("CheckForPlayers");
        Collider[] playersInSightRange = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);

        foreach (Collider obj in playersInSightRange)
        {
            if (obj.CompareTag("Player"))
            {
                RaycastHit hit;
                if (Physics.Raycast(agentEyes.position, (obj.transform.position - agentEyes.position), out hit))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Target = obj.transform;
                        Debug.DrawLine(agentEyes.position, Target.position, Color.green);
                        isChasing = true;
                        moveToLastKnown = false;
                        Debug.Log("Chasing from checkplayers");
                    }
                    else if (!hit.collider.CompareTag("Player"))
                    {
                        isChasing = false;
                    }
                    else if (!moveToLastKnown && !isWalkPointSet && !isChasing)
                    {
                        lastKnownPosition = Target.position;
                        moveToLastKnown = true;
                        Debug.Log("Chasing from checkplayers return to last known");
                        return;
                    }
                }

            }
        }
    }
    private void CheckForHidingSpots()
    {
        Debug.Log("CheckForHidingSpots");
        Collider[] playersInSightRange = Physics.OverlapSphere(transform.position, sightRange, whatIsDoor);
        foreach (Collider obj in playersInSightRange)
        {
            if (obj.CompareTag("StaticDoor"))
            {
                RaycastHit hit;
                if (Physics.Raycast(agentEyes.position, (obj.transform.position - agentEyes.position), out hit))
                {
                    if (hit.collider.CompareTag("StaticDoor"))
                    {
                        PhotonView doorview = obj.gameObject.GetComponent<PhotonView>();
                        StaticDoorInfo SDI = doorview.gameObject.GetComponent<StaticDoorInfo>();
                        if (SDI.isOpen == false)
                        {
                            Debug.DrawLine(agentEyes.position, hit.point, Color.grey);
                            if (Random.value < chanceOfSearch / 100 && !foundDoor)
                            {
                                Debug.Log("Value hit");
                                foundDoor = true;
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
                                        return;
                                    }
                                }
                            }
                            foreach (Collider player in playersInSightRange)
                            {
                                if (player.CompareTag("Player") && Vector3.Distance(player.transform.position, obj.transform.position) <= 1.5f)
                                {
                                    bool flashlightActive = CanSeePlayerLight(player.transform);
                                    if (flashlightActive)
                                    {
                                        foundDoor = true;
                                        Vector3 center = hit.collider.bounds.center;
                                        RaycastHit floorHit;
                                        if (Physics.Raycast(center, Vector3.down, out floorHit))
                                        {
                                            NavMeshHit navMeshHit;
                                            if (NavMesh.SamplePosition(floorHit.point, out navMeshHit, 1.5f, NavMesh.AllAreas))
                                            {
                                                if (doorview.Owner != PhotonNetwork.LocalPlayer) { doorview.RequestOwnership(); }
                                                Debug.Log("FOUND LIGHT IN CLOSET");
                                                lastKnownPosition = navMeshHit.position;
                                                moveToLastKnown = true;
                                                Debug.DrawLine(floorHit.point, navMeshHit.position, Color.cyan);
                                                StartCoroutine(CheckDistanceFromDoor(navMeshHit.position, doorview, SDI));
                                                return;

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    private void CheckForPlayersInChaseRange()
    {
        Debug.Log("ChecKForChaseRange");
        Collider[] playersInChaseRange = Physics.OverlapSphere(transform.position, maxRange, whatIsPlayer);
        foreach (Collider player in playersInChaseRange)
        {
            if (player.CompareTag("Player"))
            {
                RaycastHit hit;
                if (Physics.Raycast(agentEyes.position, (player.transform.position - agentEyes.position), out hit))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        bool flashlightActive = CanSeePlayerLight(player.transform);
                        if (flashlightActive)
                        {
                            Debug.Log("FoundInChaseRange");
                            Target = player.transform;
                            isChasing = true;
                            return;
                        }
                    }
                }
            }
        }
    }
    private void StaticDoorOpen(Collider player, RaycastHit hit)
    {
        Debug.Log("DOORATTACKOPEN");
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
        Debug.Log("OPENING DOOR");
    }

    IEnumerator CheckDistanceFromDoor(Vector3 pos, PhotonView doorview, StaticDoorInfo SDI)
    {
        Debug.Log("DistanceFromDoor");

        while (Vector3.Distance(transform.position, pos) >= 2f)
        {
            Debug.Log("Distance: " + Vector3.Distance(transform.position, pos));
            if (isChasing || moveToLastKnown == true)
            {
                yield break;
            }
            yield return null;
        }
        // Wait for 0.5 seconds before opening the door
        yield return new WaitForSeconds(0.5f);
        agent.velocity = Vector3.zero;
        agent.speed = agentWalkSpeed;
        Walking = false;
        Debug.DrawLine(agentEyes.position, walkPoint, Color.green);
        if (!doorOpener.StaticDoorCooldown)
        {
            StartCoroutine(doorOpener.StaticDoor(SDI, doorview.ViewID));
        }

        // Wait for 0.2 seconds after opening the door
        yield return new WaitForSeconds(1f);

        foundDoor = false;
        isWalkPointSet = false;
        Debug.Log("RESETTING");
    }
    public void Patrolling()
    {
        Debug.Log("Patrolling");
        agent.speed = agentWalkSpeed;
        if (moveToLastKnown) return;

        if (!isWalkPointSet && !foundDoor) SearchWalkPoint();

        if (isWalkPointSet)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(walkPoint);
                Debug.DrawLine(agentEyes.position, walkPoint, Color.yellow);
            }
            else
            {
                isWalkPointSet = false;
            }
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 2f)
        {
            isWalkPointSet = false;
        }
    }
    public void SearchWalkPoint()
    {
        if (moveToLastKnown) return;

        bool validWalkPoint = false;
        int attempts = 0;

        do
        {
            attempts++;
            Debug.Log("finding walkpoint Attempt: " + attempts);

            // Calculate random angle within 45-degree angle from the front direction of the agent's camera
            float randomAngle = Random.Range(-Mathf.PI / 4, Mathf.PI / 4);
            float x = walkPointRange * Mathf.Cos(randomAngle);
            float z = walkPointRange * Mathf.Sin(randomAngle);

            // Use the agent's camera forward direction instead of the agent's forward direction
            Vector3 rotatedDirection = Quaternion.Euler(0, doorOpener.DonnyCam.transform.eulerAngles.y, 0) * new Vector3(x, 0, z);
            Vector3 walkPointDirection = new Vector3(transform.position.x + rotatedDirection.x, transform.position.y, transform.position.z + rotatedDirection.z);
            NavMesh.SamplePosition(walkPointDirection, out NavMeshHit hit, walkPointRange, NavMesh.AllAreas);

            agent.SetDestination(hit.position);

            // Check if the path is complete without creating a new NavMeshPath
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
    private void GoToLastKnownPosition()
    {
        agent.speed = agentRunSpeed;
        Debug.DrawLine(agentEyes.position, lastKnownPosition, Color.blue);

        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Debug.Log("MovingTOLastKnown");
            agent.SetDestination(lastKnownPosition);
        }
        else
        {
            Debug.Log("PathNotComplete");
            moveToLastKnown = false;
            SearchWalkPoint();
        }

        float distanceToLastKnown = Vector3.Distance(transform.position, lastKnownPosition);

        if (distanceToLastKnown < 2f)
        {
            Debug.Log("Completed--MoveToLastKnown");
            agent.speed = agentWalkSpeed;
            moveToLastKnown = false;
            SearchWalkPoint();
        }
        else if (agent.velocity == Vector3.zero)
        {
            // Add a timer to check if the agent is stuck
            stuckTimer += Time.deltaTime;
            if (stuckTimer > stuckThreshold)
            {
                // If the agent is stuck, force it to recalculate its path
                moveToLastKnown = false;
                Debug.Log("CancellingMoveToLastVECTORZERO");
                SearchWalkPoint();
                stuckTimer = 0;
            }
        }
        else
        {
            // Reset the stuck timer if the agent is moving
            stuckTimer = 0;
        }
    }
    private void ChasePlayer()
    {
        Debug.Log("ChasePlayer");

        float distanceToTarget = Vector3.Distance(transform.position, Target.position);

        if (distanceToTarget <= maxRange)
        {
            isWalkPointSet = false;
            Debug.DrawLine(transform.position, Target.position, Color.red);
            // Get the player's CharacterController component
            CharacterController playerController = Target.GetComponent<CharacterController>();

            // Calculate the prediction factor based on the distance to the target
            float predictionFactor = Mathf.Lerp(0.05f, maxPredictionFactor, distanceToTarget / maxRange);

            // Calculate the predicted position based on the player's current position, velocity, and the prediction factor
            Vector3 predictedPosition = Target.position + (playerController.velocity * predictionFactor);

            // Set the agent's destination to the predicted position
            agent.SetDestination(predictedPosition);
        }
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
                                lastKnownPosition = hit.position;
                                moveToLastKnown = true;
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
    private void DifficultySet()
    {
        switch (LobbyManager.Difficulty)
        {
            case 0:
                agentWalkSpeed = 2f;
                agentRunSpeed = 4f;
                sightRange = 8;
                maxRange = 18;
                hearingDistance = 25;
                GracePeriodTime = 60;
                break;
            case 1:
                agentWalkSpeed = 3f;
                agentRunSpeed = 5f;
                sightRange = 10;
                maxRange = 22;
                hearingDistance = 30;
                GracePeriodTime = 45;
                break;
            case 2:
                agentWalkSpeed = 4;
                agentRunSpeed = 8;
                sightRange = 12;
                maxRange = 25;
                hearingDistance = 35;
                GracePeriodTime = 30;
                break;
            case 3:
                agentWalkSpeed = 4;
                agentRunSpeed = 10;
                sightRange = 15;
                maxRange = 30;
                hearingDistance = 50;
                GracePeriodTime = 15;
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
