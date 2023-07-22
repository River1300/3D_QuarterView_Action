using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C }
    public Type type;

    public Transform target;
    public GameObject bullet;

    public int maxHealth;
    public int curHealth;

    bool isChase;

    public BoxCollider meleeArea;
    public bool isAttack;

    NavMeshAgent nav;
    Rigidbody rigid;
    BoxCollider boxCollider;
    Animator anim;
    Material mat;

    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        anim = GetComponentInChildren<Animator>();
        mat = GetComponentInChildren<MeshRenderer>().material;

        Invoke("ChaseStart", 3.0f);
    }

    void FixedUpdate()
    {
        FreezeVelocity();
        Targeting();
    }

    void Update()
    {
        if(nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }

    void FreezeVelocity()
    {
        if(!isChase) return;

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void Targeting()
    {
        float targetRadius = 0.0f;
        float targetRange = 0.0f;
        RaycastHit[] rayHit;

        switch(type)
        {
            case Type.A:
                targetRadius = 1.5f;
                targetRange = 3.0f;
                break;
            case Type.B:
                targetRadius = 1.0f;
                targetRange = 12.0f;
                break;
            case Type.C:
                targetRadius = 0.5f;
                targetRange = 25.0f;
                break;
        }
        rayHit = Physics.SphereCastAll(
            transform.position, targetRadius, transform.forward, targetRange, 
            LayerMask.GetMask("Player")
        );
        if(rayHit.Length > 0 && !isAttack)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch(type)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;
                yield return new WaitForSeconds(1.0f);
                meleeArea.enabled = false;
                yield return new WaitForSeconds(1.0f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 25, ForceMode.Impulse);
                meleeArea.enabled = true;
                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;
                yield return new WaitForSeconds(2.0f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject enemyBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = enemyBullet.GetComponent<Rigidbody>();
                rigid.velocity = transform.forward * 30.0f;
                yield return new WaitForSeconds(2.5f);
                break;
        }

        anim.SetBool("isAttak", false);
        isAttack = false;
        isChase = true;
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
            isChase = false;
            nav.enabled = false;

            anim.SetBool("isWalk", false);
            anim.SetTrigger("doDie");
            gameObject.layer = 13;

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
