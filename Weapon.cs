using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range }
    public Type type;

    public int damage;
    public float rate;

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    public void Use()
    {
        if(type == Weapon.Type.Melee)
        {
            StopCoroutine(Swing());
            StartCoroutine(Swing());
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
}
