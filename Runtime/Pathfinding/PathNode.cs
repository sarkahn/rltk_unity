using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RLTK.Pathfinding
{
	/// <summary>
	/// An position index/cost pair representing a node in a pathfinding grid.
	/// </summary>
	[System.Serializable]
	public struct PathNode
	{
		public int posIndex;
		public int cost;
	} 
}
