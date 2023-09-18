using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Targeter : MonoBehaviour
{
    [SerializeField] CinemachineTargetGroup _targetGroup;
    [SerializeField] CinemachineVirtualCamera _targetingCamera;
    
    public Target CurrentTarget { get; private set; }
    List<Target> _targets = new List<Target>();

    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Target>(out Target target)) return;
        _targets.Add(target);
        target.OnDestroyed += RemoveTarget;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Target>(out Target target)) return;
        RemoveTarget(target);
    }

    void RemoveTarget(Target target)
    {
        if (CurrentTarget == target)
        {
            _targetingCamera.Priority = 9;
            _targetGroup.RemoveMember(CurrentTarget.transform);
            CurrentTarget = null;
        }

        target.OnDestroyed -= RemoveTarget;
        _targets.Remove(target);
    }

    public bool SelectTarget()
    {
        if (_targets.Count == 0)
            return false;
        Target closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Target target in _targets)
        {
            Vector2 viewPos = _mainCamera.WorldToViewportPoint(target.transform.position);
            if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
            {
                
            }

            Vector2 toCenter = viewPos - new Vector2(.5f, .5f);
            if (toCenter.sqrMagnitude < closestDistance)
            {
                closestTarget = target;
                closestDistance = toCenter.sqrMagnitude;
            }
        }

        if (closestTarget == null) return false;

        CurrentTarget = closestTarget;
        _targetGroup.AddMember(CurrentTarget.transform, 1f, 2f);
        _targetingCamera.Priority = 11;
        return true;
    }

    public void Cancel()
    {
        if (CurrentTarget == null) return;
        _targetingCamera.Priority = 9;
        _targetGroup.RemoveMember(CurrentTarget.transform);
        CurrentTarget = null;
    }
}

