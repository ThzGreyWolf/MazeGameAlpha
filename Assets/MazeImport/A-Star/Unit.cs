using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

	const float minPathUpdateTime = 0.2f;
	const float pathUpdateMoveThreshold = 0.5f;

	public Transform target;
	public bool followingPath = false;
	public float speed = 20;
	public float turnSpeed = 3;
	public float turnDst = 5;
	public float stoppingDst = 10;

	Path path;

	void Start() {
		// StartCoroutine (UpdatePath ());
	}

	public void OnPathFound(Vector3[] waypoints, bool pathsuccessful) {
		if (pathsuccessful) {
			path = new Path(waypoints, transform.position, turnDst, stoppingDst);

			StopCoroutine ("FollowPath");
			StartCoroutine ("FollowPath");
		}
	}

	IEnumerator UpdatePath() {
		if (Time.timeSinceLevelLoad < 0.3f) {
			yield return new WaitForSeconds (0.3f);
		}

		PathRequestManager.RequestPath (new PathRequest( transform.position, target.position, OnPathFound));

		float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
		Vector3 targetPosOld = target.position;

		while (true) {
			yield return new WaitForSeconds (minPathUpdateTime);
			if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold) {
				PathRequestManager.RequestPath (new PathRequest (transform.position, target.position, OnPathFound));
				targetPosOld = target.position;
			}
		}
	}

	IEnumerator FollowPath() {
		followingPath = true;
		int pathIndex = 0;
		transform.LookAt (path.lookPoints [0]);

		float speedPercent = 1;

		while (followingPath) {
			Vector2 pos2D = new Vector2 (transform.position.x, transform.position.z);
			while (path.turnBoundries [pathIndex].HasCrossedLine (pos2D)) {
				if (pathIndex == path.finishLineIndex) {
					followingPath = false;
					break;
				} else {
					pathIndex++;
				}
			}

			if (followingPath) {
				if (pathIndex >= path.slowDownIndex && stoppingDst > 0) {
					speedPercent = Mathf.Clamp01 (path.turnBoundries [path.finishLineIndex].DistanceFromPoint (pos2D) / stoppingDst);
					if (speedPercent < 0.01) {
						followingPath = false;
					}
				}

				Quaternion targetRot = Quaternion.LookRotation (path.lookPoints [pathIndex] - transform.position);
				transform.rotation = Quaternion.Lerp (transform.rotation, targetRot, Time.deltaTime * turnSpeed);
				transform.Translate (Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
			}

			yield return null;
		}
	}

	public void OnDrawGizmos() {
		if(path != null) {
			path.DrawWithGizmos ();
		}
	}
}
