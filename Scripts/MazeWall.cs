using UnityEngine;
using System.Collections;

public class MazeWall : MazeCellEdge {

	override public bool isWall () {
		return true;
	}
}
