using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour {
    public Vector3 TragetPos;
    NavMeshAgent m_Agent;

	// Use this for initialization
	void Start () 
    {
        m_Agent = GetComponent<NavMeshAgent>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        m_Agent.destination = TragetPos;
	}
}
