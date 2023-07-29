using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D }
    [Header("# Info")]
    public Type type;
    public int maxHealth;
    public int curHealth;
    public int score;
    [Header("# Object")]
    public GameManager manager;
    public GameObject[] coins;
    public Transform target;
    public GameObject bullet;
    public BoxCollider meleeArea;
    [Header("# Child")]
    public bool isAttack;
    public bool isChase;
    public bool isDead;
    public NavMeshAgent nav;
    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public Animator anim;
    public MeshRenderer[] meshs;

    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();

        if(type != Type.D)
            Invoke("ChaseStart", 3.0f);
    }

    void FixedUpdate()
    {
        FreezeVelocity();
        Targeting();
    }

    void Update()
    {
        if(type != Type.D && nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }

    void FreezeVelocity()
    {
        if(isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void Targeting()
    {
        if(isDead) return;
        if(type == Type.D) return;

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
                targetRange = 30.0f;
                break;
            case Type.C:
                targetRadius = 0.5f;
                targetRange = 45.0f;
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
                yield return new WaitForSeconds(0.75f);
                meleeArea.enabled = true;
                yield return new WaitForSeconds(1.0f);
                meleeArea.enabled = false;
                yield return new WaitForSeconds(1.0f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.55f);
                rigid.AddForce(transform.forward * 100, ForceMode.Impulse);
                meleeArea.enabled = true;
                yield return new WaitForSeconds(1.0f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;
                yield return new WaitForSeconds(2.0f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject enemyBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = enemyBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 30.0f;
                yield return new WaitForSeconds(2.5f);
                break;
        }

        anim.SetBool("isAttack", false);
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
        if(!isDead)
        {
            foreach(MeshRenderer mesh in meshs)
            {
                mesh.material.color = Color.red;
            }

            if(curHealth > 0)
            {
                yield return new WaitForSeconds(0.3f);

                foreach(MeshRenderer mesh in meshs)
                {
                    mesh.material.color = Color.white;
                }
            }
            else
            {
                foreach(MeshRenderer mesh in meshs)
                {
                    mesh.material.color = Color.gray;
                }
                isDead = true;
                isChase = false;
                nav.enabled = false;

                Player player = target.GetComponent<Player>();
                player.score += score;

                int ranCoin = Random.Range(0, 3);
                Instantiate(coins[ranCoin], transform.position, Quaternion.identity);

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
                
                switch(type)
                {
                case Enemy.Type.A:
                    manager.enemyCntA--;
                    break;
                case Enemy.Type.B:
                    manager.enemyCntB--;
                    break;
                case Enemy.Type.C:
                    manager.enemyCntC--;
                    break;
                case Enemy.Type.D:
                    manager.enemyCntD--;
                    break;
                }

                Destroy(gameObject, 4.0f);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(isDead) return;

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
