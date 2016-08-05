using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

	public float speed;
	private const float Deg2Rad = (Mathf.PI / 180);
	private Maze maze;
	private bool connected = false;

	// Use this for initialization
	void Start () { 
		transform.position = new Vector3 (((Maze.size.x / 2) * -1) + 0.5f, 0.6f, ((Maze.size.z / 2) * -1) + 0.5f);
	}

	void FixedUpdate() 
	{
		if (!connected) {
			ConnectMaze ();
		}

		if (maze.mazeReady) {
			float moveHorizontal = Input.GetAxis ("Horizontal");
			float moveVertical = Input.GetAxis ("Vertical");
			Transform camera = transform.GetChild (0).transform;


			Vector3 movement = new Vector3 ((Mathf.Cos (camera.eulerAngles.y * Deg2Rad) * moveHorizontal) + (moveVertical * Mathf.Sin (camera.eulerAngles.y * Deg2Rad)), 
		                                0.0f, 
		                                (moveVertical * Mathf.Cos (camera.eulerAngles.y * Deg2Rad)) + (moveHorizontal * -1 * Mathf.Sin (camera.eulerAngles.y * Deg2Rad)));
			transform.position += movement * speed;
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
