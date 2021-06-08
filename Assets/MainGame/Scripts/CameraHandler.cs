using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour {

    public Transform target = null;
    public Vector3 overviewPos = Vector3.zero;
    public float overviewSize = 0;
    public float yPos = 20f;

    [SerializeField]
    private float m_followSpeed = 1f;
    [SerializeField]
    private float m_viewSwitchSpeed = 10f;

    private Camera m_mainCam = null;
    private Vector3 m_targetPosition = Vector3.zero;
    private float m_oriCamSize = -1;
    private bool m_inOverview = false;

    void Start () {
        m_mainCam = GetComponent<Camera>();

        yPos = transform.position.y;
        m_oriCamSize = m_mainCam.orthographicSize;
    }
	
	void Update () {
        if (!m_inOverview) {
            if (target) {
                m_targetPosition = new Vector3(target.position.x, yPos, target.position.z);
                transform.position = Vector3.Lerp(transform.position, m_targetPosition, m_followSpeed * Time.deltaTime);
                m_mainCam.orthographicSize = Mathf.Lerp(m_mainCam.orthographicSize, m_oriCamSize, m_followSpeed * Time.deltaTime);
            }
        } else {
            transform.position = Vector3.Lerp(transform.position, overviewPos, m_viewSwitchSpeed * Time.deltaTime);
            m_mainCam.orthographicSize = Mathf.Lerp(m_mainCam.orthographicSize, overviewSize, m_viewSwitchSpeed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            m_inOverview = !m_inOverview;
        }
    }

    public void SetCameraPos (int x, int z) {
        transform.position = new Vector3(x, yPos, z);
    }
}