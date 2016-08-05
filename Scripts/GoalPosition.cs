using UnityEngine;
using System.Collections;

public class GoalPosition : MonoBehaviour {

	private int goalAreaX, goalAreaZ;

	// Use this for initialization
	void Start () {
		goalAreaX = (int) Mathf.Floor (Maze.size.x / 4);
		goalAreaZ = (int) Mathf.Floor (Maze.size.z / 4);

		transform.position = new Vector3 ((Maze.size.x / 2) - goalAreaX + 0.5f, 0.0f, (Maze.size.z / 2) - goalAreaZ + 0.5f);
	}
}