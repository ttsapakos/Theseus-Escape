using UnityEngine;
using System.Collections;

public class CameraRotation : MonoBehaviour {

	public int cameraSpeed = 2;
	private bool connected = false;
	private Maze maze;

	// Update is called once per frame
	void Update () {
		if (!connected) {
			ConnectMaze ();
		}

		if (maze.mazeReady) {
			float v = cameraSpeed * Input.GetAxis ("Mouse X");
			transform.Rotate (0, v, 0);
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