using System;
using UnityEngine;

[Serializable]
public class ShipData
{
    public int id;
    public string shipName;
    [TextArea(2, 4)]
    public string description;
    public int price;
    public Sprite shipSprite;

    [Header("Base Ship Stats")]
    public int baseLives = 3;
    public float moveSpeed = 6.0f;
    public float fireRate = 0.20f;
    public int startingWeaponLevel = 1;
    public bool startsWithDrone = false;

    public ShipData(int id, string name, string desc, int price, Sprite sprite, int lives = 3, float speed = 6f, float fireRate = 0.2f, int startWep = 1, bool drone = false)
    {
        this.id = id;
        this.shipName = name;
        this.description = desc;
        this.price = price;
        this.shipSprite = sprite;
        this.baseLives = lives;
        this.moveSpeed = speed;
        this.fireRate = fireRate;
        this.startingWeaponLevel = startWep;
        this.startsWithDrone = drone;
    }
}
