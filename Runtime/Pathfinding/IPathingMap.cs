using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace RLTK.Pathfinding
{
	public interface IPathingMap
	{
		/// <summary>
		/// Return the available exits for a given position on a pathing path
		/// </summary>
		/// <param name="posIndex"></param>
		/// <returns></returns>
		NativeArray<PathNode> GetAvailableExits(int posIndex);
	} 
}
