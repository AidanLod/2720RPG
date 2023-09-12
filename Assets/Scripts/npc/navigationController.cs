using System;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(RPGCharacterController))]

public class navigationController : MonoBehaviour
{

    [SerializeField] float _moveSpeed = 1f;

    [SerializeField] float _maxNavPathLength = 40f;

    NavMeshAgent _navMeshAgent;

    RPGCharacterController _rpgCharacterController;

    Animator _animator;

    void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rpgCharacterController = GetComponent<RPGCharacterController>();
        
    }

    void Start()
    {
        _animator = _rpgCharacterController.animator;
    }

    void Update()
    {
        if (_navMeshAgent.velocity.sqrMagnitude > 0)
        {
            _animator.SetBool(AnimationParameters.Moving, true);
            _animator.SetFloat(AnimationParameters.VelocityZ, _moveSpeed);
            
        }
        else
        {
            StopAnimation();
        }
    }

    public void StopAnimation()
    {
        _animator.SetFloat(AnimationParameters.VelocityZ, 0f);
        _animator.SetBool(AnimationParameters.Moving, false);
    }

    public void MoveTo(Vector3 destination)
    {
        if (!CanMoveTo(destination)) return;
        _navMeshAgent.destination = destination;
        _navMeshAgent.speed = _moveSpeed * 7;
        _navMeshAgent.isStopped = false;
    }

    public bool CanMoveTo(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
        if (!hasPath) return false;
        if (path.status != NavMeshPathStatus.PathComplete) return false;
        if (GetPathLength(path) > _maxNavPathLength) return false;

        return true;
    }

    double GetPathLength(NavMeshPath path)
    {
        float total = 0;
        if (path.corners.Length < 2) return total;
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return total;
    }

    public bool HasPath()
    {
        return _navMeshAgent.hasPath;
    }
}
