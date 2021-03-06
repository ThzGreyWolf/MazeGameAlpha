using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour {

	Queue<PathResult> results = new Queue<PathResult> ();

	static PathRequestManager instance;
	Pathfinding pathfinding;


	void Awake() {
		instance = this;
		pathfinding = GetComponent<Pathfinding> ();
	}

	void Update() {
		if (results.Count > 0) {
			int itemsInQueue = results.Count;
			lock (results) {
				for (int i = 0; i < itemsInQueue; i++) {
					PathResult result = results.Dequeue ();
					result.callback (result.path, result.success);
				}
			}
		}
	}

	public static void RequestPath(PathRequest request) {
		ThreadStart threadStart = delegate {
			instance.pathfinding.FindPath (request, instance.FinishedPrecessingPath);
		};
		threadStart.Invoke ();
	}

	public void FinishedPrecessingPath(PathResult result) {
		lock (results) {
			results.Enqueue (result);
		}
	}
}

public struct PathResult {
	public Vector3[] path;
	public bool success;
	public Action <Vector3[], bool> callback;

	public PathResult(Vector3[] _path, bool _success, Action<Vector3[], bool> _callback) {
		path = _path;
		success = _success;
		callback =_callback;
	}
}

public struct PathRequest {
	public Vector3 pathStart;
	public Vector3 pathEnd;
	public Action <Vector3[], bool> callback;

	public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback) {
		pathStart = _start;
		pathEnd = _end;
		callback = _callback;
	}
}
