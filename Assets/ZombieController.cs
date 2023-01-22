using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{

    [SerializeField] private Transform player;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private State state;
    

    private NavMeshAgent _navMeshAgent;
    private Animator _animator;

    private enum State
    {
        Walking,
        Attacking,
        Dead
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }
    
    private void Start()
    {
        currentHealth = maxHealth;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        state = State.Walking;
    }

    private void Update()
    {
        _navMeshAgent.SetDestination(player.position);
        
        if (currentHealth <= 0) state = State.Dead;
        else if (_navMeshAgent.remainingDistance >= _navMeshAgent.stoppingDistance) state = State.Walking;
        else state = State.Attacking;
        
        switch (state)
        {
            case State.Dead:
                _animator.SetBool("isDead", true);
                _navMeshAgent.isStopped = true;
                Invoke(nameof(Death), 5);
                return;
            case State.Attacking:
                _animator.SetBool("isAttacking", true);
                _animator.SetBool("isWalking", false);
                return;
            case State.Walking:
                _animator.SetBool("isWalking", true);
                _animator.SetBool("isAttacking", false);
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }

    }

    private void Death()
    {
        Destroy(gameObject);
    }
}
