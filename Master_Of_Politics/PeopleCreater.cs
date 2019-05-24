using UnityEngine;
using System.Collections;
using GlobalMember;

public class PeopleCreater : MonoBehaviour {

    GameObjectPool objectPool;
    float createDelay;
    public float speed;
    string peoplePath = "Prefabs/Minigame/people";
    
    private void Awake ()
    {
        objectPool = GameObjectPool.Instance;
        objectPool.ReserveInstance(peoplePath, 5);
    }

    void Start () {
        if(PlayerPrefs.HasKey("clear_"+ Global.presidentnum) == true) { //초월모드 유무 확인
            createDelay = 0.6f;
            speed = 0.06f;
        } else {
            createDelay = 0.8f;
            speed = 0.04f;
        }
        
        StartCoroutine("Peoplego");
        StartCoroutine("Createtimetrans");
	}

    public IEnumerator Peoplego() {
        while (true) {
            if(Time.timeScale == 1) {
                GameObject go = objectPool.CreateInstance(peoplePath, transform);
                go.SetActive(true);
            }
            yield return new WaitForSeconds(createDelay);
        }
    }

    public IEnumerator Createtimetrans () {
        while (true) {
            yield return new WaitForSeconds(10);
            createDelay -= 0.13f;
            speed += 0.01f;
        }
    }
}
