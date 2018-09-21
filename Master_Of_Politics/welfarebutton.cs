using UnityEngine;
using System.Collections;
using System;

public class welfarebutton : MonoBehaviour {

    public Transform _people;
    public GameObject _OX;
    public GameObject _timebar;
    public GameObject _Combo;
    public GameObject _Comboshadow;
    public GameObject _decision;

    public GameObject _effect;

    float[] peopley = new float[10];
    int oxnum;

    public bool buttonbool;
    bool peoplebool;
	// Use this for initialization
	void Start () {
        buttonbool = true;
        if (transform.name == "O_button") {
            oxnum = 1;
        } else {
            oxnum = 0;
        }

	}
    
    void OnMouseDown () {

        for (int i = 0; i < 10; i++) {
            if(_people.GetChild(i).gameObject.activeSelf == true) {
                peoplebool = true;
                break;
            }

            if(_people.GetChild(9).gameObject.activeSelf == false) {
                peoplebool = false;
            }
        }

        if(buttonbool == true) {
            if (peoplebool == true) {
                // first people select
                for (int i = 0; i < 10; i++) {
                    peopley[i] = _people.GetChild(i).localPosition.y;
                }
                Array.Sort(peopley);
                for (int i = 0; i < 10; i++) {
                    if (peopley[0] == _people.GetChild(i).localPosition.y) {
                        if (oxnum == _people.GetChild(i).gameObject.GetComponent<people>().peoplenum % 2) {
                            _effect.SetActive(false);
                            _effect.SetActive(true);
                            _effect.transform.localPosition = new Vector3(_people.GetChild(i).localPosition.x, _people.GetChild(i).localPosition.y, -0.5f);
                            _Comboshadow.GetComponent<Combo>().textselect();
                            _Combo.GetComponent<Combo>().textselect();
                            _decision.GetComponent<decision>().perfectbool = true;
                            _decision.GetComponent<decision>().textselect();
                            _people.GetChild(i).gameObject.SetActive(false);
                            _OX.GetComponent<OX_11>().Otran();
                            break;
                        } else {
                            _Combo.GetComponent<Combo>().combonum = 0;
                            _Comboshadow.GetComponent<Combo>().combonum = 0;
                            _decision.GetComponent<decision>().perfectbool = false;
                            _decision.GetComponent<decision>().textselect();
                            _OX.GetComponent<OX_11>().Xtran();
                            _timebar.GetComponent<timebar>().timeminus();
                            break;
                        }

                    }
                }
            }
        }
        


    }
}
