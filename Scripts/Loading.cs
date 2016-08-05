using UnityEngine;
using System.Collections;

public class Loading : MonoBehaviour {

	private bool connected = false;
	private Maze maze;
	private bool rendererDestroyed = false;

	// Update is called once per frame
	void Update () {
		if (!connected) {
			ConnectMaze ();
		}

		if (maze.mazeReady && !rendererDestroyed) {
			GameObject.Destroy(this.gameObject);
		}
	}

	// Gets the maze
	private void ConnectMaze () {
		GameObject mazeObject = GameObject.Find ("Maze(Clone)");
		if (mazeObject != null) {
			maze = mazeObject.GetComponent<Maze> ();
		} else {
			Debug.Log ("Cannot find Maze");
		}
		connected = true;
	}
}
