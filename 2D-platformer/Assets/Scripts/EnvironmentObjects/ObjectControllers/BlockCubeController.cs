using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCubeController : InteractionBase
{
    public SpriteRenderer blockSprite;
    Rigidbody2D rb2d;
    public float pushForce, pullForce, maxBlockSpeed;
    public Collider2D leftCollider, rightCollider;
    public float defaultGravityScale, fallingGravityScale;
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isInteractionEnabled)
        {
            if (playerMovement.transform.position.x > transform.position.x)
            {
                blockSprite.sortingOrder = GameManager.Instance.playerSortOrderInLayer - 1;
            }
            else
            {
                blockSprite.sortingOrder = GameManager.Instance.playerSortOrderInLayer + 1;
            }
        }

        if (onGround == false)
        {
            rb2d.gravityScale = fallingGravityScale;

            if (inUse == true)
            {
                Debug.Log("3");
                releaseBlockFromPlayer();
                return;
            }
        }
        else
        {
            rb2d.gravityScale = defaultGravityScale;
        }

        if (inUse)
        {
            if (playerMovement.transform.position.x > transform.position.x)
            {
                leftCollider.enabled = false;
                rightCollider.enabled = true;
            }
            else
            {
                rightCollider.enabled = false;
                leftCollider.enabled = true;
            }

            if (rb2d.velocity.x >= maxBlockSpeed)
            {
                rb2d.velocity = new Vector2(maxBlockSpeed, rb2d.velocity.y);
            }
            else if (rb2d.velocity.x <= -maxBlockSpeed)
            {
                rb2d.velocity = new Vector2(-maxBlockSpeed, rb2d.velocity.y);
            }
        }
        else
        {
            rightCollider.enabled = true;
            leftCollider.enabled = true;
        }
    }

    public override void OnInteract()
    {
        if (!inUse)
        {
            inUse = true;
            playerSpineAnimator.SetInteger("InteractID", targetObjectID);
            playerSpineAnimator.SetBool("InteractB", true);

            playerMovement.transform.SetParent(transform);
            GameManager.Instance.playerRB2D.simulated = false;
            interactionStartTime = Time.time;

            playerMovement.enterPushPullObjectState(rb2d, pushForce, pullForce);
            //Code to unify objects
        }
        else
        {
            Debug.Log("2");
            releaseBlockFromPlayer();
            //Code to break down objects
        }
    }

    public override void ForceCloseInteraction()
    {
        base.ForceCloseInteraction();
        Debug.Log("1");
        releaseBlockFromPlayer();
    }

    public void releaseBlockFromPlayer()
    {
        Debug.Log("Alr8");
        inUse = false;
        playerMovement.transform.SetParent(null);
        GameManager.Instance.playerRB2D.simulated = true;
        interactionEndTime = Time.time;

        playerSpineAnimator.SetBool("InteractB", false);
        playerSpineAnimator.SetBool("PushObject", false);
        playerSpineAnimator.SetBool("PullObject", false);

        playerMovement.exitPushPullObjectState();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.CompareTag("Platforms") && other != leftCollider && other != rightCollider && Time.time - interactionEndTime > 1.0f)
        {
            onGround = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.CompareTag("Platforms") && other != leftCollider && other != rightCollider && Time.time - interactionStartTime > 1.0f)
        {
            onGround = false;
        }
    }
}
