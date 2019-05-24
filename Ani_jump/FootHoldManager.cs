using UnityEngine;
using System.Collections;
using GlobalMember;

public class FootHoldManager : MonoBehaviour {
    private BoxCollider2D boxcollider;
    public GameObject _animal;
    public GameObject[] footHold = new GameObject[4];

    float y, animaly;
    
    public bool upbool;

    int oddEven;
    int childNum;
    int thornRan;

    int ran1, ran2;

    void Awake() {
        boxcollider = gameObject.GetComponent<BoxCollider2D>();
        
        for(int i = 0; i < 4; i++)
        {
            footHold[i] = transform.GetChild(i).gameObject;
        }
        
        y = transform.localPosition.y;
        upbool = false;
        
        for(int i = 0; i<10; i++) {
            if(transform.parent.GetChild(i) == transform) {
                oddEven = i;
            } 
        }

        if(oddEven % 2 == 0) {
            childNum = 4;
        } else {
            childNum = 3;
        }
    }

    void OnEnable() {
        DisableFootHold();
        InitFootHold();
        y = transform.localPosition.y;
    }
	
	// Update is called once per frame
	void Update () {
        if(upbool == true) {
            StartCoroutine("FootHoldUp");
            upbool = false;
        }
    }

    public IEnumerator FootHoldUp ()
    {
        yield return new WaitForSeconds(0.4f);
        transform.localPosition = new Vector2(0, y + 13);
        y = transform.localPosition.y;
        DisableFootHold();
        yield return new WaitForSeconds(0.5f);
        InitFootHold();
    }


    void DisableFootHold ()
    {
        for (int i = 0; i < childNum; i++)
        {
            footHold[i].SetActive(false);  //전부 비활성화
            footHold[i].SetActive(true);  //전부 활성화
            footHold[i].GetComponent<FootHold>().colZeroBool = true;
        }
    }

    void InitFootHold()
    {
        //점프 수에 따라 발판 조정
        if (Global.jumpnum < 10)
        {  //  4, 3
            if (childNum == 4)
            {
                ran1 = Random.Range(0, 4);
                footHold[ran1].SetActive(false);
            }
        }
        else if (Global.jumpnum < 20)
        {  // 3, 3
            if (childNum == 4)
            {
                ran1 = Random.Range(0, 4);
                footHold[ran1].SetActive(false);
            }
            else
            {
                ran1 = Random.Range(0, 3);
                footHold[ran1].SetActive(false);
            }

        }
        else
        {  // 2, 2
            if (childNum == 4)
            {
                ran1 = Random.Range(0, 4);
                footHold[ran1].SetActive(false);
                while (true)
                {
                    ran2 = Random.Range(0, 4);
                    if (ran1 != ran2)
                    {
                        break;
                    }
                }
                footHold[ran2].SetActive(false);

            }
            else
            {
                ran1 = Random.Range(0, 3);
                footHold[ran1].SetActive(false);
            }
        }

        // 스테이지에 따른 장애물 설정
        switch (Global.mapstate)
        {
            case 0:
                if (oddEven == 9)
                {
                    footHold[3].SetActive(false);  //가시 비활성화
                    footHold[3].SetActive(true);  //가시 활성화
                    footHold[5].SetActive(false);  //UFO 비활성화
                }
                else if (oddEven == 5)
                {
                    thornRan = Random.Range(0, 3);
                    footHold[4].SetActive(false);  //가시 비활성화
                    footHold[5].SetActive(false);  //UFO 비활성화
                    if (thornRan == 0)
                    {
                        footHold[4].SetActive(true);  //가시 활성화
                    }
                    footHold[childNum].SetActive(false);  //복어 비활성화
                }
                else
                {
                    footHold[childNum].SetActive(false);  //복어 비활성화
                }
                break;
            case 1:
                if (oddEven == 9)
                {
                    footHold[3].SetActive(false);  //가시 비활성화
                    footHold[4].SetActive(false);  //산소 비활성화
                    footHold[4].SetActive(true);  //산소 활성화
                }
                else if (oddEven == 5)
                {
                    footHold[4].SetActive(false);  //가시 비활성화
                }

                break;
            case 2:
                if (oddEven == 9)
                {
                    footHold[3].SetActive(false);  //가시 비활성화
                }
                else if (oddEven == 5)
                {
                    footHold[4].SetActive(false);  //가시 비활성화
                    footHold[childNum].SetActive(false);  //복어 비활성화
                }
                else
                {
                    footHold[childNum].SetActive(false);  //복어 비활성화
                }
                break;
        }
    }


    

}
