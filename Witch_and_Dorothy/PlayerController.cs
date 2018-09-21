using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;
using GlobalMember;

#region - Script Synopsis
/*
This script is responsible for determining player movement, including walking and jumping.
SpookedCheck() slows down the player when he gets "spooked" by colliding with a Ghost enemy.
*/
#endregion

public class PlayerController : MonoBehaviour
{
    //Reference to player's RigidBody component
    Rigidbody2D Rigid;

    //Used as a mutliplier along with MoveSpeed to create horizontal movement for the player
    public float HorizontalMotion;
    public float reserve_Hor;

    //Used along with HorizontalMotion multiplier to create horizontal movement
    public float MoveSpeed;
    public float basis_speed;
    //Special fields that store player's "spooked" state when colliding with a Ghost enemy, expiring when a timer limit is reached
    public static bool IsSpooked;
    float SpookTimer;
    //public Collider2D sword_collider;
    public GameObject Hp_bar;
    public GameObject Sp_bar;
    public GameObject _talk_balloon;
    public GameObject _camera;

    public AudioClip[] _audio = new AudioClip[10];
    public AudioSource audio_s;


    public GameObject _GM;
    public GameObject _item_manager;

    int soul;

    public SkeletonAnimation skeletonAnimation;
    TrackEntry trackEntry;

    Spine.Skeleton skeleton;

    public bool smash_timingbool;
    public bool move_bool;
    public bool jumpattack_bool;
    public bool JumpActivated;
    public bool invin_bool;
    public bool guard_bool;
    public bool guardhit_bool;
    public bool poison_bool;

    public bool atk_max_bool;
    public bool moving_block_bool;

    public int hurt_way;
    public int last_attack;
    public int attack_number;

    public float old_y, new_y;

    public float Hp, Sp, MaxHp, MaxSp;
    public int basis_ATK;   //나중에  계산  힘 + 무기공격력 + 아티팩트
    public int ATK;         //기본공격력 * 평타 혹은 스매시에 따른 데미지 계수
    public int DEF;
    int[] accessory_num = new int[3];

    public float ani_speed;
    
    bool stand_bool;
    public bool floor_bool;

    int knuckle_atk;

    void Awake() {
        Rigid = GetComponent<Rigidbody2D>();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        audio_s = GetComponent<AudioSource>();

        _camera = GameObject.Find("Main Camera");
        Hp_bar = GameObject.Find("hp_bar");
        Sp_bar = GameObject.Find("sp_bar");
        _GM = GameObject.Find("Gm");
        _item_manager = GameObject.Find("item_inven");

        //test  튜토리얼 본 후
        //PlayerPrefs.SetInt("tutorial", 1);

    }

    void Start()
	{
        //도로시 시작위치
        transform.localPosition = Global.dorothy_next;

        skeleton = GetComponent<SkeletonAnimation>().skeleton;

        way_init();

        smash_timingbool = false;
        move_bool = false;
        new_y = transform.localPosition.y;
        old_y = new_y;
        
        basis_atk_def_init();
        basis_speed_init();
        MoveSpeed = basis_speed;
        skeletonAnimation.timeScale = ani_speed;

        
        poison_bool = false;
        stand_bool = true;  // 튜토리얼에서 idle 고정 해제

        skeletonAnimation.state.Event += OnEvent;

        //PlayerState  함수 참조    상태 변수
        PlayerState.Instance.Horizontal = Horizontal.Idle;
        PlayerState.Instance.Vertical = Vertical.Airborne;
        PlayerState.Instance.DirectionFacing = DirectionFacing.Right;

        PlayerState.Instance.Attack_reserve = Attack_reserve.idle;
            

        if(Global.dorothyData.tutorial == 0) {  //첫 튜토리얼일때
            MaxHp = 50 + Global.dorothyData.Health * 3;
            MaxSp = 20 + Global.dorothyData.Stamina * 1;
            Hp = MaxHp;
            Sp = MaxSp;
            Hp_bar.GetComponent<hp_sp>().get_maxhp(Hp);
            Sp_bar.GetComponent<hp_sp>().get_maxsp(Sp);

            PlayerState.Instance.State = State.landing;
            stand_bool = false;
            Hp -= 50;
            Hp_bar.GetComponent<hp_sp>().hp = Hp;

            StartCoroutine("sleep_end");
            
        } else {
            PlayerState.Instance.State = State.Idle;
            stand_bool = true;
        }
        
        StartCoroutine("init_w_c");   // 옷 무기 초기화
    }

    void way_init () {  //방향 초기화
        reserve_Hor = Global.dorothy_hor;
        transform.localScale = new Vector3(-Global.dorothy_hor, 1, 1);
    }

    //악세 장착
    public void accessory_equip( bool accessory_change ) {
        accessory_num[0] = Global.myEquip.Accessory_0;
        accessory_num[1] = Global.myEquip.Accessory_1;
        accessory_num[2] = Global.myEquip.Accessory_2;

        basis_atk_def_init();
        basis_speed_init();
        mag_size();

        if (Global.dorothyData.tutorial != 0) {
            Hp_bar.transform.parent.gameObject.GetComponent<hp_sp_manager>().trans_bar();
            Sp_bar.transform.parent.gameObject.GetComponent<hp_sp_manager>().trans_bar();
            hp_sp_init(accessory_change);
            StopCoroutine("sp_up");
            StartCoroutine("sp_up");
        }
    }  

    //hp, sp 초기화
    public void hp_sp_init(bool accessory_change) {
        MaxHp = 50 + Global.dorothyData.Health * 3;
        MaxSp = 20 + Global.dorothyData.Stamina * 1;
        
        if (Global.myEquip.Accessory_0 == 8 || Global.myEquip.Accessory_1 == 8 || Global.myEquip.Accessory_2 == 8) {  //활력의 돌
            MaxSp += (5 + Global.myAccessory_int[8]);
        }
        if (Global.myEquip.Accessory_0 == 4 || Global.myEquip.Accessory_1 == 4 || Global.myEquip.Accessory_2 == 4) {  //오크의 혈청
            MaxHp += (20 + Global.myAccessory_int[4]*4);
        }
        
        if (Global.myEquip.Accessory_0 == 10 || Global.myEquip.Accessory_1 == 10 || Global.myEquip.Accessory_2 == 10) {  //양철인간 심장
            MaxSp += (1 + Global.myAccessory_int[10]);
            MaxHp += (4 + Global.myAccessory_int[10]*4);
        }
        

        if (accessory_change == true) {
            if(Hp > MaxHp) {
                Hp = MaxHp;
            }
            if(Sp > MaxSp) {
                Sp = MaxSp;
            }
            //Hp = MaxHp;
            //Sp = MaxSp;

        } else {
            if (Global.map_start_bool == true) {
                Hp = MaxHp;
                Sp = MaxSp;
                Global.map_start_bool = false;
            } else {
                Hp = Global.hp;
                Sp = Global.sp;
            }
        }

        Hp_bar.GetComponent<hp_sp>().get_maxhp(MaxHp);
        Hp_bar.GetComponent<hp_sp>().hp = Hp;
        Hp_bar.GetComponent<hp_sp>().trans_hp_bar();

        Sp_bar.GetComponent<hp_sp>().get_maxsp(MaxSp);
        Sp_bar.GetComponent<hp_sp>().sp = Sp;
        Sp_bar.GetComponent<hp_sp>().trans_sp_bar();
        
    }
    
    //스텟 변화시 작동코드
    public void basis_atk_def_init() {
        basis_ATK = (int)(Global.dorothyData.Str * 1) + Global.myWeapon_int[Global.myEquip.Weapon] * 2;   // 기본 공격력 =  힘 + 무기 강화수치 공격력
        DEF = Global.clothes[Global.myEquip.Clothes].Def + Global.myClothes_int[Global.myEquip.Clothes] * 2;

        //악세 추가 능력
        if (Global.myEquip.Accessory_0 == 0 || Global.myEquip.Accessory_1 == 0 || Global.myEquip.Accessory_2 == 0) { //마녀의 정수
            basis_ATK += (7 + Global.myAccessory_int[0]);
        }

        if(Global.myEquip.Accessory_0 == 11 || Global.myEquip.Accessory_1 == 11 || Global.myEquip.Accessory_2 == 11) { //사자의 용기 공격력증가
            basis_ATK += (1 + Global.myAccessory_int[11]);
        }
    }

    public void basis_speed_init () {
        ani_speed = 1.1f + (Global.dorothyData.Dex + Global.clothes[Global.myEquip.Clothes].Dex)* 0.01f;  // 속도  =  1.2 + 민첩 *0.01f
        
        if (Global.myEquip.Accessory_0 == 11 || Global.myEquip.Accessory_1 == 11 || Global.myEquip.Accessory_2 == 11) { //사자의 용기 이속증가
            ani_speed += (0.01f + Global.myAccessory_int[11]*0.01f);  //민첩 1증가
        }
        
        basis_speed = 3f + ani_speed;

        if (Global.myEquip.Accessory_0 == 6 || Global.myEquip.Accessory_1 == 6 || Global.myEquip.Accessory_2 == 6) { //하피의 깃털  이속증가
            basis_speed += (0.1f + Global.myAccessory_int[6]*0.02f); 
        }
        
        //악세 추가 능력
        if (Global.myEquip.Accessory_0 == 1 || Global.myEquip.Accessory_1 == 1 || Global.myEquip.Accessory_2 == 1) { //실프의 장갑 공속증가
            ani_speed += (0.1f + Global.myAccessory_int[1] * 0.02f); 
        }

        MoveSpeed = basis_speed;
    }

    void mag_size() {
        if (Global.myEquip.Accessory_0 == 5 || Global.myEquip.Accessory_1 == 5 || Global.myEquip.Accessory_2 == 5) { //정수가 든 병
            transform.GetChild(3).gameObject.GetComponent<Dorothy_mag>().circle.radius = 2 + Global.myAccessory_int[5]*0.14f;
        } else {
            transform.GetChild(3).gameObject.GetComponent<Dorothy_mag>().circle.radius = 1.3f;
        }
    }

    int atk_ran() {
        int atk_ran_temp = Random.Range(Global.weapon[Global.myEquip.Weapon].Min, Global.weapon[Global.myEquip.Weapon].Max + 1);

        if(atk_ran_temp >= Global.weapon[Global.myEquip.Weapon].Atk) {
            atk_max_bool = true;
        } else {
            atk_max_bool = false;
        }

        return atk_ran_temp;
    }
    
    void ani_speed_init() {
        skeletonAnimation.timeScale = ani_speed;
    }

    public IEnumerator init_w_c () {
        yield return YieldInstructionCache.WaitForSeconds(0.01f);
        init_weapon_clothes();
        accessory_equip(false);
    }

    public IEnumerator sleep_end() {
        yield return YieldInstructionCache.WaitForSeconds(0.01f);
        Hp_bar.GetComponent<hp_sp>().trans_hp_bar();
        yield return YieldInstructionCache.WaitForSeconds(2);
        skeletonAnimation.AnimationState.SetAnimation(0, "start_wakeup", false);
    }
    
    private void OnEvent ( TrackEntry trackEntry, Spine.Event e ) {     //모션이 끝난 후 이벤트
        
        switch(e.Data.name) {
            case "move_end":
                MoveSpeed = basis_speed;
                HorizontalMotion = 0;
                break;
            case "attack_1_end":
                ani_speed_init();
                switch(PlayerState.Instance.Attack_reserve) {
                    case Attack_reserve.attack:

                        break;
                    case Attack_reserve.smash:

                        break;
                    default:
                        end_event_nocommend();
                        StartCoroutine("attack_aftertime");
                        break;
                }
                break;
            case "attack_2_end":
                ani_speed_init();
                if (PlayerState.Instance.Attack_reserve == Attack_reserve.attack) {
                    attack(2);
                    smash_timingbool = false;
                } else if (PlayerState.Instance.Attack_reserve == Attack_reserve.smash) {
                    attack(4);
                    smash_timingbool = false;
                } else {
                    end_event_nocommend();
                    StartCoroutine("attack_aftertime");
                }
                break;
            case "attack_3_end":
                ani_speed_init();
                if (PlayerState.Instance.Attack_reserve == Attack_reserve.smash) {
                    attack(5);
                    smash_timingbool = false;
                } else {
                    end_event_nocommend();
                    //StartCoroutine("attack_aftertime");
                }
                break;
            case "smash_end":
                ani_speed_init();
                end_event_nocommend();
                PlayerState.Instance.Attack_reserve = Attack_reserve.idle;
                break;
            case "smash_timing":
                smash_timingbool = true;
                break;
            case "hurt_end":
                MoveSpeed = basis_speed;
                HorizontalMotion = 0;
                if (PlayerState.Instance.Vertical == Vertical.Grounded ) {
                    PlayerState.Instance.State = State.Idle;
                    if (move_bool == true) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                    }
                } else {
                    PlayerState.Instance.State = State.jump;
                    skeletonAnimation.AnimationState.SetAnimation(0, "jump", true);
                }
                break;
            case "hug_end":
                if (move_bool == true) {
                    skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                } else {
                    skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                }
                MoveSpeed = basis_speed;
                HorizontalMotion = 0;
                PlayerState.Instance.State = State.Idle;
                break;
            case "sound_hug":
                audio_s.clip = _audio[7];
                sound_play();
                break;
            case "attack_1_move_start":
                MoveSpeed = 2.5f * (ani_speed + Global.weapon[Global.myEquip.Weapon].Speed);
                break;
            case "attack_2_move_start":
                MoveSpeed = 3f * (ani_speed + Global.weapon[Global.myEquip.Weapon].Speed);
                break;
            case "attack_3_move_start":
                MoveSpeed = 3f * (ani_speed + Global.weapon[Global.myEquip.Weapon].Speed);
                break;
            case "smash_1_move_start":
                MoveSpeed = 9f * (ani_speed + Global.weapon[Global.myEquip.Weapon].Speed);
                break;
            case "smash_2_move_start":
                MoveSpeed = 3f * (ani_speed + Global.weapon[Global.myEquip.Weapon].Speed);
                break;
            case "smash_3_move_start":
                MoveSpeed = 4f * (ani_speed + Global.weapon[Global.myEquip.Weapon].Speed);
                break;
            case "jump_attack_end":
                if (PlayerState.Instance.Vertical != Vertical.Grounded) {
                    PlayerState.Instance.State = State.jump;
                    skeletonAnimation.timeScale = ani_speed;
                    skeletonAnimation.AnimationState.SetAnimation(0, "jump", true);
                }
                break;
            case "jump_landing_end":
                PlayerState.Instance.State = State.Idle;
                PlayerState.Instance.Attack_reserve = Attack_reserve.idle;
                StartCoroutine("attackend_after");
                break;
            case "rolling_end":
                StartCoroutine("attackend_after");
                transform.gameObject.layer = 8;
                MoveSpeed = basis_speed;
                HorizontalMotion = 0;

                break;
            case "guard_break_end":
                StartCoroutine("attackend_after");
                transform.gameObject.layer = 8;
                HorizontalMotion = 0;
                break;
            case "start_wakeup_end":
                skeletonAnimation.AnimationState.SetAnimation(0, "start_idle", true);
                // 북쪽마녀 이야기 스타트

                Global.balloon_num = 1;

                _talk_balloon.GetComponent<talk_balloon>().talkselect(0);
                break;

            case "die_end":
                Time.timeScale = 1;

                //죽은 후 일어나는 이벤트 넣는곳 !!!
                Global.balloon_num = 4;
                break;
        }
    }

    public void talk_end() {
        //마녀의 이야기 후 해줄일
        PlayerState.Instance.State = State.Idle;
        StartCoroutine("sp_up");
        stand_bool = true;
    }

    
    void end_event_ani() {
        if(PlayerState.Instance.State == State.Idle) {
            if (move_bool == true) {
                skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
            } else {
                skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
            }
        } else if (PlayerState.Instance.State == State.tired) {
            if (move_bool == true) {
                skeletonAnimation.AnimationState.SetAnimation(0, "tired_walk", true);
            } else {
                skeletonAnimation.AnimationState.SetAnimation(0, "tired_idle", true);
            }
        }
    }

    void end_event_nocommend() {
        HorizontalMotion = 0;
        if (PlayerState.Instance.Vertical != Vertical.Grounded) {
            skeletonAnimation.AnimationState.SetAnimation(0, "jump", false);
        } else {
            StopCoroutine("attackend_after");
            StartCoroutine("attackend_after");
        }
    }

    public IEnumerator attackend_after() {
        yield return null;
        PlayerState.Instance.State = State.Idle;
        MoveSpeed = basis_speed;
        end_event_ani();
    }

    public IEnumerator attack_aftertime() {
        for (int i = 0; i < 10; i++) {
            yield return null;
        }
        last_attack = -1;
        if(PlayerState.Instance.State == State.Idle) {
            smash_timingbool = false;
            PlayerState.Instance.Attack_reserve = Attack_reserve.idle;
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
        new_y = transform.localPosition.y;
        if(moving_block_bool == false) {
            if (new_y > old_y) {
                old_y = new_y;
            }
        }

        if(floor_bool == true) {
            if (PlayerState.Instance.State != State.hug) {
                if (Rigid.velocity.y == 0) {  //그라운드 판정 ( y축 힘 0, 공격 모션중)


                    if (PlayerState.Instance.State != State.hurt && PlayerState.Instance.State != State.die) {
                        if (PlayerState.Instance.Vertical != Vertical.Grounded) {
                            PlayerState.Instance.Vertical = Vertical.Grounded;
                            skeletonAnimation.timeScale = ani_speed;
                            if (stand_bool == true) {
                                PlayerState.Instance.State = State.Idle;
                            }
                            if (HorizontalMotion == 0) {
                                if (stand_bool == true) {
                                    skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                                }
                            } else {
                                if (PlayerState.Instance.Vertical == Vertical.Grounded) {
                                    skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                                }
                            }
                        }

                        if (moving_block_bool == false) {
                            if (old_y - transform.localPosition.y > 4.5f) {
                                audio_s.clip = _audio[6];
                                sound_play();
                                HorizontalMotion = 0;
                                skeletonAnimation.AnimationState.SetAnimation(0, "jump_landing", false);
                                PlayerState.Instance.State = State.landing;
                            }

                            old_y = transform.localPosition.y;
                        } else {
                            old_y = transform.localPosition.y + transform.parent.localPosition.y;
                        }
                    }
                }
            }
        }


#if UNITY_EDITOR
        keyboardinput();
#endif
        if (PlayerState.Instance.State == State.Idle || PlayerState.Instance.State == State.jump || PlayerState.Instance.State == State.tired) {
            if (HorizontalMotion != 0) {
                transform.localScale = new Vector3(HorizontalMotion * (-1), 1, 1);                       // 좌, 우 방향정하기
                PlayerState.Instance.DirectionFacing = (DirectionFacing)HorizontalMotion;         //방향 변수 변화
            }
        }
        
        Horizontal previousMotion = PlayerState.Instance.Horizontal;
        Horizontal currentMotion = PlayerState.Instance.Horizontal = (Horizontal)HorizontalMotion;

        //Fixes an error with the camera following the player incorrectly if quickly changing direction while at the furthest possible positions at each side of the screen.
        if ((int)previousMotion * (int)currentMotion == -1)    // 방향전환 될 때 카메라 방향전환
            PlayerState.Instance.Horizontal = Horizontal.Idle;
	}

    public void moving_block() {
        //old_y = transform.localPosition.y;
        moving_block_bool = true;
        old_y = transform.localPosition.y;
    }

    public void moving_block_out() {
        moving_block_bool = false;
    }

    void keyboardinput() {

        if (Input.GetButton("Horizontal")) {
            move_bool = true;
            reserve_Hor = Input.GetAxisRaw("Horizontal");
        }
        
        switch (PlayerState.Instance.State) {
            case State.Idle:
                // 좌우 방향키(or a,d키)로!!!  좌, 우 멈춤  방향 불러오기 ( -1, 0, 1)  
                HorizontalMotion = Input.GetAxisRaw("Horizontal");
                if (Input.GetButtonDown("Horizontal")) {
                    if (HorizontalMotion == 0) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                    }
                } else if (Input.GetButton("Horizontal")) {
                    move_bool = true;
                }
                
                if (Input.GetButtonUp("Horizontal")) {
                    move_bool = false;
                    if (HorizontalMotion == 0) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                    }
                }
               
                if (Input.GetButtonDown("Jump")) {                      //점프키(스페이스바)가 눌렸을 때 점프 변수 작동
                    JumpActivated = true;
                } else if (Input.GetButtonDown("Attack")) {
                    if(Sp > 1) {
                        if(PlayerState.Instance.Attack_reserve == Attack_reserve.idle) {
                            //MoveSpeed = 0;
                            attack(0);
                        } else if(PlayerState.Instance.Attack_reserve == Attack_reserve.wait) {
                            if (last_attack + 1 <= 2) {
                                attack(last_attack + 1);
                            }
                        } else if(PlayerState.Instance.Attack_reserve == Attack_reserve.attack) {
                            //skeletonAnimation.AnimationState.SetAnimation(0, "rolling", false); // 버그탐지용
                        }
                    }
                } else if (Input.GetButtonDown("guard")) {
                    if(Sp > 0) {
                        guard();
                    }
                } else if (Input.GetButtonDown("rolling")) {
                    if (Sp > 0) {
                        rolling();
                    }
                } else if (Input.GetButtonDown("Smash")) {
                    if (PlayerState.Instance.Attack_reserve == Attack_reserve.wait) {
                        if (last_attack + 3 <= 5) {
                            attack(last_attack + 3);
                        }
                    }
                }
                /*
                if (HorizontalMotion != 0) {
                    transform.localScale = new Vector3(HorizontalMotion * (-1), 1, 1);                       // 좌, 우 방향정하기
                    PlayerState.Instance.DirectionFacing = (DirectionFacing)HorizontalMotion;         //방향 변수 변화
                }
                break;
                */
                break;

            case State.jump:
                HorizontalMotion = Input.GetAxisRaw("Horizontal");

                if (HorizontalMotion != 0) {
                    transform.localScale = new Vector3(HorizontalMotion * (-1), 1, 1);                       // 좌, 우 방향정하기
                    PlayerState.Instance.DirectionFacing = (DirectionFacing)HorizontalMotion;         //방향 변수 변화
                }

                if (Input.GetButtonDown("Jump")) {                      //점프키(스페이스바)가 눌렸을 때 점프 변수 작동
                    JumpActivated = true;
                } else if (Input.GetButtonDown("Attack")) {
                    if (Sp > 0) {
                        jump_attack();
                    }
                }

                break;
            case State.guard:
                if (Input.GetButtonUp("guard")) {
                    guard_end();
                }
                break;
            case State.attack:
                if(Input.GetButtonDown("Attack")) {
                    keyinput_attack();
                } else if (Input.GetButtonDown("Smash")) {
                    keyinput_attack();
                }

                break;
            case State.tired:
                HorizontalMotion = Input.GetAxisRaw("Horizontal");
                if (Input.GetButtonDown("Horizontal")) {
                    if (HorizontalMotion == 0) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "tired_idle", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "tired_walk", true);
                    }
                } else if (Input.GetButton("Horizontal")) {
                    move_bool = true;
                }

                if (Input.GetButtonUp("Horizontal")) {
                    move_bool = false;
                    if (HorizontalMotion == 0) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "tired_idle", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "tired_walk", true);
                    }
                }
                break;
        }

        if(move_bool == true) {
            if (Input.GetButtonUp("Horizontal")) {
                move_bool = false;
            }
        }
    }

    void keyinput_attack () {
        if (Sp > 1  &&  PlayerState.Instance.Attack_reserve == Attack_reserve.wait  &&  smash_timingbool == true) {
            if (Input.GetButtonDown("Attack")) {
                if(last_attack < 2) {
                    PlayerState.Instance.Attack_reserve = Attack_reserve.attack;
                    //MoveSpeed = 0;
                }
            } else if(Input.GetButtonDown("Smash")) {
                if(last_attack < 3) {
                    PlayerState.Instance.Attack_reserve = Attack_reserve.smash;
                    //MoveSpeed = 0;
                }
            }
        }
    }

    public void touch_attack(bool attack_bool) {
        if (Sp > 1 && PlayerState.Instance.Attack_reserve == Attack_reserve.wait && smash_timingbool == true) {
            if(attack_bool == true) {
                if (last_attack < 2) {
                    PlayerState.Instance.Attack_reserve = Attack_reserve.attack;
                    //MoveSpeed = 0;
                }
            } else {
                if (last_attack < 3) {
                    PlayerState.Instance.Attack_reserve = Attack_reserve.smash;
                    //MoveSpeed = 0;
                }
            }
        }
    }

    //Handles basic horizontal movement using physics-based velocity, called in FixedUpdate()
    private void WalkMotion()
    {
        Rigid.velocity = new Vector2(HorizontalMotion * MoveSpeed, Rigid.velocity.y);   //좌우 움직임
    }

    //Handles player's vertical state and allows jumping only when grounded, using physics-based AddForce(), called in FixedUpdate()
    private void JumpMotion()
    {
        if (JumpActivated)
        {
            if (PlayerState.Instance.Vertical != Vertical.Airborne_high)  //플레이어가 땅에 닿아있을 경우만 점프 발동
            {
                if(PlayerState.Instance.Vertical == Vertical.Grounded) {
                    PlayerState.Instance.Vertical = Vertical.Airborne;
                    Rigid.AddForce(new Vector2(0, 8), ForceMode2D.Impulse);  // 점프 (y축 방향으로 6만큼 힘을줌)
                } else {
                    PlayerState.Instance.Vertical = Vertical.Airborne_high;
                    Rigid.velocity = new Vector2(Rigid.velocity.x, 0);
                    Rigid.AddForce(new Vector2(0, 6.5f), ForceMode2D.Impulse);  // 점프 (y축 방향으로 6만큼 힘을줌)
                }

                audio_s.clip = _audio[3];
                sound_play();

                PlayerState.Instance.State = State.jump;
                //GetComponent<AudioSource>().Play();  // 점프 사운드재생
                skeletonAnimation.AnimationState.SetAnimation(0, "jump", false);
                jumpattack_bool = true;
            }
            JumpActivated = false; //점프 상태 false
        }
    }

    void OnTriggerEnter2D ( Collider2D other ) {
        switch(other.name) {
            case "air":
                oneshot_death();
                break;
            case "oz_talk_col":
                //Global.dorothyData.epic_talk_num = 5;

                if (Global.dorothyData.epic_talk_num == 5) {
                    Global.balloon_num = 1;

                    StartCoroutine("oz_talk_go");
                } else {
                    _talk_balloon.transform.GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<text_reader>().boss_go();
                }
                other.gameObject.SetActive(false);
                other.transform.parent.GetChild(5).gameObject.SetActive(true);

                break;
            case "oz_talk_col_2":
                if(other.transform.localPosition.x > transform.localPosition.x) {
                    transform.localScale = new Vector3(1, 1, 1);
                } else {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
                
                Global.balloon_num = 1;

                StartCoroutine("oz_talk_2_go");
                other.gameObject.SetActive(false);
                break;
        }

        /*
        if (other.name == "general_see_col") {
            if (other.transform.parent.gameObject.GetComponent<Enemy_AI>().Enemy_state == "idle") {
                other.transform.parent.gameObject.GetComponent<Enemy_AI>().ad();
            }
        }
        */
    }

    public IEnumerator oz_talk_go () {
        _talk_balloon.GetComponent<talk_balloon>().talk_epic(9);
        yield return null;
        stop();
        _talk_balloon.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<text_reader>().go();
    }

    public IEnumerator oz_talk_2_go() {
        _talk_balloon.GetComponent<talk_balloon>().talk_epic(10);
        yield return null;
        stop();
        _talk_balloon.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<text_reader>().go();
    }



    /*
    private void OnCollisionStay2D ( Collision2D  other) {
        if(other.transform.gameObject.layer == 20) {
            if (PlayerState.Instance.State != State.hug) {
                if (Rigid.velocity.y == 0) {  //그라운드 판정 ( y축 힘 0, 공격 모션중)
                    

                    if (PlayerState.Instance.State != State.hurt && PlayerState.Instance.State != State.die) {
                        if (PlayerState.Instance.Vertical != Vertical.Grounded) {
                            PlayerState.Instance.Vertical = Vertical.Grounded;
                            skeletonAnimation.timeScale = ani_speed;
                            if (stand_bool == true) {
                                PlayerState.Instance.State = State.Idle;
                            }
                            if (HorizontalMotion == 0) {
                                if (stand_bool == true) {
                                    skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                                }
                            } else {
                                if (PlayerState.Instance.Vertical == Vertical.Grounded) {
                                    skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                                }
                            }
                        }

                        if (moving_block_bool == false) {
                            if (old_y - transform.localPosition.y > 3f) {
                                audio_s.clip = _audio[6];
                                sound_play();
                                HorizontalMotion = 0;
                                skeletonAnimation.AnimationState.SetAnimation(0, "jump_landing", false);
                                PlayerState.Instance.State = State.landing;
                            }

                            old_y = transform.localPosition.y;
                        } else {
                            old_y = transform.localPosition.y + transform.parent.localPosition.y;
                            //Debug.Log("도로시 y :" + transform.localPosition.y + "엘레베이터 y :" + transform.parent.localPosition.y + "=" + old_y);
                        }
                    }
                }
            }
        }
        
    }
    */
    
    public void jumping() {
        PlayerState.Instance.Attack_reserve = Attack_reserve.idle;
        MoveSpeed = basis_speed;
        HorizontalMotion = 0;
        skeletonAnimation.AnimationState.SetAnimation(0, "jump", true);
        if (Sp == 0) {
            PlayerState.Instance.Vertical = Vertical.Grounded;
            PlayerState.Instance.State = State.Idle;
            Rigid.velocity = new Vector2(Rigid.velocity.x, 12);
        } else {
            Rigid.velocity = new Vector2(Rigid.velocity.x, 12);
            PlayerState.Instance.Vertical = Vertical.Airborne;
            PlayerState.Instance.State = State.jump;

            jumpattack_bool = true;
        }
    }
    
    /*
    //수정중  바람으로 힘가하기
    void OnTriggerStay2D(Collider2D other) {
        if(other.name == "wind") {
            if(other.transform.localRotation.y == 0) {
                Rigid.AddForce(new Vector2(-100, 0), ForceMode2D.Force);
            } else {
                Rigid.AddForce(new Vector2(100, 0), ForceMode2D.Force);
            }
        }
    }
    */

    public void poison () {
        poison_bool = true;
        StartCoroutine("poison_ing");
    }

    public void poisonland() {
        Hp -= 40;

        if (Hp <= 0) {
            Hp = 0;
            skeletonAnimation.AnimationState.SetAnimation(0, "die", false);
            HorizontalMotion = 0;
            PlayerState.Instance.State = State.die;
            Time.timeScale = 0.3f;
        }

        Hp_bar.GetComponent<hp_sp>().hp = Hp;
        Hp_bar.GetComponent<hp_sp>().trans_hp_bar();
    }

    public IEnumerator poison_ing() {
        for(int i = 0; i < 6; i++) {
            if (i % 2 == 0) {
                skeleton.r = 170 / 255f;
                skeleton.g = 50 / 255f;
                skeleton.b = 210 / 255f;
                Hp -= 30;

                if (Hp <= 0) {
                    Hp = 0;
                    skeletonAnimation.AnimationState.SetAnimation(0, "die", false);
                    HorizontalMotion = 0;
                    PlayerState.Instance.State = State.die;
                    Time.timeScale = 0.3f;
                }

                Hp_bar.GetComponent<hp_sp>().hp = Hp;
                Hp_bar.GetComponent<hp_sp>().trans_hp_bar();
            } else {
                skeleton.r = 1;
                skeleton.b = 1;
                skeleton.g = 1;
            }
            yield return YieldInstructionCache.WaitForSeconds(0.5f);
        }
        poison_bool = false;
    }
    
    public void move () {
        if (PlayerState.Instance.State == State.Idle || PlayerState.Instance.State == State.jump) {
            move_bool = true;
            if (PlayerState.Instance.Vertical == Vertical.Grounded) {
                skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
            } else {
                skeletonAnimation.AnimationState.SetAnimation(0, "jump", true);
            }
        } else if(PlayerState.Instance.State == State.tired) {
            move_bool = true;
            if (PlayerState.Instance.Vertical == Vertical.Grounded) {
                skeletonAnimation.AnimationState.SetAnimation(0, "tired_walk", true);
            }
        }
    }

    public void stop () {
        if (PlayerState.Instance.Vertical == Vertical.Grounded) {
            HorizontalMotion = 0;
            move_bool = false;
            if (PlayerState.Instance.State == State.Idle) {
                skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
            } else if(PlayerState.Instance.State == State.tired) {
                skeletonAnimation.AnimationState.SetAnimation(0, "tired_idle", true);
            }
        } else {
            HorizontalMotion = 0;
            move_bool = false;
        }
    }

    public void attack(int attacknum) {
        StopCoroutine("attack_aftertime");
        PlayerState.Instance.Attack_reserve = Attack_reserve.wait;
        HorizontalMotion = reserve_Hor;
        MoveSpeed = 0;

        if (HorizontalMotion != 0) {
            transform.localScale = new Vector3(HorizontalMotion * (-1), 1, 1);                       // 좌, 우 방향정하기
            PlayerState.Instance.DirectionFacing = (DirectionFacing)HorizontalMotion;         //방향 변수 변화
        }

        
        if(Global.myEquip.Accessory_0 == 3 || Global.myEquip.Accessory_0 == 3 || Global.myEquip.Accessory_0 == 3) {
            if(MaxHp / 10 * 3  > Hp) {
                knuckle_atk = 10 + Global.myAccessory_int[3]*2;
            }
        } else {
            knuckle_atk = 0;
        }

        skeletonAnimation.timeScale = ani_speed + Global.weapon[Global.myEquip.Weapon].Speed;
        //Debug.Log(ani_speed + Global.weapon[Global.myEquip.Weapon].Speed);
        attack_number = attacknum;
        switch (attacknum) {
            case 0:
                PlayerState.Instance.State = State.attack;
                skeletonAnimation.AnimationState.SetAnimation(0, "attack_1", false);
                Sp -= 2;
                ATK = (int)( (basis_ATK + atk_ran() + knuckle_atk) * 0.6f);
                audio_s.clip = _audio[1];
                break;
            case 1:
                PlayerState.Instance.State = State.attack;
                skeletonAnimation.AnimationState.SetAnimation(0, "attack_2", false);
                Sp -= 2;
                ATK = (int)((basis_ATK + atk_ran() + knuckle_atk) * 0.8f);
                audio_s.clip = _audio[1];
                break;
            case 2:
                PlayerState.Instance.State = State.attack;
                skeletonAnimation.AnimationState.SetAnimation(0, "attack_3", false);
                Sp -= 2;
                ATK = (int)((basis_ATK + atk_ran() + knuckle_atk) * 1);
                audio_s.clip = _audio[1];
                break;
            case 3:
                PlayerState.Instance.State = State.attack;
                skeletonAnimation.AnimationState.SetAnimation(0, "smash_1", false);
                Sp -= 5;
                ATK = (int)((basis_ATK + atk_ran() + knuckle_atk) * 2f);
                audio_s.clip = _audio[2];
                break;
            case 4:
                PlayerState.Instance.State = State.attack;
                skeletonAnimation.AnimationState.SetAnimation(0, "smash_2", false);
                Sp -= 6;
                ATK = (int)((basis_ATK + atk_ran() + knuckle_atk) * 3f);
                audio_s.clip = _audio[2];
                break;
            case 5:
                PlayerState.Instance.State = State.attack;
                skeletonAnimation.AnimationState.SetAnimation(0, "smash_3", false);
                Sp -= 7;
                ATK = (int)((basis_ATK + atk_ran() + knuckle_atk) * 5f);
                audio_s.clip = _audio[2];
                break;
        }

        //Debug.Log(ATK);

        sound_play();

        if (Sp <= 0) {
            Sp = 0;
        }
        Sp_bar.GetComponent<hp_sp>().sp = Sp;
        Sp_bar.GetComponent<hp_sp>().trans_sp_bar();

        last_attack = attacknum;
    }

    public void guard () {
        guard_bool = true;
        PlayerState.Instance.State = State.guard;
        skeletonAnimation.AnimationState.SetAnimation(0, "guard_ing", true);
        HorizontalMotion = 0;
    }

    public void guard_end () {
        if(guardhit_bool == false) {
            PlayerState.Instance.State = State.Idle;
            end_event_ani();
        }
        guard_bool = false;
    }

    public IEnumerator guard_hit_move () {
        yield return YieldInstructionCache.WaitForSeconds(0.3f);
        HorizontalMotion = 0;
        guardhit_bool = false;
        if (guard_bool == false) {
            PlayerState.Instance.State = State.Idle;
            end_event_ani();
        }
        MoveSpeed = basis_speed;
    }

    public void guard_hit (float speed) {
        audio_s.clip = _audio[4];
        sound_play();

        HorizontalMotion = hurt_way;
        MoveSpeed = speed;

        Sp -= 4;
        if (Sp <= 0) {
            Sp = 0;
            skeletonAnimation.AnimationState.SetAnimation(0, "guard_break", false);
            audio_s.clip = _audio[10];
            sound_play();
            //skeletonAnimation.AnimationState.TimeScale = 1f;
            PlayerState.Instance.State = State.guard_break;
            transform.gameObject.layer = 12;
            MoveSpeed = basis_speed;
            guard_bool = false;
        } 
        Sp_bar.GetComponent<hp_sp>().sp = Sp;
        Sp_bar.GetComponent<hp_sp>().trans_sp_bar();
        guardhit_bool = true;
            
        StopCoroutine("guard_hit_move");
        StartCoroutine("guard_hit_move");
    }

    public void heavy_guard_hit (int spdown) {
        HorizontalMotion = hurt_way;
        MoveSpeed = 1.7f;

        Sp -= spdown;
        if(Sp <= 0) {
            Sp = 0;
            guard_bool = false;
        }
        audio_s.clip = _audio[10];
        sound_play();
        skeletonAnimation.AnimationState.SetAnimation(0, "guard_break", false);
        //skeletonAnimation.AnimationState.TimeScale = 1f;
        PlayerState.Instance.State = State.guard_break;
        transform.gameObject.layer = 12;
        MoveSpeed = 5f;
        
        Sp_bar.GetComponent<hp_sp>().sp = Sp;
        Sp_bar.GetComponent<hp_sp>().trans_sp_bar();
        guardhit_bool = true;

        StopCoroutine("guard_hit_move");
        StartCoroutine("guard_hit_move");
    }

    public void rolling() {
        audio_s.clip = _audio[5];
        sound_play();

        PlayerState.Instance.State = State.rolling;
        transform.gameObject.layer = 12;
        Sp -= 6;
        if (Sp <= 0) {
            Sp = 0;
        }
        Sp_bar.GetComponent<hp_sp>().sp = Sp;
        Sp_bar.GetComponent<hp_sp>().trans_sp_bar();
        skeletonAnimation.AnimationState.SetAnimation(0, "rolling", false);
        MoveSpeed = 6 * ani_speed;
        if(HorizontalMotion == 0) {
            if (PlayerState.Instance.DirectionFacing == DirectionFacing.Right) {
                HorizontalMotion = 1;
            } else {
                HorizontalMotion = -1;
            }
        }
    }

    public void hurt (int Damage, bool guard_ignore) {
        PlayerState.Instance.State = State.hurt;
        HorizontalMotion = hurt_way;
        MoveSpeed = 1.7f;
        Rigid.AddForce(new Vector2(0, 2), ForceMode2D.Impulse);
        
        _camera.GetComponent<Camera_init>().camera_shake();   //카메라 흔들림
        
        smash_timingbool = false;
        PlayerState.Instance.Attack_reserve = Attack_reserve.idle;
        

        if(guard_ignore == true) {
            Hp -= Damage;
        } else {
            if(Damage != 0) {
                int temp_dmg = Damage - (int)(DEF * 0.5f);
                temp_dmg = temp_dmg - (int)(temp_dmg * DEF * 0.01f);

                //int temp_dmg = Damage - (int)(Damage * DEF * 0.01f);
                //temp_dmg -= (int)(DEF * 0.5f);

                if (temp_dmg > 0) {
                    Hp -= temp_dmg;
                } else {
                    Hp -= 1;
                }
            } 
            /*
            if (Damage - DEF <= 0) {
                Hp -= 1;
            } else {
                Hp -= (Damage - DEF);
            }
            */
        }
        
        if (Hp <= 0) {
            audio_s.clip = _audio[8];
            StopAllCoroutines();
            Hp = 0;
            skeletonAnimation.AnimationState.SetAnimation(0, "die", false);
            HorizontalMotion = 0;
            PlayerState.Instance.State = State.die;
            
            Time.timeScale = 0.3f;
        } else {
            audio_s.clip = _audio[7];
            skeletonAnimation.AnimationState.SetAnimation(0, "hurt", false);
            StartCoroutine("invincibility");
        }

        sound_play();
        old_y = transform.localPosition.y;

        Hp_bar.GetComponent<hp_sp>().hp = Hp;
        Hp_bar.GetComponent<hp_sp>().trans_hp_bar();
    }

    public void grab (float grabx, float graby) {
        audio_s.clip = _audio[9];
        sound_play();

        Rigid.velocity = new Vector2(0,0);
        PlayerState.Instance.State = State.hug;
        PlayerState.Instance.Attack_reserve = Attack_reserve.idle;
        transform.localPosition = new Vector3(grabx, graby, -0.3f);
        skeletonAnimation.AnimationState.SetAnimation(0, "hug", true);
        Rigid.gravityScale = 0;
        HorizontalMotion = 0;
        StartCoroutine("hug_ing");
    }

    public IEnumerator hug_ing () {
        StartCoroutine("invincibility");
        for (int i = 0; i<45; i++) {
           
            if (i % 10 == 1) {
                Hp -= 10;
            }

            if (Hp <= 0) {
                Hp = 0;
                skeletonAnimation.AnimationState.SetAnimation(0, "die", false);
                HorizontalMotion = 0;
                Rigid.gravityScale = 2;
                PlayerState.Instance.State = State.die;
                Time.timeScale = 0.3f;
            }

            Hp_bar.GetComponent<hp_sp>().hp = Hp;
            Hp_bar.GetComponent<hp_sp>().trans_hp_bar();

            yield return null;
        }


        if (Hp != 0) {
            HorizontalMotion = -reserve_Hor;
            MoveSpeed = 3f;
            Rigid.gravityScale = 2;
            skeletonAnimation.AnimationState.SetAnimation(0, "hug_end", false);
            yield return null;
        }

    }


    public void jump_attack () {
        if (jumpattack_bool == true) {
            ATK = (int)((basis_ATK + atk_ran()) * 1.4f);
            Sp -= 6;
            if (Sp <= 0) {
                Sp = 0;
            }

            audio_s.clip = _audio[1];
            sound_play();

            Sp_bar.GetComponent<hp_sp>().sp = Sp;
            Sp_bar.GetComponent<hp_sp>().trans_sp_bar();

            skeletonAnimation.timeScale = ani_speed + 0.25f;
            skeletonAnimation.AnimationState.SetAnimation(0, "jump_attack", false);
            jumpattack_bool = false;
            PlayerState.Instance.Vertical = Vertical.Airborne_high;
        }
    }


    public void oneshot_death () {    //즉사 스킬 맞을 경우
        
        Hp = 0;
        audio_s.clip = _audio[8];
        sound_play();

        Hp_bar.GetComponent<hp_sp>().hp = Hp;
        Hp_bar.GetComponent<hp_sp>().trans_hp_bar();

        skeletonAnimation.AnimationState.SetAnimation(0, "die", false);
        HorizontalMotion = 0;
        Rigid.gravityScale = 2;
        PlayerState.Instance.State = State.die;
        Time.timeScale = 0.3f;
        
    }


    public IEnumerator invincibility() {
        invin_bool = true;
        transform.gameObject.layer = 12;
        for (int i = 0; i< 120; i++) {
            if(i % 3 == 1) {
                skeleton.a = 0.2f;
            } else {
                skeleton.a = 1;
            }
            yield return null;
        }
        transform.gameObject.layer = 8;
        invin_bool = false;
    }

    
    public IEnumerator sp_up() {
        float sp_up_num;

        if(Global.myEquip.Accessory_0 == 7 || Global.myEquip.Accessory_1 == 7 || Global.myEquip.Accessory_2 == 7) {
            sp_up_num = (0.17f + Global.myAccessory_int[7]*0.004f)  + Global.dorothyData.Stamina *0.001f;
        } else {
            sp_up_num = 0.15f + Global.dorothyData.Stamina * 0.001f;
        }

        while(true) {
            if(PlayerState.Instance.State == State.Idle) {
             
                if(Sp == 0) {
                    yield return null;
                    yield return null;
                    PlayerState.Instance.State = State.tired;
                    MoveSpeed = 2;
                    if (HorizontalMotion == 0) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "tired_idle", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "tired_walk", true);
                    }
                    yield return YieldInstructionCache.WaitForSeconds(1.2f);
                    PlayerState.Instance.State = State.Idle;
                    

                    if (HorizontalMotion == 0) {
                        skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                    } else {
                        skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                    }
                    
                    MoveSpeed = basis_speed;
                }

                if (Time.timeScale == 1) {
                    Sp += sp_up_num;  // *  (MaxSp / 20);
                    if (Sp >= MaxSp) {
                        Sp = MaxSp;
                    }
                    Sp_bar.GetComponent<hp_sp>().sp = Sp;
                    Sp_bar.GetComponent<hp_sp>().trans_sp_bar();
                }
            } else if(PlayerState.Instance.State == State.jump) {
                if (Time.timeScale == 1) {
                    if(jumpattack_bool == true) {
                        if (Sp != 0) {
                            Sp += sp_up_num;  // *  (MaxSp / 20);
                            if (Sp >= MaxSp) {
                                Sp = MaxSp;
                            }
                            Sp_bar.GetComponent<hp_sp>().sp = Sp;
                            Sp_bar.GetComponent<hp_sp>().trans_sp_bar();
                        }
                    }
                }
            }
            yield return null;
        }
    }

    public void bloodsucking() {
        Hp += (1 + Global.myAccessory_int[2]*0.2f);
        if (Hp >= MaxHp) {
            Hp = MaxHp;
        }
        Hp_bar.GetComponent<hp_sp>().hp = Hp;
        Hp_bar.GetComponent<hp_sp>().trans_hp_bar();
    }

    public void potion() {
        Hp += (50 + Global.myItem_int[0] * 5);
        if(Hp >= MaxHp) {
            Hp = MaxHp;
        }
        
        Hp_bar.GetComponent<hp_sp>().hp = Hp;
        Hp_bar.GetComponent<hp_sp>().trans_hp_bar();
    }

    public void bridge() {
        stop();
        _camera.transform.GetChild(7).gameObject.SetActive(true);
    }

    public void hp_spring() {
        Hp = MaxHp;
        Hp_bar.GetComponent<hp_sp>().hp = Hp;
        Hp_bar.GetComponent<hp_sp>().trans_hp_bar();
    }
    
    public void weaponchange() {   //무기 체인지
        string weapon_name = "sword/sword_" + string.Format("{0}", Global.item_num);
        skeletonAnimation.skeleton.SetAttachment("sword_1", weapon_name);
        basis_atk_def_init();
    }

    public void clotheschange() {
        string clothes_name = "cloth_" + string.Format("{0}", Global.item_num);

        skeletonAnimation.skeleton.SetSkin(clothes_name);
        skeletonAnimation.skeleton.SetSlotsToSetupPose();

        string weapon_name = "sword/sword_" + string.Format("{0}", Global.myEquip.Weapon);
        skeletonAnimation.skeleton.SetAttachment("sword_1", weapon_name);
        basis_atk_def_init();
        basis_speed_init();
    }

    public void init_weapon_clothes() {
        string clothes_name = "cloth_" + string.Format("{0}", Global.myEquip.Clothes);

        skeletonAnimation.skeleton.SetSkin(clothes_name);
        skeletonAnimation.skeleton.SetSlotsToSetupPose();

        string weapon_name = "sword/sword_" + string.Format("{0}", Global.myEquip.Weapon);
        skeletonAnimation.skeleton.SetAttachment("sword_1", weapon_name);
    }

    void sound_play () {
        if (Global.soundbool) {
            audio_s.Play();
        }
    }

}