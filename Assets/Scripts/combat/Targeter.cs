using System.Collections.Generic;
using UnityEngine;

public class Targeter : MonoBehaviour
{

    public List<Target> _targets = new List<Target>();

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Target>(out Target target)) return;
        _targets.Add(target);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Target>(out Target target)) return;
        _targets.Remove(target);
    }
}

