using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float jumpPower;

    bool wDown;
    bool jDown;

    bool isJump;
    bool isDodge;

    float hAxis;
    float vAxis;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anim;
    Rigidbody rigid;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        InputSystem();
        Move();
        Turn();
        Jump();
        Dodge();
    }

    void InputSystem()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if(isDodge) moveVec = dodgeVec;

        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Jump()
    {
        if(jDown && !isJump && !isDodge && moveVec == Vector3.zero)
        {
            isJump = true;
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
        }
    }

    void Dodge()
    {
        if(jDown && !isJump && !isDodge && moveVec != Vector3.zero)
        {
            isDodge = true;
            StartCoroutine(DodgeRoutine());
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
        }
    }

    IEnumerator DodgeRoutine()
    {
        speed *= 4;
        isDodge = true;
        dodgeVec = moveVec;
        anim.SetTrigger("doDodge");

        yield return new WaitForSeconds(0.35f);

        isDodge = false;
        speed *= 0.25f;
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Floor")
        {
            isJump = false;
            anim.SetBool("isJump", false);
        }
    }
}
