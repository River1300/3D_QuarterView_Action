using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public GameObject[] weapons;
    public bool[] hasWeapon;

    public int maxHealth;
    public int health;
    public int maxAmmo;
    public int ammo;
    public int maxGrenade;
    public int grenade;
    public int coin;

    public float speed;
    public float jumpPower;

    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;

    float hAxis;
    float vAxis;

    int equipWeaponIndex = -1;

    GameObject nearObject;
    GameObject equipWeapon;

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
        Interaction();
        Move();
        Turn();
        Jump();
        Dodge();
        Swap();
    }

    void InputSystem()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Keyboard.current.eKey.wasPressedThisFrame;
        sDown1 = Keyboard.current.digit1Key.wasPressedThisFrame;
        sDown2 = Keyboard.current.digit2Key.wasPressedThisFrame;
        sDown3 = Keyboard.current.digit3Key.wasPressedThisFrame;
    }

    void Interaction()
    {
        if(iDown && nearObject != null)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapon[weaponIndex] = true;
                Destroy(nearObject);
            }
        }
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
        if(jDown && !isJump && !isDodge && !isSwap && moveVec == Vector3.zero)
        {
            isJump = true;
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
        }
    }

    void Dodge()
    {
        if(jDown && !isJump && !isDodge && !isSwap && moveVec != Vector3.zero)
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

    void Swap()
    {
        if(sDown1 && (!hasWeapon[0] || equipWeaponIndex == 0)) return;
        if(sDown2 && (!hasWeapon[1] || equipWeaponIndex == 1)) return;
        if(sDown3 && (!hasWeapon[2] || equipWeaponIndex == 2)) return;

        int weaponIndex = -1;

        if(sDown1) weaponIndex = 0;
        if(sDown2) weaponIndex = 1;
        if(sDown3) weaponIndex = 2;

        if((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if(equipWeapon != null) equipWeapon.SetActive(false);

            equipWeapon = weapons[weaponIndex];
            equipWeapon.SetActive(true);
            equipWeaponIndex = weaponIndex;
            anim.SetTrigger("doSwap");
            StartCoroutine(SwapRoutine());
        }
    }

    IEnumerator SwapRoutine()
    {
        isSwap = true;

        yield return new WaitForSeconds(0.4f);

        isSwap = false;
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Floor")
        {
            isJump = false;
            anim.SetBool("isJump", false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon")
        {
            nearObject = other.gameObject;
            Debug.Log(nearObject.name + "Stay");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Weapon")
        {
            nearObject = null;
            Debug.Log("Exit");
        }
    }
}
