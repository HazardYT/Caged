using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DogAI : MonoBehaviour
{
    public Vector3 walkPoint;
    bool walkPointSet;
    public float SearchRadius;
    public Animator Anim;
    public NavMeshAgent agent;
    public LayerMask whatIsPlayer;
    private Transform target;
    public bool chasing = false;
    bool waitingforwalkpoint = false;
    public float ChaseSpeed;
    public float PatrolSpeed;

    public Vector3 roomMinBounds;
    public Vector3 roomMaxBounds;

    public bool _walking;
    public bool _running;

    private void Update()
    {
        if (agent.isOnNavMesh)
        {
            if (agent.velocity == Vector3.zero)
            {
                if (!walkPointSet && !waitingforwalkpoint)
                {
                    StartCoroutine(WaitForNewWalkPoint());
                }
                else
                {
                    Patrolling();
                }
            }
            Collider[] playersInChaseRange = Physics.OverlapSphere(transform.position, SearchRadius, whatIsPlayer);
            if (!chasing)
            {
                Patrolling();
                foreach (Collider player in playersInChaseRange)
                {
                    if (player.CompareTag("Player"))
                    {
                        target = player.transform;
                        if (target.position.x <= roomMinBounds.x && target.position.x >= roomMaxBounds.x && target.position.z <= roomMinBounds.z && target.position.z >= roomMaxBounds.z)
                        {
                            chasing = true;
                            return;
                        }
                    }
                }
            }
            if (chasing)
            {
                foreach (Collider player in playersInChaseRange)
                {
                    if (player.CompareTag("Player"))
                    {
                        target = player.transform;
                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, (target.position - transform.position), out hit))
                        {
                            if (target.CompareTag("Player") && target.position.x <= roomMinBounds.x && target.position.x >= roomMaxBounds.x && target.position.z <= roomMinBounds.z && target.position.z >= roomMaxBounds.z)
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
    }

    private void ChasePlayer()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        CharacterController playerController = target.GetComponent<CharacterController>();
        float predictionFactor = Mathf.Lerp(0, 0.5f, distanceToTarget / SearchRadius);
        Vector3 predictedPosition = target.position + (playerController.velocity * predictionFactor);
        if (predictedPosition.x <= roomMinBounds.x && predictedPosition.x >= roomMaxBounds.x && predictedPosition.z <= roomMinBounds.z && predictedPosition.z >= roomMaxBounds.z){
        agent.SetDestination(predictedPosition);
        }
        else {agent.SetDestination(target.position);}
        
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