using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type{ Ammo, Coin, Grenade, Heart, Weapon }
    public Type type;

    public int value;

    Rigidbody rigid;
    SphereCollider newCollider;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        newCollider = GetComponent<SphereCollider>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up * 25 * Time.deltaTime);
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Floor")
        {
            rigid.isKinematic = true;
            newCollider.enabled = false;
        }
    }
}
