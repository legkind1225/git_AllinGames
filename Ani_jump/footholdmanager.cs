using UnityEngine;
using System.Collections;
using GlobalMember;

public class footholdmanager : MonoBehaviour {
    private BoxCollider2D boxcollider;
    public GameObject _animal;
    public Transform[] tran = new Transform[4];
    public Transform childfoot;

    float y, animaly;
    
    public bool upbool;

    int oddeven;
    int childnum;
    int thornran;

    int ran1, ran2;

    void Awake() {
        boxcollider = gameObject.GetComponent<BoxCollider2D>();
        tran = gameObject.GetComponentsInChildren<Transform>();
        y = transform.localPosition.y;
        upbool = false;

        
        for(int i = 0; i<10; i++) {
            if(transform.parent.GetChild(i) == transform) {
                oddeven = i;
            } 
        }

        if(oddeven % 2 == 0) {
            childnum = 4;
        } else {
            childnum = 3;
        }
    }

    void OnEnable() {

        for (int i = 0; i < childnum; i++) {
            transform.GetChild(i).gameObject.SetActive(false);  //전부 비활성화
            transform.GetChild(i).gameObject.SetActive(true);  //전부 활성화
            transform.GetChild(i).gameObject.GetComponent<foothold>().colzerobool = true;
        }
        
        if (Global.jumpnum < 10) {  //  4, 3
            if (childnum == 4) {
                ran1 = Random.Range(0, 4);

                transform.GetChild(ran1).gameObject.SetActive(false);
            }
        } else if (Global.jumpnum < 20) {  // 3, 3
            if (childnum == 4) {
                ran1 = Random.Range(0, 4);
                transform.GetChild(ran1).gameObject.SetActive(false);
            } else {
                ran1 = Random.Range(0, 3);
                transform.GetChild(ran1).gameObject.SetActive(false);
            }

        } else {  // 2, 2
            if (childnum == 4) {
                ran1 = Random.Range(0, 4);
                transform.GetChild(ran1).gameObject.SetActive(false);
                while (true) {
                    ran2 = Random.Range(0, 4);
                    if (ran1 != ran2) {
                        break;
                    }
                }
                transform.GetChild(ran2).gameObject.SetActive(false);

            } else {
                ran1 = Random.Range(0, 3);
                transform.GetChild(ran1).gameObject.SetActive(false);
            }
        }
        
        // 장애물
        switch (Global.mapstate) {
            case 0:
                if (oddeven == 9) {
                    transform.GetChild(3).gameObject.SetActive(false);  //가시 비활성화
                    transform.GetChild(3).gameObject.SetActive(true);  //가시 활성화
                    transform.GetChild(5).gameObject.SetActive(false);  //UFO 비활성화
                } else if (oddeven == 5) {
                    thornran = Random.Range(0, 3);
                    transform.GetChild(4).gameObject.SetActive(false);  //가시 비활성화
                    transform.GetChild(5).gameObject.SetActive(false);  //UFO 비활성화
                    if (thornran == 0) {
                        transform.GetChild(4).gameObject.SetActive(true);  //가시 활성화
                    }
                    transform.GetChild(childnum).gameObject.SetActive(false);  //복어 비활성화
                } else {
                    transform.GetChild(childnum).gameObject.SetActive(false);  //복어 비활성화
                }
                break;
            case 1:
                if (oddeven == 9) {
                    transform.GetChild(3).gameObject.SetActive(false);  //가시 비활성화
                    transform.GetChild(4).gameObject.SetActive(false);  //산소 비활성화
                    transform.GetChild(4).gameObject.SetActive(true);  //산소 활성화
                } else if (oddeven == 5) {
                    transform.GetChild(4).gameObject.SetActive(false);  //가시 비활성화
                }

                break;
            case 2:
                if (oddeven == 9) {
                    transform.GetChild(3).gameObject.SetActive(false);  //가시 비활성화
                    
                } else if (oddeven == 5) {
                    transform.GetChild(4).gameObject.SetActive(false);  //가시 비활성화
                    transform.GetChild(childnum).gameObject.SetActive(false);  //복어 비활성화
                } else {
                    transform.GetChild(childnum).gameObject.SetActive(false);  //복어 비활성화
                }
                break;
        }
        
        y = transform.localPosition.y;
    }
	
	// Update is called once per frame
	void Update () {
        if(upbool == true) {
            StartCoroutine("uptime");
            upbool = false;
        }
    }
    
    public IEnumerator uptime() {
        //yield return new WaitForSeconds(0.5f); // first speed
        yield return new WaitForSeconds(0.4f);   // second speed
        
        transform.localPosition = new Vector2(0, y + 13);
        y = transform.localPosition.y;

        for(int i = 0; i <childnum; i++) {
            transform.GetChild(i).gameObject.SetActive(false);  //전부 비활성화
            transform.GetChild(i).gameObject.SetActive(true);  //전부 활성화
            transform.GetChild(i).gameObject.GetComponent<foothold>().colzerobool = true;
        }

        yield return new WaitForSeconds(0.5f);

        //선택적 비활성화(비는 계단)
        if (Global.jumpnum < 10) {  //  4, 3
            if (childnum == 4) {
                ran1 = Random.Range(0, 4);

                transform.GetChild(ran1).gameObject.SetActive(false);
            }
        } else if(Global.jumpnum < 20) {  // 3, 3
            if (childnum == 4) {
                ran1 = Random.Range(0, 4);
                transform.GetChild(ran1).gameObject.SetActive(false);
            } else {
                ran1 = Random.Range(0, 3);
                transform.GetChild(ran1).gameObject.SetActive(false);
            }

        } else {  // 2, 2
            if (childnum == 4) {
                ran1 = Random.Range(0, 4);
                transform.GetChild(ran1).gameObject.SetActive(false);
                while (true) {
                    ran2 = Random.Range(0, 4);
                    if(ran1 != ran2) {
                        break;
                    }
                }
                transform.GetChild(ran2).gameObject.SetActive(false);

            } else {
                ran1 = Random.Range(0, 3);
                transform.GetChild(ran1).gameObject.SetActive(false);
            }
        }


        // 장애물
        switch (Global.mapstate) {
            case 0:
                if (oddeven == 9) {
                    transform.GetChild(3).gameObject.SetActive(false);  //가시 비활성화
                    transform.GetChild(5).gameObject.SetActive(false);  //UFO 비활성화
                    if (Global.jumpnum < 130) {
                        transform.GetChild(3).gameObject.SetActive(true);  //가시 활성화
                    } else {
                        
                        transform.GetChild(5).gameObject.SetActive(true);  //UFO 활성화
                    }
                } else if (oddeven == 5) {
                    transform.GetChild(4).gameObject.SetActive(false);  //가시 비활성화
                    transform.GetChild(5).gameObject.SetActive(false);  //UFO 비활성화
                    thornran = Random.Range(0, 3);
                    if (thornran == 0) {
                        if (Global.jumpnum < 130) {
                            transform.GetChild(4).gameObject.SetActive(true);  //가시 활성화
                        } else {
                            transform.GetChild(5).gameObject.SetActive(false);  //UFO 비활성화
                            transform.GetChild(5).gameObject.SetActive(true);  //UFO 활성화
                        }
                    }
                    
                    
                }
                break;
            case 1:
                if (oddeven == 9) {
                    transform.GetChild(3).gameObject.SetActive(false);  //가시 비활성화
                    transform.GetChild(4).gameObject.SetActive(false);  //산소 비활성화
                    transform.GetChild(4).gameObject.SetActive(true);  //산소 활성화
                } else if (oddeven == 5) {
                    transform.GetChild(4).gameObject.SetActive(false);  //가시 비활성화
                } 

                if( oddeven != 9) {
                    thornran = Random.Range(0, 7);
                    transform.GetChild(childnum).gameObject.SetActive(false);  //복어 비활성화
                    if (thornran == 0) {

                        transform.GetChild(childnum).gameObject.SetActive(true);  //복어 활성화
                    }
                }

                break;
            case 2:
                if (oddeven == 9) {
                    transform.GetChild(3).gameObject.SetActive(false);  //가시 비활성화

                } else if (oddeven == 5) {
                    transform.GetChild(4).gameObject.SetActive(false);  //가시 비활성화
                    transform.GetChild(childnum).gameObject.SetActive(false);  //복어 비활성화
                } else {
                    transform.GetChild(childnum).gameObject.SetActive(false);  //복어 비활성화
                }
                break;
        }



    }
}
