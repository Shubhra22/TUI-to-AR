using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IManager
{
    bool DependencyManager(Vector3 position);
    void ImpactManager();
    void ShowError(Vector3 position);
}
