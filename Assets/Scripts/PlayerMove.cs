using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    #region 변수
    public float speed;
    public float jumpPower;
    private bool turnDirection;
    private Vector2 boxCastSize = new Vector2(1.0f, 0.25f);
    private float boxCastMaxDistance = 1.0f;
    public List<AudioClip> audioList = new List<AudioClip>();
    AudioSource audioSource;


    public GameManager gameManager;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    CapsuleCollider2D playerCollider;

    private readonly int hashIsJump = Animator.StringToHash("isJumping");
    private readonly int hashIsWalk = Animator.StringToHash("isWalking");
    #endregion
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
        turnDirection = false;
    }
    
    void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioList[0];
                    break;
            case "ATTACK":
                audioSource.clip = audioList[1];
                break;
            case "DAMAGED":
                audioSource.clip = audioList[2];
                break;
            case "ITEM":
                audioSource.clip = audioList[3];
                break;
            case "DIE":
                audioSource.clip = audioList[4];
                break;
            case "FINISH":
                audioSource.clip = audioList[5];
                break;
        }
        audioSource.Play();
    }
    private void Update()
    {
        #region 주석1
        //vec.x = Input.GetAxisRaw("Horizontal");
        //vec.y = Input.GetAxisRaw("Vertical");

        //Stop Speed
        //if (Input.GetButtonUp("Horizontal"))            
        //    rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        #endregion 1
        //Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool(hashIsJump))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool(hashIsJump, true);
            PlaySound("JUMP");
        }

        //Animation
        if ((Mathf.Abs(rigid.velocity.x) == 0) || anim.GetBool(hashIsJump))
            anim.SetBool(hashIsWalk, false);
        else
            anim.SetBool(hashIsWalk, true);

        #region Direction Sprite
        //Direction Sprite
        if (Input.GetAxisRaw("Horizontal") == 1)
        {
            spriteRenderer.flipX = false;
            turnDirection = false;
        }
        else if (Input.GetAxisRaw("Horizontal") == -1)
        {
            spriteRenderer.flipX = true;
            turnDirection = true;
        }
        else if((Input.GetKey("left") && Input.GetKey("right")))
            spriteRenderer.flipX = !turnDirection;
        #endregion
    }
    void FixedUpdate()
    {
        //Move Speed
        float h = Input.GetAxisRaw("Horizontal");
        rigid.velocity = new Vector2(h * speed, rigid.velocity.y);

        //BoxCast를 이용한 Platform 인식. (RayCast의 단점을 보완)
        RaycastHit2D rayHit = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size-new Vector3(0.2f,0,0), 0f, Vector2.down, 0.02f, LayerMask.GetMask("Platform"));

        // RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1f, LayerMask.GetMask("Platform"));  //  rayHit 빔을 쏴서 맞은 오브젝트의 정보
        if (rigid.velocity.y < 0)
        {
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.8f)
                    anim.SetBool("isJumping", false);
                //Debug.Log(rayHit.collider.name);
                else anim.SetBool("isJumping", true);
            }
        }
        #region 주석2
        //rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        //Max Speed
        //if (rigid.velocity.x > maxSpeed)    //  Right Max Speed
        //    rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        //else if (rigid.velocity.x < maxSpeed * (-1))    //  Left Max Speed
        //    rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
        #endregion
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {   //Attack
            if(rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y){
                bool isSpike = collision.gameObject.name.Contains("Spike");
                if(!isSpike)
                    OnAttack(collision.transform);
                else
                    OnDamaged(collision.transform.position);
            }
            else
                OnDamaged(collision.transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Item")
        {   //Point
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");
            
            if(isBronze)
                gameManager.stagePoint += 50;
            else if(isSilver)
                gameManager.stagePoint += 100;
            else if(isGold)
                gameManager.stagePoint += 300;

            //Deactive Item
            collision.gameObject.SetActive(false);
            PlaySound("ITEM");
        }
        else if(collision.gameObject.tag == "Finish")
        {   //Next Stage
            gameManager.NextStage();
            PlaySound("FINISH");
        }
    }

    void OnAttack(Transform enemy)
    {
        //Point
        gameManager.stagePoint += 100;
        //Reaction Force
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        //Enemy Die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
        PlaySound("ATTACK");
    }

    void OnDamaged(Vector2 targetPos)
    {
        //Health down
        gameManager.HealthDown();
        //change Layer
        gameObject.layer = 9; // layer 9 = PlayerDamaged

        //view alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f); // Color(R,G,B, 투명도)

        //reaction Force
        int pDir = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(pDir,1)*5, ForceMode2D.Impulse);

        //Animation
        anim.SetTrigger("doDamaged");
        PlaySound("DAMAGED");

        Invoke("OffDamaged", 2);
    }

    void OffDamaged()
    {
        gameObject.layer = 8;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        //Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        //Sprite Flip Y
        spriteRenderer.flipY = true;
        //Collider Disable
        playerCollider.enabled = false;
        //Die Effect Jump
        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
        PlaySound("DIE");
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
}
