using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WelfareButton : MonoBehaviour {

    public Transform _people;
    public GameObject _OX;
    public GameObject _timeBar;
    public GameObject _Combo;
    public GameObject _ComboShadow;
    public GameObject _decision;

    public GameObject _effect;

    GameObject selectedPeople;

    Combo combo;
    Combo comboShadow;
    Decision decision;
    OxAni oxAni;
    TimeBar timebar;
    
    float[] peopley;
    public int oxnum;

    public bool buttonbool;
    bool peoplebool;

    private void Awake ()
    {
        combo = _Combo.GetComponent<Combo>();
        comboShadow = _ComboShadow.GetComponent<Combo>();
        decision = _decision.GetComponent<Decision>();
        oxAni = _OX.GetComponent<OxAni>();
        timebar = _timeBar.GetComponent<TimeBar>();

        buttonbool = true;
    }
    
    GameObject SelectPeople()
    {
        float minY = _people.GetChild(0).localPosition.y;
        GameObject selectObject = _people.GetChild(0).gameObject;

        for (int i = 0; i < _people.childCount; i++)
        {
            if(minY > _people.GetChild(i).localPosition.y)
            {
                minY = _people.GetChild(i).localPosition.y;
                selectObject = _people.GetChild(i).gameObject;
            }
        }
        return selectObject;
    }

    void OnMouseDown () {
        if (_people.childCount == 0)
            return;

        selectedPeople = SelectPeople();

        if(buttonbool)
        {
            if (oxnum == selectedPeople.GetComponent<people>().peopleNum % 2)
            {
                Success();
            }
            else
            {
                Failure();
            }
        }
    }

    void Success()
    {
        _effect.SetActive(false);
        _effect.SetActive(true);
        _effect.transform.localPosition = new Vector3(selectedPeople.transform.localPosition.x, selectedPeople.transform.localPosition.y, -0.5f);
        comboShadow.textselect();
        combo.textselect();
        decision.perfectbool = true;
        decision.textselect();
        selectedPeople.SetActive(false);
        oxAni.Otran();
    }

    void Failure()
    {
        combo.combonum = 0;
        comboShadow.combonum = 0;
        decision.perfectbool = false;
        decision.textselect();
        oxAni.Xtran();
        timebar.timeminus();
    }
}
