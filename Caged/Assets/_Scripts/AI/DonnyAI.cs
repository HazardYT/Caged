using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Photon.Realtime;
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
    public Animator anim;
    [SerializeField] private Transform agentEyes;
    [SerializeField] private DonnyDoorOpener doorOpener;
    [SerializeField] private ItemSpawning itemSpawning;
    [SerializeField] private ProceduralAnimation proceduralAnim;
    [SerializeField] private GameManager manager;
    [SerializeField] private Transform holder;
    [SerializeField] private UnityEngine.Rendering.VolumeProfile holdingVolume;
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
    public bool _attackrun;
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
    bool isMoveToLightSwitchCalled = false;

    private void Awake(){
        #if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
        #else 
            Debug.unityLogger.logEnabled = false;
        #endif
    }
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
            DoorMemory();
            Animation();
            if (!isAttacking) { AttackRange(); }
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
                        //Debug.Log("ATTACK Static Door Called");
                        StaticDoorOpen(obj);
                    }
                    if (hit.collider.CompareTag("Player") && !isAttacking)
                    {
                        //Debug.Log("ATTACK player Called");
                        StartCoroutine(Attack(hit.collider));
                    }
                }
            }
        }
    }
    // Sight Range Function That Checks for players and there lights and updates the isChasing var accordingly
    public void SightRange()
    {
        //Debug.Log("(SightRange) - Function Called");
        foreach (Collider obj in overlapSphereResults)
        {
            if (obj == null) { continue; }
            if (obj.CompareTag("Player") || obj.CompareTag("StaticDoor"))
            {
                if (Vector3.Distance(transform.position, obj.transform.position) <= sightRange)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(agentEyes.position, obj.transform.position - agentEyes.position, out hit))
                    {
                        // Check if can see line of sight to player and is in range, if so, isChase is true
                        if (hit.collider.CompareTag("Player"))
                        {
                            //Debug.Log("(SightRange) - Chasing");
                            Target = obj.transform;
                            isMovingToPos = false;
                            isChasing = true;
                        }
                        // Check if can't see player but is also chasing and there's no light seen
                        if (obj.CompareTag("Player") && !hit.collider.CompareTag("Player") && isChasing && !CanSeePlayerLight(obj.transform))
                        {
                            //Debug.Log("(SightRange) - Going To Position");
                            isChasing = false;
                            MovePos = Target.position;
                            isMovingToPos = true;
                            Target = null;
                        }
                        if (hit.collider.CompareTag("StaticDoor") && isChasing){
                            if (Vector3.Distance(Target.position, hit.transform.position) < 1.5f){
                                if (Vector3.Distance(transform.position, hit.transform.position) < 2){
                                PhotonView doorview = obj.gameObject.GetComponent<PhotonView>();
                                StaticDoorInfo SDI = doorview.gameObject.GetComponent<StaticDoorInfo>();
                                StaticDoorOpen(obj);
                                StartCoroutine(Attack(hit.collider));
                                }
                            }
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
        //Debug.Log("(SightRangeDoors) - Function Called");
        foreach (Collider obj in overlapSphereResults)
        {
            if (obj == null) {continue;}
            if (obj.CompareTag("StaticDoor") || obj.CompareTag("Player")){
                if (Vector3.Distance(transform.position, obj.transform.position) <= sightRange){
                    if (Physics.Raycast(agentEyes.position, obj.transform.position, out RaycastHit hit))
                    {
                        if (hit.collider.CompareTag("StaticDoor"))
                        {
                            //Debug.Log("(SightRangeDoors) - Random Open Check");
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
                            else return; //Debug.Log("(SightRangeDoors) - Error on Static Door Info");
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
                        //Debug.Log("(DoorMemory) - Door Memory Called");
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
        foreach (Collider obj in overlapSphereResults)
        {
            if (obj == null) {return;}
            if (obj.CompareTag("Player")){
                if (Physics.Raycast(agentEyes.position, obj.transform.position - agentEyes.position, out RaycastHit hit))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        bool flashlightActive = CanSeePlayerLight(obj.transform);
                        if (flashlightActive)
                        {
                            Debug.DrawLine(transform.position, obj.transform.position, Color.white);
                            isMovingToPos = false;
                            Target = obj.transform;
                            isChasing = true;
                            return;
                        }
                    }
                    if (!hit.collider.CompareTag("Player") && !isMovingToPos && isChasing && !CanSeePlayerLight(obj.transform))
                    {
                        //Debug.Log("(MaxRange) - Lost Player Line Of Sight Moving To Last Pos.");
                        isChasing = false;
                        MovePos = Target.position;
                        isMovingToPos = true;
                        Target = null;
                    }
                }
            }
            if (obj.CompareTag("LightSwitch") && !isChasing){
                LightInfo lightInfo = obj.transform.GetComponentInChildren<LightInfo>();
                if (lightInfo.isOn){
                    MovePos = obj.transform.position;
                    isMovingToPos = true;
                    if (!isMoveToLightSwitchCalled) {StartCoroutine(MoveToLightSwitch(lightInfo)); isMoveToLightSwitchCalled = true;}
                }
            }
        }
    }
    public IEnumerator MoveToLightSwitch(LightInfo info){
        yield return new WaitUntil(() => Vector3.Distance(transform.position, MovePos) <= 2);
        StartCoroutine(info.LightSwitchToggle());
        Debug.Log("DONNY TOGGLING LIGHT");
        isMoveToLightSwitchCalled = false;
    }
    // Called From other functions which predicts the movement and actually chases the player while being run. as long as isChasing is true.
    public void Chase()
    {
        //Debug.Log("(Chase) - Function called");
        float distanceToTarget = Vector3.Distance(transform.position, Target.position);

        if (distanceToTarget <= maxRange)
        {   
            if (Target.CompareTag("Player")){
                CharacterController playerController = Target.GetComponent<CharacterController>();
                float predictionFactor = Mathf.Lerp(0.05f, predictionAmount, distanceToTarget / maxRange);
                Vector3 predictedPosition = Target.position + (playerController.velocity * predictionFactor);
                agent.SetDestination(predictedPosition);
                Debug.DrawLine(agentEyes.position, predictedPosition, Color.yellow);
            }
            else{
                agent.SetDestination(Target.position);
            }
            Debug.DrawLine(agentEyes.position, Target.position, Color.red);
        }
    }
    // Called From other functions which checks searchwalkpoint and gets a position then will patroll finding random points.
    public void Patrolling()
    {
        if (!isWalkPointSet) SearchWalkPoint();

        //Debug.Log("(Patrolling) - Function called");
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
                //Debug.Log("(GoToPosition) - Path Uncompleted.");
                isMovingToPos = false;
                SearchWalkPoint();
                pathIncompleteTimer = 0f; // Reset the timer after bailing out
            }
        }
        if (Vector3.Distance(transform.position, MovePos) < 2f)
        {
            //Debug.Log("(GoToPosition) - Path Completed!");
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
                //Debug.Log("(SearchWalkPoint) - Found Valid Walkpoint");
                return;
            }
            else
            {
                //Debug.Log("(SearchWalkPoint) - Finding Random Walkpoint");
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
                //Debug.Log("(SearchWalkPointFocused) - Finding Focused Walkpoint After Chase");
            }
            else
            {
                //Debug.Log("(SearchWalkPointFocused) - Finding Focused Random Walkpoint");
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
        //Debug.Log("(Listening) - Function Called");
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
                                //Debug.Log("(Listening) - Found Sound, Moving To Pos");
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
        //Debug.Log("(CanSeePlayerLight) - Function Called");
        Light light = playerTransform.root.GetComponent<Flashlight>()._flashlight;
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
                    //Debug.Log("(CanSeePlayerLight) - Light Found");
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
    private void StaticDoorOpen(Collider collider)
    {
        PhotonView doorview = collider.gameObject.GetComponent<PhotonView>();
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
        //Debug.Log("ATTACK ENUMERATOR");
        isAttacking = true;
        AttackRun = true;
        PhotonView view = player.gameObject.GetComponent<PhotonView>();
        PhotonView managerView = manager.GetComponent<PhotonView>();
        managerView.RPC(nameof(GameManager.StartJumpScare),view.Owner, manager.GetComponent<PhotonView>().ViewID);
        GameObject playercamera = view.gameObject.GetComponent<PlayerMovement>().playerCam.gameObject;
        playercamera.GetComponent<UnityEngine.Rendering.Volume>().profile = holdingVolume;
        view.gameObject.GetComponent<InventoryManager>().DropAllItems();
        yield return new WaitForEndOfFrame();
        Player ownerPlayer = PhotonNetwork.CurrentRoom.GetPlayer(view.OwnerActorNr);
        view.RequestOwnership();
        isChasing = false;
        photonView.RPC(nameof(DonnyCatching), RpcTarget.AllViaServer, view.ViewID, photonView.ViewID);
        yield return new WaitForEndOfFrame();
        //AttackInitial(view);
        MovePos = playerCagePos;
        isMovingToPos = true;
        yield return new WaitUntil(() => !isMovingToPos);
        AttackRelease(view, ownerPlayer);
    }
    // Initial attack which gets the view and sets the ownership of the player to the agent
    // Also sets transform to pick up the player and face the agent then sets the MovePos to the player cage
    // public void AttackInitial(PhotonView view)
    // {
    //     view.transform.SetParent(holder);
    //     PlayerMovement pm = view.gameObject.GetComponent<PlayerMovement>();
    //     pm.Walking = false; pm.Running = false; pm.Crouching = false; pm.Prone = false;
    //     Quaternion newRotation = Quaternion.Euler(0, -180, 0);
    //     view.transform.localRotation = newRotation;
    // }
    // Release attack which reverts ownership transfer and calls the rpc to drop the player and then sets movetopos to false and sets walkpoint to false to call it.
    bool hasReleased = false;
    public void AttackRelease(PhotonView view, Player player)
    {
        //Debug.Log("ATTACK RELEASED");
        if (doorOpener.CageInfo.isLocked && !hasReleased){
            hasReleased = true;
            StartCoroutine(itemSpawning.SpawnItem("Jail Key", 1, itemSpawning.JailKeySpawnPoints));
        }
        else if (!doorOpener.CageInfo.isLocked) { StartCoroutine(doorOpener.LockCage());} 
        view.transform.position = playerDropPos;
        view.TransferOwnership(player);
        photonView.RPC(nameof(DonnyRelease), RpcTarget.AllViaServer, view.ViewID);
        GameObject playercamera = view.gameObject.GetComponent<PlayerMovement>().playerCam.gameObject;
        playercamera.GetComponent<UnityEngine.Rendering.Volume>().profile = null;
        agent.speed = agentWalkSpeed;
        AttackRun = false;
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
        playerview.transform.localPosition = Vector3.zero;
        playerview.transform.localRotation = Quaternion.Euler(0,-180, 0);
        playerview.GetComponent<PlayerMovement>().isEnabled = false;
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
        playerview.transform.rotation = Quaternion.Euler(0, 0, 0);
        playerview.GetComponent<PlayerMovement>().isEnabled = true;
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
    public bool AttackRun
    {
        get { return _attackrun; }
        set
        {
            if (value == _attackrun) return;
            _attackrun = value;
            anim.SetBool("Grabbing", _attackrun);
        }
    }
}
