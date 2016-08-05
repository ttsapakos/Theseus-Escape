using UnityEngine;
using System.Collections;

public class KillPlayer : MonoBehaviour {

	private GameManager gameManager;

	void Start()
	{
		GameObject gameManagerObject = GameObject.FindWithTag ("GameManager");
		if (gameManagerObject != null) {
			gameManager = gameManagerObject.GetComponent<GameManager>();
		} else {
			Debug.Log ("Cannot find 'GameManager' script");
		}
	}

	void OnTriggerEnter(Collider other) {
		Destroy(other.gameObject);
		gameManager.GameOver();
	}
}
