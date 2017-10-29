using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesScript : MonoBehaviour {
	public int lineNum;
	private GameManager _gameManager;
	int dx ,dy;

	// Use this for initialization
	void Start () {
		_gameManager = GameObject.Find ("GameManager").GetComponent<GameManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.position += Vector3.down * 5 * Time.deltaTime;
		if (this.transform.position.y < -5.0f) {
			//Debug.Log("false");
			//false
			Destroy (this.gameObject);
		}
	}

	void OnTriggerStay(Collider other){
		switch (lineNum) {
		case 0:
			//CheckInput (KeyCode.D);
			//left
			dx = _gameManager.getDx ();
			dy = _gameManager.getDy ();
			if (dx < 0 && dy > 0) {
				Debug.Log ("↑ or ←");
				goodTongue ();
			} else if (dx < 0 && dy < 0) {
				Debug.Log ("← or ↓");
				goodTongue ();
			}
			break;
		case 1:
			//CheckInput (KeyCode.F);
			//up
			dx = _gameManager.getDx ();
			dy = _gameManager.getDy ();
			if (dx > 0 && dy < 0) {
				Debug.Log ("→ or ↑");
				goodTongue ();
			} else if (dx < 0 && dy > 0) {
				Debug.Log ("↑ or ←");
				goodTongue ();
			}
			break;
		case 2:
			//CheckInput (KeyCode.Space);
			//down
			dx = _gameManager.getDx ();
			dy = _gameManager.getDy ();
			if (dx > 0 && dy > 0) {
				Debug.Log ("→ or ↓");
				goodTongue ();
			}else if (dx < 0 && dy < 0) {
				Debug.Log ("← or ↓");
				goodTongue ();
			}
			break;
		case 3:
			//CheckInput (KeyCode.J);
			//right
			dx = _gameManager.getDx ();
			dy = _gameManager.getDy ();
			if (dx > 0 && dy > 0) {
				Debug.Log ("→ or ↓");
				goodTongue ();
			} else if (dx > 0 && dy < 0) {
				Debug.Log ("→ or ↑");
				goodTongue ();
			}
			break;
		default:
			break;
		}
	}

	void goodTongue(){
		_gameManager.GoodTimingFunc (lineNum);
		Destroy (this.gameObject);
	}
}
