using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {

	[SerializeField] private float m_maxDistance = 10.0f;

	private Unit m_unit;
	private Vector3 m_myWaypoint = Vector3.zero;

	void Awake() {
		m_unit = GetComponent<Unit> ();
	}

	void Start() {
		m_myWaypoint = GetNextWaypoint ();
		PathRequestManager.RequestPath(new PathRequest(transform.position, m_myWaypoint, OnMyPathFound));
	}

	void Update() {
		if (!m_unit.followingPath) {
			m_myWaypoint = GetNextWaypoint ();
			PathRequestManager.RequestPath(new PathRequest(transform.position, m_myWaypoint, OnMyPathFound));
		}
	}

	public void OnMyPathFound(Vector3[] waypoints, bool pathsuccessful) {
		if (pathsuccessful) {
			m_unit.OnPathFound (waypoints, pathsuccessful);
		} else {
			m_myWaypoint = GetNextWaypoint ();
			PathRequestManager.RequestPath(new PathRequest(transform.position, m_myWaypoint, OnMyPathFound));
		}
	}

	private Vector3 GetNextWaypoint() {
		Vector3 randPos = transform.position + Random.insideUnitSphere * m_maxDistance;
		randPos.y = 1f;

		return randPos;
	}
}
