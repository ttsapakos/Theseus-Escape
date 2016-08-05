using UnityEngine;
using System.Collections;

public class MazePassage : MazeCellEdge {

	override public bool isWall () {
		return false;
	}
}
