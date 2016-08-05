using UnityEngine;
using System.Collections;

public class GameOver : MonoBehaviour {

	void OnTriggerEnter (Collider other) {
		if (other.CompareTag("MainCamera")) {
			Application.LoadLevel (Application.loadedLevel);
		}
	}
}
