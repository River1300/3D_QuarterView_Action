using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject bossMissile;
    public Transform missilePortA;
    public Transform missilePortB;

    public bool isLook;

    Vector3 lookVec;
    Vector3 tauntVec;

    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();

        nav.isStopped = true;
        StartCoroutine(Think());
    }

    void Update()
    {
        if(isDead)
        {
            StopAllCoroutines();
            return;
        }

        if(isLook)
        {
            float hVec = Input.GetAxisRaw("Horizontal");
            float vVec = Input.GetAxisRaw("Vertical");

            lookVec = new Vector3(hVec, 0, vVec) * 3.0f;
            transform.LookAt(target.position + lookVec);
            rigid.isKinematic = true;
        }
        else
        {
            rigid.isKinematic = false;
            nav.SetDestination(tauntVec);
        }
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(1.0f);

        int ranAction = Random.Range(0, 5);

        switch(ranAction)
        {
            case 0:
            case 1:
                StartCoroutine(Missile());
                break;
            case 2:
            case 3:
                StartCoroutine(Rock());
                break;
            case 4:
                StartCoroutine(Taunt());
                break;
        }
    }

    IEnumerator Missile()
    {
        anim.SetTrigger("doShot");

        yield return new WaitForSeconds(0.2f);
        GameObject insMissileA = Instantiate(bossMissile, missilePortA.position, missilePortA.rotation);
        BossMissile missileA = insMissileA.GetComponent<BossMissile>();
        missileA.target = target;
        yield return new WaitForSeconds(0.3f);
        GameObject insMissileB = Instantiate(bossMissile, missilePortB.position, missilePortB.rotation);
        BossMissile missileB = insMissileB.GetComponent<BossMissile>();
        missileB.target = target;

        yield return new WaitForSeconds(2.5f);
        StartCoroutine(Think());
    }

    IEnumerator Rock()
    {
        anim.SetTrigger("doBigShot");
        isLook = false;
        Instantiate(bullet, transform.position, transform.rotation);

        yield return new WaitForSeconds(3.0f);
        isLook = true;
        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        anim.SetTrigger("doTaunt");
        tauntVec = target.position + lookVec;
        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;

        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;
        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;
        yield return new WaitForSeconds(1.0f);

        boxCollider.enabled = true;
        nav.isStopped = true;
        isLook = true;
        StartCoroutine(Think());
    }
}
