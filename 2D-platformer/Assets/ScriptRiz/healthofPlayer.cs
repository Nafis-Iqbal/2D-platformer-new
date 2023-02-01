using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthofPlayer : MonoBehaviour
{
    Vector2 preLeft, preRight;
    public Vector2 leftBox , rightBox;
    public Transform rayThrowerLeft;
    public Transform rayThrowerRight;
    private float health = 100f;
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Knife")){
            health -= 15f;
        }
        if(other.CompareTag("HeavyKnife")){
            health -= 30f;
        }
    }
    float x;
    float y;

    private void Start() {
        preLeft = Vector2.zero;
        preRight = Vector2.zero;
    }
    private void FixedUpdate() {
        if(health <= 0f){
            // Destroy(gameObject);
        }

        if(Input.GetKeyDown("space")){
            Destroy(gameObject);
        }

        x = Input.GetAxis("Horizontal")*Time.fixedDeltaTime * 10f;
        y = Input.GetAxis("Vertical")* Time.fixedDeltaTime * 10f;

        transform.position = new Vector3(transform.position.x + x , transform.position.y + y , transform.position.z);


        RaycastHit2D hitLeft = Physics2D.Raycast(rayThrowerLeft.position, Vector2.left);
        RaycastHit2D hitright = Physics2D.Raycast(rayThrowerRight.position, Vector2.right);
        Debug.DrawRay(rayThrowerLeft.position, Vector2.left * 10f, Color.red);
        Debug.DrawRay(rayThrowerRight.position, Vector2.right * 10f, Color.green);

        if(hitLeft.collider.gameObject.CompareTag("RepositionBox")) {
            leftBox = hitLeft.collider.gameObject.transform.position;
        }
        if(hitright.collider.gameObject.CompareTag("RepositionBox")) {
            rightBox = hitright.collider.gameObject.transform.position;
        }
    }
}
