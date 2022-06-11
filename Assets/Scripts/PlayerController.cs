using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour
{
    //VARIABLES

    [SerializeField] float moveSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float runSpeed;

    [SerializeField] bool busy;
    [SerializeField] bool grounded;
    [SerializeField] bool defend;
    [SerializeField] Vector3 jumpSpeed;


    [SerializeField] float GroundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float gravity;
    [SerializeField] float jumpHeight;

    Vector3 moveDir;

    
    [SerializeField] float CurrentHealth = 100;
    [SerializeField] float damage = 10;
    [SerializeField] float MaxHealth = 100;

    

    //REFERENCES
    CharacterController controller;
    Animator animator;
    PhotonView PV;
    PhotonTransformViewClassic PT;

    [SerializeField] GameObject weaponObject;
    CapsuleCollider weapon;

    GameObject p1;
    GameObject p2;
    
    Image h_bar01;
    Image h_bar02;

    
    GameObject Win;
    GameObject Lose;
    GameObject Draw;

    
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        PT = GetComponent<PhotonTransformViewClassic>();

        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        weapon = weaponObject.GetComponent<CapsuleCollider>();

        h_bar01 = GameObject.Find("Health01").GetComponent<Image>();
        h_bar02 = GameObject.Find("Health02").GetComponent<Image>();
        
        Win = GameObject.FindWithTag("Win");
        Lose = GameObject.FindWithTag("Lose");
        Draw = GameObject.FindWithTag("Draw");
    }


    void Start() 
    {
        if(!PV.IsMine)
        {
            Destroy(GetComponent<Rigidbody>());
        }

        if(PhotonNetwork.IsMasterClient)
        {
            gameObject.tag = "Player01";
            weaponObject.tag = "Weapon01";
        }
        else
        {   
            gameObject.tag = "Player02";
            weaponObject.tag = "Weapon02";
        }
    }

    // Update is called once per frame
    void Update()
    {
         if(PhotonNetwork.PlayerList.Length ==2 && !PV.IsMine)
         {
            if(PhotonNetwork.IsMasterClient)
            {
                gameObject.tag = "Player02";
                weaponObject.tag = "Weapon02";
            }
            else
            {   
                gameObject.tag = "Player01";
                weaponObject.tag = "Weapon01";
            }
         }

        if(PV.IsMine && PhotonNetwork.PlayerList.Length ==2)
        {
            move();
        }


        //------------------Player- rotation------------//

        p1 = GameObject.FindWithTag("Player01");
        p2 = GameObject.FindWithTag("Player02");

        if(gameObject.tag == "Player01")
        {
            if(p1.transform.position.z > p2.transform.position.z)
            {
            p1.transform.eulerAngles = new  Vector3 (0,180,0);
            }
            else if(p1.transform.position.z < p2.transform.position.z)
            {
            p1.transform.eulerAngles = new  Vector3 (0,0,0);
            }
        }

        else if(gameObject.tag == "Player02")
        {
            if(p1.transform.position.z > p2.transform.position.z)
            {
            p2.transform.eulerAngles = new  Vector3 (0,0,0);
            }
            else if(p1.transform.position.z < p2.transform.position.z)
            {
            p2.transform.eulerAngles = new  Vector3 (0,180,0);
            }
        }

        
       
    }

    //// RPC GameOver ////
    [PunRPC]
    void gameOver()
    {
        if(PV.IsMine)
        return;

        if(Lose.activeSelf)
        {
            Draw.SetActive(true);
            return;
        }
        Win.SetActive(true);
    }

    void FixedUpdate()
    {
        if(!PV.IsMine)
            return;

        grounded = Physics.CheckSphere(transform.position , GroundCheck, groundMask);

        if(grounded && jumpSpeed.y <0)
            jumpSpeed.y = -2f;

        //------------- Move-Speed -------------------//

        Vector3 syncSpeed = moveDir;
            PV.RPC("SyncPosition",RpcTarget.All,syncSpeed);
        
        jumpSpeed.y += gravity*Time.fixedDeltaTime;

        Vector3 syncJump = jumpSpeed;
            PV.RPC("SyncPosition",RpcTarget.All,syncJump);


        //--------------- Mid-air ---------------------//

        if(transform.position.y < 0.5f && jumpSpeed.y<0)
        {
            animator.SetBool("Jump",false);
        }
        
    }



    //------------ RPC SyncPosition -----------------//
    [PunRPC]
    void SyncPosition(Vector3 speed)
    {
        controller.Move(speed*Time.fixedDeltaTime);
    }


    //////   Functions  //////

    void move()
    {
        float moveX = 0;
        float moveY = 0;
        float moveZ = CrossPlatformInputManager.GetAxis("Horizontal");

        moveDir = new Vector3(moveX, moveY , moveZ);

            if(!busy && grounded && !defend)
            {
                //     if(moveDir != Vector3.zero && !CrossPlatformInputManager.GetKey(KeyCode.LeftShift))
                // walk(moveZ);

                if(moveDir != Vector3.zero /*&& CrossPlatformInputManager.GetKey(KeyCode.LeftShift)*/)
                run(moveZ);

                else if(moveDir == Vector3.zero)
                idle();

                if(CrossPlatformInputManager.GetButtonDown("Jump"))
                jump(moveZ);

                if(CrossPlatformInputManager.GetButtonDown("Block"))
                block();
                
                if(CrossPlatformInputManager.GetButtonDown("Attack"))
                attack();
            }

            if(CrossPlatformInputManager.GetButtonUp("Block") && grounded)
                unblock();
                

            moveDir *= moveSpeed;
        
    }

    void idle()
    {
                PV.RPC("SyncMove",RpcTarget.All,0f);
    }

    void walk(float moveZ)
    {
        moveSpeed = walkSpeed;

        //FOR moveZ//  
            if(transform.eulerAngles == new  Vector3 (0,0,0))
            {
                if(moveZ == 1)
                PV.RPC("SyncMove",RpcTarget.All,0.25f);

                if(moveZ == -1)
                PV.RPC("SyncMove",RpcTarget.All,-0.25f);
            }

        
        //FOR -moveZ//
            if(transform.eulerAngles == new  Vector3 (0,180,0))
            {
                if(moveZ == 1)
                PV.RPC("SyncMove",RpcTarget.All,-0.25f);

                if(moveZ == -1)
                PV.RPC("SyncMove",RpcTarget.All,0.25f);
            }
    }

    void run(float moveZ)
    {   
        //FOR moveZ// 
        if(transform.eulerAngles == new  Vector3 (0,0,0))
        {
            if(moveZ == 1){
                moveSpeed = runSpeed;
                PV.RPC("SyncMove",RpcTarget.All,0.5f);
            }
            if(moveZ == -1){
                moveSpeed = runSpeed-1f;
                PV.RPC("SyncMove",RpcTarget.All,-0.5f);  
            }
        }

        //FOR -moveZ// 
        if(transform.eulerAngles == new  Vector3 (0,180,0))
        {
            if(moveZ == 1){
                moveSpeed = runSpeed;
                PV.RPC("SyncMove",RpcTarget.All,-0.5f); 
            }
            if(moveZ == -1){
                moveSpeed = runSpeed-1f;
                PV.RPC("SyncMove",RpcTarget.All,0.5f);
            }
        }
            

    }

    ///------------  RPC Move -----------///
    [PunRPC]
    void SyncMove(float speed)
    {
        animator.SetFloat("Speed", speed, 0.15f,Time.deltaTime);
    }

    void jump(float moveZ)
    {
        
        jumpSpeed.y = Mathf.Sqrt(-2*gravity*jumpHeight);

        animator.SetBool("Jump",true);
    }

    void block()
    {

     animator.SetBool("Defend",true);
     
     defend = true;
     busy = true;
     moveSpeed = 0;
     
    //  Invoke("unblock",1f);
    }
    
     void unblock()
     {
        animator.SetBool("Defend",false);
        
        defend = false;
        busy = false;
     }


    void attack()
    {
        PV.RPC("ToggleCollider",RpcTarget.All);
        weapon.enabled = !weapon.enabled;
        
        moveSpeed = walkSpeed/3;

        busy = true;

        Invoke ("toggler",0.7f);
    }

    void toggler()
    {   
        weapon.enabled = !weapon.enabled;

        busy = false;
    }


    //--------------  RPC for Attack  -----------------//
    [PunRPC]    
    void ToggleCollider ()
    {       
        animator.SetTrigger("Attack");

        Debug.Log("Weapon triggered");
    }
        


    void OnCollisionEnter(Collision other)
    {

        if(gameObject.tag == "Player01" && other.gameObject.tag == "Weapon02")
        {
            bool d1 = GetComponent<PlayerController>().defend;

            if(d1 == true)
                Debug.Log("blocked");


            if(d1 == false)
                PV.RPC("TakeDamage",RpcTarget.All, 1);
            else
                animator.SetTrigger("Blocked");
        }

        else if(gameObject.tag == "Player02" && other.gameObject.tag == "Weapon01")
        {   
            bool d2 = GetComponent<PlayerController>().defend;

            if(d2 == false)
                PV.RPC("TakeDamage",RpcTarget.All, 2);
            else
               animator.SetTrigger("Blocked");
        }
    }


//--------------  RPC for Take-Damage  -----------------//
    [PunRPC]    
    void TakeDamage(int id)
    {
        animator.SetTrigger("Damage");
        CurrentHealth += -damage;

        if(id == 1)
            h_bar01.fillAmount = CurrentHealth/MaxHealth;

        if(id == 2)
            h_bar02.fillAmount = CurrentHealth/MaxHealth;


        Debug.Log("Player " + id + " got hit");
    }

}