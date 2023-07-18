using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range }
    public Type type;

    public int maxAmmo;
    public int curAmmo;
    public int damage;
    public float rate;

    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    public void Use()
    {
        if(type == Weapon.Type.Melee)
        {
            StopCoroutine(Swing());
            StartCoroutine(Swing());
        }
        else if(type == Weapon.Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine(Shot());
        }
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.15f);

        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.7f);

        meleeArea.enabled = false;
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
        GameObject instanceBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody rigid = instanceBullet.GetComponent<Rigidbody>();
        rigid.velocity = bulletPos.forward * 50;

        yield return new WaitForSeconds(0.25f);

        GameObject instanceBulletCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody rigidCase = instanceBulletCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -1) + bulletCasePos.up * Random.Range(1, 3);
        rigidCase.AddForce(caseVec, ForceMode.Impulse);
        rigidCase.AddTorque(Vector3.up * 15, ForceMode.Impulse);
    }
}
