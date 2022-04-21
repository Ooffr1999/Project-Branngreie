using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;


public class Enemy_Lampe : MonoBehaviour
{
    public Enemy_Pathfind agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    public GameObject attackEffect;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    //Animation
    public SpriteRenderer _renderer;
    public Animator _animator;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<Enemy_Pathfind>();
    }

    private void Update()
    {
        //Checks for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange)
        {
            AttackPlayer();
            attackEffect.SetActive(true);
        }
        else attackEffect.SetActive(false);

        //Animation
        if (walkPoint.x < transform.position.x)
            _renderer.flipX = true;
        else _renderer.flipX = false;

        attackEffect.SetActive(!_animator.GetBool("IsMoving"));

        if (_renderer.flipX)
        {
            attackEffect.transform.localPosition = new Vector3(-1.5f, 1.2f, 0f);
            attackEffect.transform.localScale = new Vector3(-2, 1.2f, 0f);
        }
        else
        {
            attackEffect.transform.localPosition = new Vector3(1.5f, 1.2f, 0f);
            attackEffect.transform.localScale = new Vector3(2, 1.2f, 0f);
        }

    }

    private void Patrolling()
    {
        if (!walkPointSet) SearchWalkPoint();
        
        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    private void SearchWalkPoint()
    {
        _animator.SetBool("IsMoving", true);
        walkPoint = agent.GetRandomPathInRange(walkPointRange);
        walkPointSet = true;
    }
    private void ChasePlayer()
    {
        _animator.SetBool("IsMoving", true);
        agent.GetPath(player.transform.position);
    }
    private void AttackPlayer()
    {
        _animator.SetBool("IsMoving", false);

        if (!alreadyAttacked)
        {
            //Attack
            _animator.Play("Lamp_Attack");

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}