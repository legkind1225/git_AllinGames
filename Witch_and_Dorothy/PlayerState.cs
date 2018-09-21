using UnityEngine;
using System.Collections;

public class PlayerState : MonoBehaviour
{
    //Singleton instance, globally accessible for setting player's state
    private static PlayerState _instance;
    public static PlayerState Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameObject("PlayerState").AddComponent<PlayerState>();

            return _instance;
        }
    }
    
    public Horizontal Horizontal;
    public Vertical Vertical;
    public DirectionFacing DirectionFacing;
    public State State;
    public Attack_reserve Attack_reserve;
}

//멈춤, 좌,우 움직임
public enum Horizontal
{
    Idle = 0,
    MovingLeft = -1,
    MovingRight = 1
}

//땅위에 있음, 점프, 이단점프상태
public enum Vertical
{
    Grounded,
    Airborne,
    Airborne_high
}

//케릭터가 바라보는 방향
public enum DirectionFacing
{
    Left = -1,
    Right = 1
}

//케릭터의 상태
public enum State {
    Idle,
    jump,
    attack,
    smash,
    hurt,
    hug,
    guard,
    guard_break,
    rolling,
    landing,
    tired,
    die
}

//케릭터의 공격 예약(모션이 끝나기전 미리 키를입력할 경우)
public enum Attack_reserve {
    idle,
    wait,
    attack,
    smash
}
