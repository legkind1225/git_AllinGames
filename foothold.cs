using UnityEngine;
using System.Collections;
using GlobalMember;

public class foothold : MonoBehaviour {
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxcollider;

    public GameObject _animal;

    public Sprite[] forest = new Sprite[4];
    public Sprite fish;
    public Sprite firestone;

    float y, animaly;
    float breakspeed = 1;
    float speeddown = 0.01f;
    float breaktime;
    float a = 1;

    public bool colbool;

    public bool colzerobool;
    

    void Awake() {
        boxcollider = gameObject.GetComponent<BoxCollider2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        _animal = GameObject.FindWithTag("Animal");

        y = transform.parent.localPosition.y;
        colbool = false;
        colzerobool = false;
    }

    void OnEnable() {
        switch (Global.mapstate) {
            case 0:
                spriteRenderer.sprite = forest[0];
                break;
            case 1:
                spriteRenderer.sprite = fish;
                break;
            case 2:
                spriteRenderer.sprite = firestone;
                break;
        }
        

        a = 1;
        spriteRenderer.color = new Color(1, 1, 1, a);
        boxcollider.size = new Vector2(0, 0);
    }
	
	// Update is called once per frame
	void Update () {
        if(colbool == true) {
            StartCoroutine("col");
            colbool = false;
        }

        if(colzerobool == true) {
            //init
            boxcollider.size = new Vector2(0, 0);
            colzerobool = false;
        }
	}


    public IEnumerator col () {
        yield return new WaitForSeconds(0.5f);
        boxcollider.size = new Vector2(0.75f, 0.15f);

    }

    public IEnumerator footbreak() {
        for (int i = 1; i < 4; i++) {
            if(Time.timeScale == 1) {
                breaktime = breakspeed - Global.jumpnum * speeddown;
                if (breaktime < 0.05f) {
                    breaktime = 0.05f;
                }
                yield return new WaitForSeconds(breaktime);
                spriteRenderer.sprite = forest[i];
            }
        }

        while (true) {
            a -= 0.05f;
            spriteRenderer.color = new Color(1, 1, 1, a);
            if (a < 0) {
                break;
            }
            yield return null;
        }

        boxcollider.size = new Vector2(0, 0);
    }

    void OnTriggerEnter2D ( Collider2D other ) {
        if (other.name == "3_Animal") {
            if(Global.mapstate == 0) {
                StartCoroutine("footbreak");
            }
        }
    }

}
