using UnityEngine;
using System.Collections;

public class BaseGameEntity : MonoBehaviour {

	// every entity has a unique identifying number
	private int m_ID;

	static int m_iNextValID = 0;

	public int ID() {
		return(m_ID);
	}


	public void Start () {
		Debug.Log ("Start in BaseGameEntity");
		SetID (m_iNextValID);
	}
	
	private void SetID(int val) {
		if (val >= m_iNextValID) {
			m_ID = val;
			m_iNextValID++;
		}
	}

}
