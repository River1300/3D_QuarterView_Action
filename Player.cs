using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public GameManager manager;
    public Camera followCamera;
    public GameObject grenadeObj;
    public GameObject[] grenades;
    public GameObject[] weapons;
    public bool[] hasWeapon;

    public int maxHealth;
    public int health;
    public int maxAmmo;
    public int ammo;
    public int maxGrenade;
    public int grenade;
    public int maxCoin;
    public int coin;
    public int score;

    public float speed;
    public float jumpPower;

    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool fDown;
    bool rDown;
    bool gDown;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isFireReady;
    bool isReload;
    bool isBoarder;
    bool isDamage;
    bool isShop;
    bool isDead;

    float hAxis;
    float vAxis;
    float fireDelay;

    int equipWeaponIndex = -1;

    GameObject nearObject;
    public Weapon equipWeapon;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anim;
    Rigidbody rigid;
    MeshRenderer[] meshs;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        
        fireDelay = 2;
        if(!PlayerPrefs.HasKey("MaxScore"))
            PlayerPrefs.SetInt("MaxScore", 0);
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void Update()
    {
        if(isDead) return;

        InputSystem();
        Interaction();
        Move();
        Turn();
        Jump();
        Dodge();
        Swap();
        Grenade();
        Attack();
        Reload();
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);

        isBoarder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
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
        fDown = Mouse.current.leftButton.isPressed;
        rDown = Keyboard.current.rKey.wasPressedThisFrame;
        gDown = Mouse.current.rightButton.wasPressedThisFrame;
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
            else if(nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
        }
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if(isDodge) moveVec = dodgeVec;

        if(!isBoarder)
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);

        if(fDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if(Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
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
            if(equipWeapon != null) equipWeapon.gameObject.SetActive(false);

            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);
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

    void Grenade()
    {
        if(grenade == 0) return;

        if(gDown && !isSwap && !isReload && !isDodge)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if(Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 30;

                GameObject instanceGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instanceGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec * 2f, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                grenade--;
                grenades[grenade].SetActive(false);
            }
        }
    }

    void Attack()
    {
        if(equipWeapon == null || isShop) return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady &&!isJump && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if(equipWeapon == null) return;
        if(equipWeapon.type == Weapon.Type.Melee) return;
        if(ammo <= 0) return;

        if(rDown && !isReload && !isJump && !isDodge && !isSwap && equipWeapon.maxAmmo != equipWeapon.curAmmo)
        {
            isReload = true;
            anim.SetTrigger("doReload");
            Invoke("ReloadOut", 0.5f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = Mathf.Min(equipWeapon.maxAmmo - equipWeapon.curAmmo, ammo);
        equipWeapon.curAmmo += reAmmo;
        ammo -= reAmmo;

        isReload = false;
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;

        if(isBossAtk)
        {
            rigid.AddForce(transform.forward * -20.0f, ForceMode.Impulse);
        }

        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }

        yield return new WaitForSeconds(1.0f);

        if(isBossAtk)
        {
            rigid.velocity = Vector3.zero;
        }

        if(health <= 0 && !isDead)
        {
            OnDie();
        }

        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

        isDamage = false;
    }

    void OnDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Floor")
        {
            isJump = false;
            anim.SetBool("isJump", false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();

            switch(item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if(ammo > maxAmmo) ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if(coin > maxCoin) coin = maxCoin;
                    break;
                case Item.Type.Grenade:
                    grenades[grenade].SetActive(true);
                    grenade += item.value;
                    if(grenade > maxGrenade) grenade = maxGrenade;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if(health > maxHealth) health = maxHealth;
                    break;
            }

            Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyBullet")
        {
            if(!isDamage)
            {
                Bullet enemyBullet = other.gameObject.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAtk = other.name == "Boss Melee Area";

                StartCoroutine(OnDamage(isBossAtk));
            }
            if(other.GetComponent<Rigidbody>() != null) Destroy(other.gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon" || other.tag == "Shop")
        {
            nearObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Weapon")
        {
            nearObject = null;
            Debug.Log("Exit");
        }
        if(other.tag == "Shop")
        {
            Shop shop = other.GetComponent<Shop>();
            shop.Exit();

            isShop = false;
            nearObject = null;
        }
    }
}
