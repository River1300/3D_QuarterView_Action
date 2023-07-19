using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponent<MeshRenderer>().material;
    }

    public void HitByGrenade(Vector3 explosionVec)
    {
        curHealth -= 100;

        Vector3 reactVec = transform.position - explosionVec;
        reactVec = reactVec.normalized;
        reactVec += Vector3.up * 10f;

        StartCoroutine(OnDamaged(reactVec, true));
    }

    IEnumerator OnDamaged(Vector3 vec, bool isGrenade)
    {
        mat.color = Color.red;

        yield return new WaitForSeconds(0.3f);

        if(curHealth > 0)
        {
            mat.color = Color.white;
        }
        else
        {
            mat.color = Color.gray;

            if(isGrenade)
            {
                rigid.AddForce(vec * 5, ForceMode.Impulse);
                rigid.freezeRotation = false;
                rigid.AddTorque(vec * 15, ForceMode.Impulse);
            }
            else
            {
                rigid.AddForce(vec * 5, ForceMode.Impulse);
            }
            
            gameObject.layer = 13;
            Destroy(gameObject, 4.0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Vector3 reactVec = transform.position - other.transform.position;
        reactVec = reactVec.normalized;
        reactVec += Vector3.up * 3f;

        if(other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            StartCoroutine(OnDamaged(reactVec, false));
        }
        else if(other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            StartCoroutine(OnDamaged(reactVec, false));
            Destroy(other.gameObject);
        }
    }
}
