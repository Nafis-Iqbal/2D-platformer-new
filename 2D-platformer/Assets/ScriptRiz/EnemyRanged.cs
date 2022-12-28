using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRanged : EnemyBase
{
    public Transform rowPossition;
    private bool canThrow = true;
    public GameObject row;
    float timeBetweenShoots = 1.15f;

    IEnumerator Throw(){
        canThrow = false;
        yield return new WaitForSeconds(timeBetweenShoots);

        GameObject throwRow = Instantiate(row, rowPossition.position, Quaternion.identity);
        throwRow.transform.position = rowPossition.position;
        throwRow.transform.rotation = transform.rotation;

        int dir;
        if (transform.position.x > player.position.x)
        {
            dir = -1;
        }
        else
        {
            dir = 1;
        }

        Vector2 rowScale = throwRow.transform.localScale;
        rowScale.x *= dir;
        throwRow.transform.localScale = rowScale;
        canThrow = true;
    }

    public override void attack(){
        if (player.position.x > transform.position.x && transform.localScale.x < 0 || player.position.x < transform.position.x && transform.localScale.x > 0){
            Flip();
        }
        rb.velocity = Vector3.zero;
        if (canThrow){
            StartCoroutine(Throw());
        }
    }
}