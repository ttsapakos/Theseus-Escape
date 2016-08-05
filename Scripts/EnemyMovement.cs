using UnityEngine;
using System.Collections;

struct EnemyInfo {
	public bool wallNorth;
	public bool wallSouth;
	public bool wallEast;
	public bool wallWest;
	public MazeDirection directionFacing;

	public EnemyInfo(bool wallNorth, bool wallSouth, bool wallWest, bool wallEast, MazeDirection directionFacing) {
		this.wallNorth = wallNorth;
		this.wallSouth = wallSouth;
		this.wallWest = wallWest;
		this.wallEast = wallEast;
		this.directionFacing = directionFacing;
	}
}

public class EnemyMovement : MonoBehaviour {
	// The current cell that the enemy is in
	private MazeCell currentCell;
	// the instance of the maze
	private Maze maze;
	// The name of the cell that we'll want to get
	private string cellName;
	// Whether the maze is connected or not
	private bool connected = false;
	// an instance of this enemy's informations
	private EnemyInfo info;
	// movement rate for walking
	private const float walkSpeed = 0.01f;
	// movement limit
	// PRECONDITION: move walk limit MUST be 1 / walk speed;
	private static int MOVE_LIMIT_WALK = 100;
	// the total movecount, for counting movement
	private int moveCount;
	// to determine if the minotaur is walking or running
	private bool isWalking;
	// movement rate for charging
	private const float chargeSpeed = 0.04f;
	// movement limit
	// PRECONDITION: move walk limit MUST be 1 / walk speed;
	private static int MOVE_LIMIT_CHARGE = 25;
	// a list of all of the spaces taht are directly in front of the enemy
	private MazeCell farthestCellInSight;
	// the position of the player
	public BoxCollider playerPos;



	// the roar sound effect
	public AudioClip Roar;
	// For how often the bull roars
	public float roarRate;
	private float nextRoar;

	// the growl sound effect
	public AudioClip Growl;
	// for how often it growls;
	private float growlRate;
	private float nextGrowl;

	void Start () {
		// set the position to the most northwest corner
		// 0.51 and 0.49 are to put the enemy in the optimal starting position
		transform.position = new Vector3 (-1.0f * (Maze.size.x / 2) + 0.51f, 0.6f, (Maze.size.z / 2) - 0.49f);
		info = new EnemyInfo (false, false, false, false, MazeDirection.North);
		isWalking = true;

		if (isWalking) {
			moveCount = MOVE_LIMIT_WALK;
		} else {
			moveCount = MOVE_LIMIT_CHARGE;
		}
	}


	void Update () {
		if (!connected) {
			ConnectMaze ();
		}

		// Set the string for the name equal to the most northwest corner cell
		cellName = "Maze Cell " + (Mathf.Floor (transform.position.x) + (Maze.size.x / 2)) + ", " + (Mathf.Floor (transform.position.z) + (Maze.size.z / 2));

		if (maze.mazeReady) {

			ConnectCell ();
			GetWallsAround ();

			// Growl if appropriate
			if (Time.time > nextGrowl) {
				nextGrowl = Time.time + Random.Range(20, 30);
				AudioSource.PlayClipAtPoint(Growl, transform.position);
			}

			if(isWalking) {
				TurnOrMove(MOVE_LIMIT_WALK, walkSpeed);
			} else {
				ChargeStraight(MOVE_LIMIT_CHARGE, chargeSpeed);
			}
		}

		//Debug.Log (isWalking);
	}

	// For use when the enemy is walking, to choose when to turn and if to charge
	private void TurnOrMove (int limit, float speed) {
		if (moveCount == limit) {
			// check the left hand side is a wall
			if (CheckDirection(info, false)) {
				// check if straight ahead is a wall
				if (CheckDirection(info, true)) {
					info.directionFacing = rotateClockwise(info.directionFacing);
					// check straight ahead again, to see if you've hit a dead end, if so, rotate again
					if (CheckDirection(info, true)) {
						info.directionFacing = rotateClockwise(info.directionFacing);
					}
				}
				// if there's no left hand wall, rotate left
			} else {
				info.directionFacing = rotateCounterClockwise(info.directionFacing);
			}
			moveCount = 0;
			// Get the farthest cell in line of sight
			farthestCellInSight = getFarthestCellInSight(info);
			ChooseWalkingOrCharging ();
		} else {
			//Debug.Log("Facing: " + info.directionFacing + " North: " + info.wallNorth + " East: " + info.wallEast + " South: " + info.wallSouth + " West: " + info.wallWest);
			Move (info, speed);
			moveCount++;
		}
	}

	// For use when the enemy is charging, so that they continue straight without turning so long as they're charging
	private void ChargeStraight (int limit, float speed) {
		if (moveCount == limit) {
			// check straight ahead, if no wall, continue
			if (!CheckDirection (info, true)) {
				// Do nothing
			// else there is a wall ahead
			} else {
				// check if there's a wall on the left, if not, rotate left
				if (!CheckDirection (info, false)) {
					info.directionFacing = rotateCounterClockwise (info.directionFacing);
				// else, rotate right
				} else {
					info.directionFacing = rotateClockwise (info.directionFacing);
					// check straight ahead again, to see if you've hit a dead end, if so, rotate again

					if (CheckDirection(info, true)) {
						info.directionFacing = rotateClockwise(info.directionFacing);
					}
				}
			}

			moveCount = 0;
			// Get the farthest cell in line of sight
			farthestCellInSight = getFarthestCellInSight(info);
			ChooseWalkingOrCharging ();
		} else {
			Move(info, speed);
			moveCount++;
		}
	}

	// Move the enemy in the correct direction
	private void Move (EnemyInfo info, float speed) {
		if (info.directionFacing == MazeDirection.North) {
			transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z + speed);
		}

		if (info.directionFacing == MazeDirection.South) {
			transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z + (-1.0f * speed));
		}

		if (info.directionFacing == MazeDirection.East) {
			transform.position = new Vector3 (transform.position.x + speed, transform.position.y, transform.position.z);
		}

		if (info.directionFacing == MazeDirection.West) {
			transform.position = new Vector3 ((transform.position.x + (-1.0f * speed)), transform.position.y, transform.position.z);
		}
	}

	// Gets the farthest cell within a straight line of sight of the minotaur
	private MazeCell getFarthestCellInSight (EnemyInfo info) {
		// the temporary cell that will be returned
		MazeCell tempCell;
		GameObject tempCellObject = maze.transform.FindChild(cellName).gameObject;
		// if the cell is found
		if (tempCellObject != null) {
			tempCell = tempCellObject.GetComponent<MazeCell> ();
			// keep going in the direction until you've hit a wall
			while (!tempCell.GetEdge(info.directionFacing).isWall()) {
				tempCell = tempCell.GetEdge(info.directionFacing).otherCell;
			}
			// return the cell that has a wall
			return tempCell;
		} else {
			Debug.Log ("Cannot find MazeCell");
			return null;
		}
	}

	// determines if the player is within the line of sight and changes the speed if they are
	private void ChooseWalkingOrCharging () {
		float distToWallX = Mathf.Abs(farthestCellInSight.transform.position.x - transform.position.x);
		float distToWallZ = Mathf.Abs(farthestCellInSight.transform.position.z - transform.position.z);
		float distToPlayerX = Mathf.Abs(playerPos.transform.position.x - transform.position.x);
		float distToPlayerZ = Mathf.Abs(playerPos.transform.position.z - transform.position.z);

		if (((distToWallX > distToPlayerX) && (distToPlayerZ <= 0.5f)) ||
			((distToWallZ > distToPlayerZ) && (distToPlayerX <= 0.5f))) {
			isWalking = false;

			// Roar if appropriate
			if (Time.time > nextRoar) {
				nextRoar = Time.time + roarRate;
				AudioSource.PlayClipAtPoint(Roar, transform.position);
			}

		} else {
			isWalking = true;
		}

		//Debug.Log (playerPos.transform.position);
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

	// Gets the current cell
	private void ConnectCell () {
		GameObject currenctCellObject = maze.transform.FindChild(cellName).gameObject;
		if (currenctCellObject != null) {
			currentCell = currenctCellObject.GetComponent<MazeCell> ();
		} else {
			Debug.Log ("Cannot find MazeCell");
		}
	}

	// Gets the walls around the current position
	private void GetWallsAround () {
		info.wallNorth = currentCell.GetEdge (MazeDirection.North).isWall ();
		info.wallSouth = currentCell.GetEdge (MazeDirection.South).isWall ();
		info.wallEast = currentCell.GetEdge (MazeDirection.East).isWall ();
		info.wallWest = currentCell.GetEdge (MazeDirection.West).isWall ();
	}

	// Checks if there is a wall at the given location of the cell
	// true means straight is being checked, false means on counterclockwise is being checked
	private bool CheckDirection (EnemyInfo info, bool straightOrLeft) {
		if (info.directionFacing == MazeDirection.North) {
			if (straightOrLeft == true) {
				return info.wallNorth;
			} else {
				return info.wallWest;
			}
		}

		else if (info.directionFacing == MazeDirection.East) {
			if (straightOrLeft == true) {
				return info.wallEast;
			} else {
				return info.wallNorth;
			}
		}


		else if (info.directionFacing == MazeDirection.South) {
			if (straightOrLeft == true) {
				return info.wallSouth;
			} else {
				return info.wallEast;
			}
		}

		else {
			if (straightOrLeft == true) {
				return info.wallWest;
			} else {
				return info.wallSouth;
			}
		}
	}

	// get the direction clockwise from where you are
	private MazeDirection rotateClockwise (MazeDirection dir) {
		if (dir == MazeDirection.North) {
			return MazeDirection.East;
		} else if (dir == MazeDirection.East) {
			return MazeDirection.South;
		} else if (dir == MazeDirection.South) {
			return MazeDirection.West;
		} else {
			return MazeDirection.North;
		}
	}

	// get the direction counterClockwise from where you are
	private MazeDirection rotateCounterClockwise (MazeDirection dir) {
		if (dir == MazeDirection.North) {
			return MazeDirection.West;
		} else if (dir == MazeDirection.East) {
			return MazeDirection.North;
		} else if (dir == MazeDirection.South) {
			return MazeDirection.East;
		} else {
			return MazeDirection.South;
		}
	}
}
	
