using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;
using GlobalMember;

public class PlayerController : MonoBehaviour
{
    Dorothy_stat dorothyStat;
    Talk_balloon talkBalloon;
    Rigidbody2D rigid;
    PlayerState playerState;

    public float horizontalMotion;
    public float reserveHor;
    
    public float moveSpeed;
    public float basisSpeed;

    public SkeletonAnimation skeletonAnimation;
    TrackEntry trackEntry;

    Skeleton skeleton;

    public bool smashTimingBool;
    public bool moveBool;
    public bool jumpattackBool;
    public bool jumpActivated;
    
    public bool movingBlockBool;

    public int hurtWay;
    public int lastAttack;
    public int attackNumber;

    public float oldY, newY;
    public float aniSpeed;
    
    bool standBool;
    public bool floorBool;
    
    void Awake() {
        rigid = GetComponent<Rigidbody2D>();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        skeleton = GetComponent<SkeletonAnimation>().skeleton;
        talkBalloon = GetComponent<Talk_balloon>();
        dorothyStat = GetComponent<Dorothy_stat>();

        playerState = PlayerState.Instance;
    }

    void Start()
	{
        //도로시 시작위치
        transform.localPosition = Global.dorothy_next;
        WayInit();

        smashTimingBool = false;
        moveBool = false;
        newY = transform.localPosition.y;
        oldY = newY;
        
        moveSpeed = basisSpeed;
        skeletonAnimation.timeScale = aniSpeed;
        
        standBool = true;  // 튜토리얼에서 idle 고정 해제

        skeletonAnimation.state.Event += OnEvent;

        playerState.Horizontal = Horizontal.Idle;
        playerState.Vertical = Vertical.Airborne;
        playerState.DirectionFacing = DirectionFacing.Right;
        playerState.Attack_reserve = Attack_reserve.idle;
            
        if(Global.dorothyData.tutorial == 0) {  //튜토리얼
            playerState.State = State.landing;
            standBool = false;
            StartCoroutine("SleepEnd");
        } else {
            playerState.State = State.Idle;
            standBool = true;
        }
    }

    void WayInit () {  //방향 초기화
        reserveHor = Global.dorothy_hor;
        transform.localScale = new Vector3(-Global.dorothy_hor, 1, 1);
    }
    
    void AniSpeedInit() {
        skeletonAnimation.timeScale = aniSpeed;
    }

    public IEnumerator SleepEnd() {
        yield return YieldInstructionCache.WaitForSeconds(0.01f);
        dorothyStat.hp_bar.trans_hp_bar();
        yield return YieldInstructionCache.WaitForSeconds(2);
        skeletonAnimation.AnimationState.SetAnimation(0, "start_wakeup", false);
    }
    
    private void OnEvent ( TrackEntry trackEntry, Spine.Event e ) {     //Spine 모션이 끝난 후 이벤트
        
        switch(e.Data.name) {
            case "move_end":
                moveSpeed = basisSpeed;
                horizontalMotion = 0;
                break;
            case "attack_1_end":
                AniSpeedInit();
                switch(playerState.Attack_reserve) {
                    case Attack_reserve.attack:
                        Attack(1);
                        smashTimingBool = false;
                        break;
                    case Attack_reserve.smash:
                        Attack(3);
                        smashTimingBool = false;
                        break;
                    default:
                        CheckGroundEventEnd();
                        StartCoroutine("AttackAfterTime");
                        break;
                }
                break;
            case "attack_2_end":
                AniSpeedInit();
                if (playerState.Attack_reserve == Attack_reserve.attack) {
                    Attack(2);
                    smashTimingBool = false;
                } else if (playerState.Attack_reserve == Attack_reserve.smash) {
                    Attack(4);
                    smashTimingBool = false;
                } else {
                    CheckGroundEventEnd();
                    StartCoroutine("AttackAfterTime");
                }
                break;
            case "attack_3_end":
                AniSpeedInit();
                if (playerState.Attack_reserve == Attack_reserve.smash) {
                    Attack(5);
                    smashTimingBool = false;
                } else {
                    CheckGroundEventEnd();
                }
                break;
            case "smash_end":
                AniSpeedInit();
                CheckGroundEventEnd();
                playerState.Attack_reserve = Attack_reserve.idle;
                break;
            case "smash_timing":
                smashTimingBool = true;
                break;
            case "hurt_end":
                moveSpeed = basisSpeed;
                horizontalMotion = 0;
                if (playerState.Vertical == Vertical.Grounded ) {
                    playerState.State = State.Idle;
                    if (moveBool == true) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                    }
                } else {
                    playerState.State = State.jump;
                    skeletonAnimation.AnimationState.SetAnimation(0, "jump", true);
                }
                break;
            case "hug_end":
                if (moveBool == true) {
                    skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                } else {
                    skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                }
                moveSpeed = basisSpeed;
                horizontalMotion = 0;
                playerState.State = State.Idle;
                break;
            case "sound_hug":
                dorothyStat.audio_s.clip = dorothyStat._audio[7];
                dorothyStat.sound_play();
                break;
            case "attack_1_move_start":
                moveSpeed = ChangeAttackSpeed(2.5f);
                break;
            case "attack_2_move_start":
                moveSpeed = ChangeAttackSpeed(3f);
                break;
            case "attack_3_move_start":
                moveSpeed = ChangeAttackSpeed(3f);
                break;
            case "smash_1_move_start":
                moveSpeed = ChangeAttackSpeed(9f);
                break;
            case "smash_2_move_start":
                moveSpeed = ChangeAttackSpeed(3f);
                break;
            case "smash_3_move_start":
                moveSpeed = ChangeAttackSpeed(4f);
                break;
            case "jump_attack_end":
                if (playerState.Vertical != Vertical.Grounded) {
                    playerState.State = State.jump;
                    skeletonAnimation.timeScale = aniSpeed;
                    skeletonAnimation.AnimationState.SetAnimation(0, "jump", true);
                }
                break;
            case "jump_landing_end":
                playerState.State = State.Idle;
                playerState.Attack_reserve = Attack_reserve.idle;
                StartCoroutine("AttackendAfter");
                break;
            case "rolling_end":
                StartCoroutine("AttackendAfter");
                transform.gameObject.layer = 8;
                moveSpeed = basisSpeed;
                horizontalMotion = 0;

                break;
            case "guard_break_end":
                StartCoroutine("AttackendAfter");
                transform.gameObject.layer = 8;
                horizontalMotion = 0;
                break;
            case "start_wakeup_end":
                skeletonAnimation.AnimationState.SetAnimation(0, "start_idle", true);
                // 북쪽마녀 이야기 스타트

                Global.balloon_num = 1;

                talkBalloon.talkselect(0);
                break;

            case "die_end":
                Time.timeScale = 1;
                Global.balloon_num = 4;
                break;
        }
    }

    float ChangeAttackSpeed(float defaultSpeed)
    {
        return defaultSpeed * (aniSpeed + Global.weapon[Global.myEquip.Weapon].Speed);
    }

    void CheckGroundEventEnd ()
    {
        horizontalMotion = 0;
        if (playerState.Vertical != Vertical.Grounded)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, "jump", false);
        }
        else
        {
            StopCoroutine("AttackendAfter");
            StartCoroutine("AttackendAfter");
        }
    }

    public IEnumerator AttackendAfter ()
    {
        yield return null;
        playerState.State = State.Idle;
        moveSpeed = basisSpeed;
        PlayAniEndEvent();
    }

    public void PlayAniEndEvent() {
        if(playerState.State == State.Idle) {
            if (moveBool == true) {
                skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
            } else {
                skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
            }
        } else if (playerState.State == State.tired) {
            if (moveBool == true) {
                skeletonAnimation.AnimationState.SetAnimation(0, "tired_walk", true);
            } else {
                skeletonAnimation.AnimationState.SetAnimation(0, "tired_idle", true);
            }
        }
    }
    
    public IEnumerator AttackAfterTime() {
        for (int i = 0; i < 10; i++) {
            yield return null;
        }
        lastAttack = -1;
        if(playerState.State == State.Idle) {
            smashTimingBool = false;
            playerState.Attack_reserve = Attack_reserve.idle;
        }
    }

    //Calls methods that handle physics-based movement
    void FixedUpdate()        //물리적 변화
    {
        WalkMotion();
        JumpMotion();
    }

    //Used to detect player inputs and set parameters & players states for physics behaviour that occurs in FixedUpdate()
	void Update()           // 논리적 변화
	{
        newY = transform.localPosition.y;
        if(movingBlockBool == false) {
            if (newY > oldY) {
                oldY = newY;
            }
        }

        if(floorBool == true) {
            if (playerState.State != State.hug) {
                if (rigid.velocity.y == 0) {  //그라운드 판정 ( y축 힘 0, 공격 모션중)
                    if (playerState.State != State.hurt && playerState.State != State.die) {
                        if (playerState.Vertical != Vertical.Grounded) {
                            playerState.Vertical = Vertical.Grounded;
                            skeletonAnimation.timeScale = aniSpeed;
                            if (standBool == true) {
                                playerState.State = State.Idle;
                            }
                            if (horizontalMotion == 0) {
                                if (standBool == true) {
                                    skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                                }
                            } else {
                                if (playerState.Vertical == Vertical.Grounded) {
                                    skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                                }
                            }
                        }

                        if (movingBlockBool == false) {
                            if (oldY - transform.localPosition.y > 4.5f) {
                                dorothyStat.audio_s.clip = dorothyStat._audio[6];
                                dorothyStat.sound_play();
                                horizontalMotion = 0;
                                skeletonAnimation.AnimationState.SetAnimation(0, "jump_landing", false);
                                playerState.State = State.landing;
                            }

                            oldY = transform.localPosition.y;
                        } else {
                            oldY = transform.localPosition.y + transform.parent.localPosition.y;
                        }
                    }
                }
            }
        }


#if UNITY_EDITOR
        KeyboardInput();
#endif
        if (playerState.State == State.Idle || playerState.State == State.jump || playerState.State == State.tired) {
            if (horizontalMotion != 0) {
                transform.localScale = new Vector3(horizontalMotion * (-1), 1, 1);                       // 좌, 우 방향정하기
                playerState.DirectionFacing = (DirectionFacing)horizontalMotion;         //방향 변수 변화
            }
        }
        
        Horizontal previousMotion = playerState.Horizontal;
        Horizontal currentMotion = playerState.Horizontal = (Horizontal)horizontalMotion;
        
        if ((int)previousMotion * (int)currentMotion == -1)    // 방향전환 될 때 카메라 방향전환
            playerState.Horizontal = Horizontal.Idle;
	}

    public void InMovingBlock() {
        //old_y = transform.localPosition.y;
        movingBlockBool = true;
        oldY = transform.localPosition.y;
    }

    public void OutMovingBlock() {
        movingBlockBool = false;
    }

    void KeyboardInput() {

        if (Input.GetButton("Horizontal")) {
            moveBool = true;
            reserveHor = Input.GetAxisRaw("Horizontal");
        }
        
        switch (playerState.State) {
            case State.Idle:
                // 좌우 방향키(or a,d키)로!!!  좌, 우 멈춤  방향 불러오기 ( -1, 0, 1)  
                horizontalMotion = Input.GetAxisRaw("Horizontal");
                if (Input.GetButtonDown("Horizontal")) {
                    if (horizontalMotion == 0) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                    }
                } else if (Input.GetButton("Horizontal")) {
                    moveBool = true;
                }
                
                if (Input.GetButtonUp("Horizontal")) {
                    moveBool = false;
                    if (horizontalMotion == 0) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                    }
                }
               
                if (Input.GetButtonDown("Jump")) {                      //점프키(스페이스바)가 눌렸을 때 점프 변수 작동
                    jumpActivated = true;
                } else if (Input.GetButtonDown("Attack")) {
                    if(dorothyStat.Sp > 1) {
                        if(playerState.Attack_reserve == Attack_reserve.idle) {
                            //MoveSpeed = 0;
                            Attack(0);
                        } else if(playerState.Attack_reserve == Attack_reserve.wait) {
                            if (lastAttack + 1 <= 2) {
                                Attack(lastAttack + 1);
                            }
                        } else if(playerState.Attack_reserve == Attack_reserve.attack) {
                            //skeletonAnimation.AnimationState.SetAnimation(0, "rolling", false); // 버그탐지용
                        }
                    }
                } else if (Input.GetButtonDown("guard")) {
                    if(dorothyStat.Sp > 0) {
                        Guard();
                    }
                } else if (Input.GetButtonDown("rolling")) {
                    if (dorothyStat.Sp > 0) {
                        Rolling();
                    }
                } else if (Input.GetButtonDown("Smash")) {
                    if (playerState.Attack_reserve == Attack_reserve.wait) {
                        if (lastAttack + 3 <= 5) {
                            Attack(lastAttack + 3);
                        }
                    }
                }

                break;

            case State.jump:
                horizontalMotion = Input.GetAxisRaw("Horizontal");

                if (horizontalMotion != 0) {
                    transform.localScale = new Vector3(horizontalMotion * (-1), 1, 1);                       // 좌, 우 방향정하기
                    playerState.DirectionFacing = (DirectionFacing)horizontalMotion;         //방향 변수 변화
                }

                if (Input.GetButtonDown("Jump")) {                      //점프키(스페이스바)가 눌렸을 때 점프 변수 작동
                    jumpActivated = true;
                } else if (Input.GetButtonDown("Attack")) {
                    if (dorothyStat.Sp > 0) {
                        JumpAttack();
                    }
                }

                break;
            case State.guard:
                if (Input.GetButtonUp("guard")) {
                    GuardEnd();
                }
                break;
            case State.attack:
                if(Input.GetButtonDown("Attack")) {
                    InputKeyAttack();
                } else if (Input.GetButtonDown("Smash")) {
                    InputKeyAttack();
                }

                break;
            case State.tired:
                horizontalMotion = Input.GetAxisRaw("Horizontal");
                if (Input.GetButtonDown("Horizontal")) {
                    if (horizontalMotion == 0) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "tired_idle", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "tired_walk", true);
                    }
                } else if (Input.GetButton("Horizontal")) {
                    moveBool = true;
                }

                if (Input.GetButtonUp("Horizontal")) {
                    moveBool = false;
                    if (horizontalMotion == 0) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "tired_idle", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "tired_walk", true);
                    }
                }
                break;
        }

        if(moveBool == true) {
            if (Input.GetButtonUp("Horizontal")) {
                moveBool = false;
            }
        }
    }

    void InputKeyAttack () {
        if (dorothyStat.Sp > 1  && playerState.Attack_reserve == Attack_reserve.wait  &&  smashTimingBool == true) {
            if (Input.GetButtonDown("Attack")) {
                if(lastAttack < 2) {
                    playerState.Attack_reserve = Attack_reserve.attack;
                    //MoveSpeed = 0;
                }
            } else if(Input.GetButtonDown("Smash")) {
                if(lastAttack < 3) {
                    playerState.Attack_reserve = Attack_reserve.smash;
                    //MoveSpeed = 0;
                }
            }
        }
    }

    public void TouchAttack(bool attack_bool) {
        if (dorothyStat.Sp > 1 && playerState.Attack_reserve == Attack_reserve.wait && smashTimingBool == true) {
            if(attack_bool == true) {
                if (lastAttack < 2) {
                    playerState.Attack_reserve = Attack_reserve.attack;
                    //MoveSpeed = 0;
                }
            } else {
                if (lastAttack < 3) {
                    playerState.Attack_reserve = Attack_reserve.smash;
                    //MoveSpeed = 0;
                }
            }
        }
    }
    
    private void WalkMotion()
    {
        rigid.velocity = new Vector2(horizontalMotion * moveSpeed, rigid.velocity.y);   //좌우 움직임
    }
    
    private void JumpMotion()
    {
        if (jumpActivated)
        {
            if (playerState.Vertical != Vertical.Airborne_high)  //플레이어가 땅에 닿아있을 경우만 점프 발동
            {
                if(playerState.Vertical == Vertical.Grounded) {
                    playerState.Vertical = Vertical.Airborne;
                    rigid.AddForce(new Vector2(0, 8), ForceMode2D.Impulse);
                } else {
                    playerState.Vertical = Vertical.Airborne_high;
                    rigid.velocity = new Vector2(rigid.velocity.x, 0);
                    rigid.AddForce(new Vector2(0, 6.5f), ForceMode2D.Impulse);
                }

                dorothyStat.audio_s.clip = dorothyStat._audio[3];
                dorothyStat.sound_play();

                playerState.State = State.jump;
                skeletonAnimation.AnimationState.SetAnimation(0, "jump", false);
                jumpattackBool = true;
            }
            jumpActivated = false; //점프 상태 false
        }
    }

    public void Jumping() {
        playerState.Attack_reserve = Attack_reserve.idle;
        moveSpeed = basisSpeed;
        horizontalMotion = 0;
        skeletonAnimation.AnimationState.SetAnimation(0, "jump", true);
        if (dorothyStat.Sp == 0) {
            playerState.Vertical = Vertical.Grounded;
            playerState.State = State.Idle;
            rigid.velocity = new Vector2(rigid.velocity.x, 12);
        } else {
            rigid.velocity = new Vector2(rigid.velocity.x, 12);
            playerState.Vertical = Vertical.Airborne;
            playerState.State = State.jump;

            jumpattackBool = true;
        }
    }

    public void MoveAni () {
        if (playerState.State == State.Idle || playerState.State == State.jump) {
            moveBool = true;
            if (playerState.Vertical == Vertical.Grounded) {
                skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
            } else {
                skeletonAnimation.AnimationState.SetAnimation(0, "jump", true);
            }
        } else if(playerState.State == State.tired) {
            moveBool = true;
            if (playerState.Vertical == Vertical.Grounded) {
                skeletonAnimation.AnimationState.SetAnimation(0, "tired_walk", true);
            }
        }
    }

    public void StopAni () {
        if (playerState.Vertical == Vertical.Grounded) {
            horizontalMotion = 0;
            moveBool = false;
            if (playerState.State == State.Idle) {
                skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
            } else if(playerState.State == State.tired) {
                skeletonAnimation.AnimationState.SetAnimation(0, "tired_idle", true);
            }
        } else {
            horizontalMotion = 0;
            moveBool = false;
        }
    }

    public void Attack(int attacknum) {
        StopCoroutine("attack_aftertime");
        playerState.Attack_reserve = Attack_reserve.wait;
        horizontalMotion = reserveHor;
        moveSpeed = 0;

        if (horizontalMotion != 0) {
            transform.localScale = new Vector3(horizontalMotion * (-1), 1, 1);                       // 좌, 우 방향정하기
            playerState.DirectionFacing = (DirectionFacing)horizontalMotion;         //방향 변수 변화
        }
        
        skeletonAnimation.timeScale = aniSpeed + Global.weapon[Global.myEquip.Weapon].Speed;
        attackNumber = attacknum;

        switch (attacknum) {
            case 0:
                skeletonAnimation.AnimationState.SetAnimation(0, "attack_1", false);
                break;
            case 1:
                skeletonAnimation.AnimationState.SetAnimation(0, "attack_2", false);
                break;
            case 2:
                skeletonAnimation.AnimationState.SetAnimation(0, "attack_3", false);
                break;
            case 3:
                skeletonAnimation.AnimationState.SetAnimation(0, "smash_1", false);
                break;
            case 4:
                skeletonAnimation.AnimationState.SetAnimation(0, "smash_2", false);
                break;
            case 5:
                skeletonAnimation.AnimationState.SetAnimation(0, "smash_3", false);
                break;
        }
        playerState.State = State.attack;
        dorothyStat.ChangeAttack(attacknum);

        lastAttack = attacknum;
    }

    public void Guard () {
        dorothyStat.guardBool = true;
        playerState.State = State.guard;
        skeletonAnimation.AnimationState.SetAnimation(0, "guard_ing", true);
        horizontalMotion = 0;
    }

    public void GuardEnd () {
        if(dorothyStat.guardhitBool == false) {
            playerState.State = State.Idle;
            PlayAniEndEvent();
        }
        dorothyStat.guardBool = false;
    }

    public void Rolling() {
        dorothyStat.rolling();
        skeletonAnimation.AnimationState.SetAnimation(0, "rolling", false);
        moveSpeed = 6 * aniSpeed;
        if(horizontalMotion == 0) {
            if (playerState.DirectionFacing == DirectionFacing.Right) {
                horizontalMotion = 1;
            } else {
                horizontalMotion = -1;
            }
        }
    }
    
    public void JumpAttack ()
    {
        if (jumpattackBool == true)
        {
            dorothyStat.ChangeAttack(6);
            skeletonAnimation.timeScale = aniSpeed + 0.25f;
            skeletonAnimation.AnimationState.SetAnimation(0, "jump_attack", false);
            jumpattackBool = false;
            playerState.Vertical = Vertical.Airborne_high;
        }
    }

    public void Hurt ()
    {
        playerState.State = State.hurt;
        horizontalMotion = hurtWay;
        moveSpeed = 1.7f;
        rigid.AddForce(new Vector2(0, 2), ForceMode2D.Impulse);
        smashTimingBool = false;
        playerState.Attack_reserve = Attack_reserve.idle;
        oldY = transform.localPosition.y;
    }

    public void Grab (float grabx, float graby) {
        rigid.velocity = new Vector2(0,0);
        playerState.State = State.hug;
        playerState.Attack_reserve = Attack_reserve.idle;
        transform.localPosition = new Vector3(grabx, graby, -0.3f);
        skeletonAnimation.AnimationState.SetAnimation(0, "hug", true);
        rigid.gravityScale = 0;
        horizontalMotion = 0;
    }
}
