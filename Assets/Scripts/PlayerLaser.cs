using System.Collections.Generic;
using UnityEngine;

public class PlayerLaser : MonoBehaviour
{
    [Header("Laser Properties")]
    public int damage = 1;
    public int maxPierces = 0; // 0 = Hủy ngay khi trúng mục tiêu đầu tiên, 1+ = Xuyên qua mục tiêu
    private int currentPierces = 0;

    // Tránh việc đạn gây sát thương trùng lặp nhiều lần lên cùng một mục tiêu trong 1 frame
    private HashSet<int> hitTargetIds = new HashSet<int>();

    public void SetPiercing(int extraPierces)
    {
        maxPierces = extraPierces;
    }

    public bool CanHit(GameObject target)
    {
        if (target == null) return false;
        int id = target.GetInstanceID();
        if (hitTargetIds.Contains(id)) return false;
        return true;
    }

    public void RegisterHit(GameObject target)
    {
        if (target != null)
        {
            hitTargetIds.Add(target.GetInstanceID());
        }

        currentPierces++;
        if (currentPierces > maxPierces)
        {
            Destroy(gameObject);
        }
    }
}
