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
    [SerializeField] private ItemSpawning itemSpawning;
    [SerializeField] private ProceduralAnimation proceduralAnim;
    [Header("Settings")]
    [SerializeField] private float attackRange;
    [SerializeField] private float sightRange;
    [SerializeField] private float maxRange;
    [SerializeField] private float gracePeriodLength;
    [SerializeField] private float predictionAmount;
    [SerializeField] private float chanceOfSearch;
    [SerializeField] private float HearingDistanceMinVolume;
    [SerializeField] private float HearingDistanceMaxVolume;
    [SerializeField] private int maxWalkPointAttempts = 3;
    [SerializeField] private float hearingDistance;
    [SerializeField] Collider[] overlapSphereResults;
    public bool isListening = true;
    public bool _running;
    public bool _walking;
    public float agentWalkSpeed;
    public float agentRunSpeed;

    [Header("Info")]
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
    [SerializeField] private Vector3 jailMinBounds;
    [SerializeField] private Vector3 jailMaxBounds;
    [SerializeField] private Vector3 playerCagePos;
    [SerializeField] private Vector3 playerDropPos;
    private bool isAttacking;
    private float pathIncompleteTimer = 0f;
    private float pathIncompleteWaitTime = 1f;
    public int maxColliders;

    // Start Function Disabling the script for any player but the master client. so it runs one instance. and also disabling audiolistener for every client, and callling set difficulty.
    private void Start()
    {
        if (!photonView.IsMine)
        {
            enabled = false;
            return;
        }
        photonView.RPC(nameof(SetDifficulty),RpcTarget.AllBufferedViaServer);   
    }
    
    // Update Function Doing the logic for All functions based on chasing ismovingtopos and islistening parameters. and also calls the neccessary functions.
    private void Update()
    {
        if (!isGraceMode && !isAttacking)
        {
            MaxRange();
            SightRange();
            AttackRange();
            DoorMemory();
            Animation();
            if (isChasing && !isMovingToPos) { Chase(); agent.speed = agentRunSpeed; }
            if (!isChasing && isMovingToPos) { GoToPosition(); agent.speed = agentRunSpeed; }
            if (!isChasing && !isMovingToPos) { Patrolling(); SightRangeDoors(); agent.speed = agentWalkSpeed; }
            if (isListening && !isMovingToPos && !isChasing) { Listening(); }
        }
        else
        {
            isChasing = false;
            if (isMovingToPos) { GoToPosition(); }
            else { Patrolling(); }
            return;
        }
    }
    // Attack Range Function that checks if it can see the player and if it sees a door while chasing it will open it. else it attacks if it can see the player.
    public void AttackRange()
    {
        foreach (Collider obj in overlapSphereResults)
        {
            if (obj == null) {continue;}
            if (Vector3.Distance(transform.position, obj.transform.position) <= attackRange){
                if (Physics.Raycast(agentEyes.position, obj.transform.position - agentEyes.position, out RaycastHit hit, attackRange))
                {
                    Debug.DrawLine(agentEyes.position, obj.transform.position, Color.cyan);
                    if (isChasing && hit.collider.CompareTag("StaticDoor") && !isAttacking)
                    {
                        print("ATTACK Static Door Called");
                        StaticDoorOpen(obj, hit);
                    }
                    if (hit.collider.CompareTag("Player") && !isAttacking)
                    {
                        print("ATTACK player Called");
                        StartCoroutine(Attack(hit.collider));
                    }
                }
            }
        }
    }
    // Sight Range Function That Checks for players and there lights and updates the isChasing var accordingly
    public void SightRange()
    {
        print("(SightRange) - Function Called");
        foreach (Collider obj in overlapSphereResults)
        {
            if (obj == null) {continue;}
            if (Vector3.Distance(transform.position, obj.transform.position) <= sightRange){
                if (obj.CompareTag("Player"))
                {
                    RaycastHit hit;
                    if (Physics.Raycast(agentEyes.position, obj.transform.position - agentEyes.position, out hit))
                    {
                        // Check if can see line of sight to player and is in range if so isChase is true
                        if (hit.collider.CompareTag("Player"))
                        {
                            print("(SightRange) - Chasing");
                            Target = obj.transform;
                            isMovingToPos = false;
                            isChasing = true;
                        }
                        // Check if cant see player but is also chasing and theres no light seen
                        if (!hit.collider.CompareTag("Player") && isChasing && !CanSeePlayerLight(obj.transform))
                        {
                            print("(SightRange) - Going To Position");
                            isChasing = false;
                            MovePos = Target.position;
                            isMovingToPos = true;
                            Target = null;
                        }
                    }
                }
            }
        }
    }
    // Checks Sight Range for Static Doors AND if it hits the percentage it will go to the door and open it
    // also checks if theres a players light behind a door and if there is it also opens door 
    public void SightRangeDoors()
    {
        print("(SightRangeDoors) - Function Called");
        foreach (Collider obj in overlapSphereResults)
        {
            if (obj == null) {continue;}
            if (obj.CompareTag("StaticDoor") || obj.CompareTag("Player")){
                if (Vector3.Distance(transform.position, obj.transform.position) <= sightRange){
                    if (Physics.Raycast(agentEyes.position, obj.transform.position, out RaycastHit hit))
                    {
                        if (hit.collider.CompareTag("StaticDoor"))
                        {
                            print("(SightRangeDoors) - Random Open Check");
                            PhotonView doorview = obj.gameObject.GetComponent<PhotonView>();
                            StaticDoorInfo SDI = doorview.gameObject.GetComponent<StaticDoorInfo>();

                            if (SDI != null && SDI.isOpen == false)
                            {
                                Debug.DrawRay(agentEyes.position, obj.transform.position, Color.grey);

                                if (Random.value < chanceOfSearch / 100 && !hasFoundDoor)
                                {
                                    hasFoundDoor = true;
                                    OpenDoor(hit,doorview, SDI);
                                }
                                else
                                {
                                    foreach (Collider nearbyObj in overlapSphereResults)
                                    {
                                        if (nearbyObj == null) { continue; }
                                        if (nearbyObj.CompareTag("Player"))
                                        {
                                            bool flashlightActive = IsPlayerLightOn(nearbyObj.transform);
                                            if (flashlightActive && Vector3.Distance(nearbyObj.transform.position, hit.transform.position) < 2)
                                            {
                                                hasFoundDoor = true;
                                                OpenDoor(hit, doorview, SDI);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else print("(SightRangeDoors) - Error on Static Door Info");
                        }
                    }
                }
            }
        }
    }
    // Called From other functions with info to find nearest navmesh point for agent to call CheckDistanceFromDoor which does the moving.
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
    // Updates Door memory to see what door has been opened the last time the agent seen it
    // also compares it and if its not the same will go to it, and if the door is not in the dictionary it will add it.
    public void DoorMemory()
    {
        foreach (Collider obj in overlapSphereResults)
        {
            if (obj == null) {continue;}
            if (Vector3.Distance(transform.position, obj.transform.position) <= sightRange){
                if (Physics.Raycast(agentEyes.position, obj.transform.position, out RaycastHit hit))
                {
                    // Door Memory
                    if (hit.collider.CompareTag("StaticDoor") || hit.collider.CompareTag("Door"))
                    {
                        print("(DoorMemory) - Door Memory Called");
                        PhotonView doorView = hit.collider.GetComponent<PhotonView>();
                        bool currentIsOpen = hit.collider.CompareTag("StaticDoor") ? hit.collider.GetComponent<StaticDoorInfo>().isOpen : hit.collider.GetComponent<DoorInfo>().isOpen;
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
                        // add door to dictionary
                        else { DoorStates.Add(hit.collider.GetComponent<PhotonView>().ViewID, currentIsOpen); return; }
                    }
                }
            }
        }
    }
    // Checks the Max Range around the agent to check for a player if, and if the agent can see the light it will chase.
    // Also has a check for if the player is not in the range and is chasing it will stop chase. to cleanup previous chasing
    public void MaxRange()
    {
        overlapSphereResults = new Collider[maxColliders];
        Physics.OverlapSphereNonAlloc(transform.position, maxRange, overlapSphereResults, allMask);
        foreach (Collider player in overlapSphereResults)
        {
            if (player == null) {return;}
            if (player.gameObject.CompareTag("Player")){
                if (Physics.Raycast(agentEyes.position, player.transform.position - agentEyes.position, out RaycastHit hit))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        bool flashlightActive = CanSeePlayerLight(player.transform);
                        if (flashlightActive)
                        {
                            Debug.DrawLine(transform.position, player.transform.position, Color.white);
                            isMovingToPos = false;
                            Target = player.transform;
                            isChasing = true;
                            return;
                        }
                    }
                    if (!hit.collider.CompareTag("Player") && !isMovingToPos && isChasing && !CanSeePlayerLight(player.transform))
                    {
                        print("(MaxRange) - Lost Player Line Of Sight Moving To Last Pos.");
                        isChasing = false;
                        MovePos = Target.position;
                        isMovingToPos = true;
                        Target = null;
                    }
                }
            }
        }
    }
    // Called From other functions which predicts the movement and actually chases the player while being run. as long as isChasing is true.
    public void Chase()
    {
        print("(Chase) - Function called");
        float distanceToTarget = Vector3.Distance(transform.position, Target.position);

        if (distanceToTarget <= maxRange)
        {
            Debug.DrawLine(agentEyes.position, Target.position, Color.red);
            CharacterController playerController = Target.GetComponent<CharacterController>();
            float predictionFactor = Mathf.Lerp(0.05f, predictionAmount, distanceToTarget / maxRange);
            Vector3 predictedPosition = Target.position + (playerController.velocity * predictionFactor);
            agent.SetDestination(predictedPosition);
        }
    }
    // Called From other functions which checks searchwalkpoint and gets a position then will patroll finding random points.
    public void Patrolling()
    {
        if (!isWalkPointSet) SearchWalkPoint();

        print("(Patrolling) - Function called");
        if (isWalkPointSet)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(walkPoint);
                Debug.DrawLine(agentEyes.position, walkPoint, Color.yellow);
            }
            else { isWalkPointSet = false; }
        }
        if ((transform.position - walkPoint).sqrMagnitude < 2f * 2f) { isWalkPointSet = false; }
    }
    // Called From Other functions which will go to the location that is set on MovePos.
    private void GoToPosition()
    {
        Debug.DrawLine(agentEyes.position, MovePos, Color.blue);

        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(MovePos);
            pathIncompleteTimer = 0f; // Reset the timer when the path is complete
        }
        else
        {
            pathIncompleteTimer += Time.deltaTime; // Increment the timer

            if (pathIncompleteTimer >= pathIncompleteWaitTime)
            {
                print("(GoToPosition) - Path Uncompleted.");
                isMovingToPos = false;
                SearchWalkPoint();
                pathIncompleteTimer = 0f; // Reset the timer after bailing out
            }
        }
        if (Vector3.Distance(transform.position, MovePos) < 2f)
        {
            print("(GoToPosition) - Path Completed!");
            agent.speed = agentWalkSpeed;
            isMovingToPos = false;
            SearchWalkPointFocused();
    }
}
    // Finds a Walkpoint either in a specific way or Randomly depending on conditions for the patrolling function.
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
                print("(SearchWalkPoint) - Found Valid Walkpoint");
                return;
            }
            else
            {
                print("(SearchWalkPoint) - Finding Random Walkpoint");
                Vector3 randomDirection = Random.insideUnitSphere * walkPointRange;
                randomDirection += transform.position;
                NavMesh.SamplePosition(randomDirection, out hit, walkPointRange, NavMesh.AllAreas);

                agent.SetDestination(hit.position);

                if (agent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    walkPoint = hit.position;
                    isWalkPointSet = true;
                    return;
                }
            }
        } while (!validWalkPoint && attempts < maxWalkPointAttempts);
    }
    // Runs after chasing to set the walkpoint to a more specific angle direction toward where the lastknown pos direction is.
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
                print("(SearchWalkPointFocused) - Finding Focused Walkpoint After Chase");
            }
            else
            {
                print("(SearchWalkPointFocused) - Finding Focused Random Walkpoint");
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
    // Called From other Functions listening for audiosources playing at a certain volume if so it will go to the location.
    private void Listening()
    {
        print("(Listening) - Function Called");
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
                        float hearingVolume = Mathf.Lerp(HearingDistanceMinVolume, HearingDistanceMaxVolume, (distance - 2) / (hearingDistance - 2));
                        if (source.volume > hearingVolume)
                        {
                            Target = source.transform.root.transform;
                            Vector3 randomPoint = source.transform.position + Random.insideUnitSphere * 5f;
                            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                            {
                                print("(Listening) - Found Sound, Moving To Pos");
                                MovePos = hit.position;
                                isMovingToPos = true;
                            }
                        }
                    }
                }
            }
        }
    }
    // Bool : Checks all lights attatched to the player and finds if its a spotlight and is enabled then it will check if its in a certain angle from the agent if so it will return true.
    private bool CanSeePlayerLight(Transform playerTransform)
    {
        print("(CanSeePlayerLight) - Function Called");
        Light light = playerTransform.GetComponent<Flashlight>()._flashlight;
        if (light.enabled)
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
                    print("(CanSeePlayerLight) - Light Found");
                    return true;
                }
            }
        }
        return false;
    }
    private bool IsPlayerLightOn(Transform playerTransform){
        Light light = playerTransform.GetComponent<Flashlight>()._flashlight;
        if (light.enabled) { return true;}
        return false;
    }
    // Calls the Door objects open function and gets ownership to do so.
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
    // Enumerator to go to the position set for the door and waits until it gets there then will call the door opening function.
    // if chasing or movingtopos is true at any time it will break out and cancel.
    IEnumerator CheckDistanceFromDoor(Vector3 pos, PhotonView doorview, StaticDoorInfo SDI)
    {
        while (Vector3.Distance(transform.position, pos) >= 2f )
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
    // Initial attack Function called on AttackRange to call the inital and release functions. 
    public IEnumerator Attack(Collider player)
    {
        print("ATTACK ENUMERATOR");
        isAttacking = true;
        PhotonView view = player.gameObject.GetComponent<PhotonView>();
        view.gameObject.GetComponent<InventoryManager>().DropAllItems();
        yield return new WaitForEndOfFrame();
        view.TransferOwnership(PhotonNetwork.LocalPlayer);
        isChasing = false;
        AttackInitial(view);
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !isMovingToPos);
        AttackRelease(view);
    }
    // Initial attack which gets the view and sets the ownership of the player to the agent
    // Also sets transform to pick up the player and face the agent then sets the MovePos to the player cage
    public void AttackInitial(PhotonView view)
    {
        print("ATTACK INITIAL");
        Transform _holder = GameObject.FindGameObjectWithTag("PlayerHolder").transform;
        photonView.RPC(nameof(DonnyCatching), RpcTarget.AllViaServer, view.ViewID, photonView.ViewID);
        Vector3 Pos = new Vector3(transform.position.x, view.transform.position.y, transform.position.z);
        view.transform.LookAt(Pos, Vector3.up);
        view.transform.position = _holder.position;
        MovePos = playerCagePos;
        isMovingToPos = true;
    }
    // Release attack which reverts ownership transfer and calls the rpc to drop the player and then sets movetopos to false and sets walkpoint to false to call it.
    bool hasReleased = false;
    public void AttackRelease(PhotonView view)
    {
        print("ATTACK RELEASED");
        if (doorOpener.CageInfo.isLocked && !hasReleased){
            hasReleased = true;
            StartCoroutine(itemSpawning.SpawnItem("Jail Key", 1, itemSpawning.JailKeySpawnPoints));
        }
        else if (!doorOpener.CageInfo.isLocked) { StartCoroutine(doorOpener.LockCage());} 
        view.transform.position = playerDropPos;
        view.TransferOwnership(view.Owner);
        photonView.RPC(nameof(DonnyRelease), RpcTarget.AllViaServer, view.ViewID);
        agent.speed = agentWalkSpeed;
        Running = false;
        Walking = true;
        hasReleased = false;
        isAttacking = false;
        StartCoroutine(nameof(GracePeriod));

    }
    // Starts grace period
    private IEnumerator GracePeriod()
    {
        isGraceMode = true;
        yield return new WaitForSeconds(gracePeriodLength);
        isGraceMode = false;
    }
    // Controls all the animation based on move speed
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
            proceduralAnim.bounceAmplitude = 0.3f;
        }
        if (agent.speed == agentWalkSpeed)
        {
            Walking = true;
            Running = false;
            proceduralAnim.smoothness = 3;
            proceduralAnim.stepHeight = 0.5f;
            proceduralAnim.stepLength = 2.3f;
            proceduralAnim.angularSpeed = 7;
            proceduralAnim.bounceAmplitude = 0.2f;
        }
        animationPos = agent.transform.position;
    }
    // Rpc to set agent parameters based on the static difficulty set in the lobby by the host.
    [PunRPC]
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
    // Rpc to disable all scripts on the player while being grabbed and sets tag and layer and parents.
    [PunRPC]
    public void DonnyCatching(int playerid, int viewid)
    {
        PhotonView playerview = PhotonView.Find(playerid);
        PhotonView view = PhotonView.Find(viewid);
        Transform _holder = GameObject.FindGameObjectWithTag("PlayerHolder").transform;
        playerview.transform.SetParent(_holder);
        playerview.GetComponent<PlayerMovement>().enabled = false;
        playerview.GetComponent<InventoryManager>().enabled = false;
        playerview.GetComponent<CharacterController>().enabled = false;
        playerview.GetComponent<CapsuleCollider>().enabled = false;
        playerview.GetComponent<Interactions>().enabled = false;
        playerview.tag = "Grabbed";
        playerview.gameObject.layer = 11;
    }
    // Rpc to re-enable the components and reset tags and layer and also parent.
    [PunRPC]
    public void DonnyRelease(int playerid)
    {
        PhotonView playerview = PhotonView.Find(playerid);
        playerview.transform.SetParent(null);
        playerview.GetComponent<PlayerMovement>().enabled = true;
        playerview.GetComponent<InventoryManager>().enabled = true;
        playerview.GetComponent<CharacterController>().enabled = true;
        playerview.GetComponent<CapsuleCollider>().enabled = true;
        playerview.GetComponent<Interactions>().enabled = true;
        playerview.tag = "Player";
        playerview.gameObject.layer = 8;
    }
    // Draws Sight Range, Max Range and Attack Range OverlapSphere
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, maxRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    // Bool For Setting and Getting Walking and Running
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
