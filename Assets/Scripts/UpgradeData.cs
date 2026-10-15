using System;
using UnityEngine;

public enum UpgradeType
{
    OrbitalDrone,    // Vệ tinh hộ vệ xoay quanh tàu, chặn và gây sát thương quái
    HomingMissile,   // Tên lửa tầm nhiệt tự động tìm và tiêu diệt mục tiêu
    PiercingLaser,   // Tia laser bắn xuyên qua nhiều thiên thạch và quái
    TitaniumHull,    // Giáp titan: Tăng 1 Mạng tối đa, hồi máu và tăng thời gian bất tử
    Overcharge,      // Quá tải năng lượng: Tăng tốc độ bắn và tốc độ bay của tàu
    GravitonCore     // Lõi hấp dẫn: Vĩnh viễn hút Sao và Vật phẩm trong phạm vi rộng
}

[Serializable]
public class UpgradeData
{
    public UpgradeType type;
    public string title;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public int currentLevel = 0;
    public int maxLevel = 3;

    public bool IsMaxed => currentLevel >= maxLevel;

    public UpgradeData(UpgradeType type, string title, string description, Sprite icon = null, int maxLevel = 3)
    {
        this.type = type;
        this.title = title;
        this.description = description;
        this.icon = icon;
        this.currentLevel = 0;
        this.maxLevel = maxLevel;
    }
}
