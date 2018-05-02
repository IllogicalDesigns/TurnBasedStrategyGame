using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
	public int ownerNumber = 1;
	public Color playerColor;
	public List<TBSUnit> activeUnits = new List<TBSUnit> ();
	[SerializeField] List<TBSUnit> allActiveUnits = new List<TBSUnit> ();
	public TBSUnit activatedUnit;

	public GameObject targetCircle;
	//SpriteRenderer m_SelectCircleSR;
	public GameObject targetCross;
	SpriteRenderer m_TarXSR;

	public Button confirmButton;
	public Button chargeButton;
	public Slider moveLeftSlider;
	//[SerializeField] List<Button> selectButtons = new List<Button> ();

	public GameObject arrowBody;
	public GameObject arrowHead;
	public GameObject gridObject;
	TurnController myTurnManager;

	public Material activeColor;
	public Material deactiveColor;
	[SerializeField] GameObject LostBanner;

	// Use this for initialization
	void Start()
	{
		foreach (TBSUnit u in gameObject.GetComponentsInChildren<TBSUnit>()) {
			allActiveUnits.Add (u);
		}
		activeUnits = new List<TBSUnit> (allActiveUnits);

		//m_SelectCircleSR = targetCircle.GetComponent<SpriteRenderer> ();
		m_TarXSR = targetCross.GetComponent<SpriteRenderer> ();
		myTurnManager = GameObject.FindObjectOfType<TurnController> ();
	}

	void moveSelectSrpite(Vector3 Pos, bool enabled, Color clr, GameObject obj)
	{
		//m_SelectCircleSR.color = clr;
		obj.transform.position = Pos;
		obj.SetActive (enabled);
	}

	void checkIfWeLost () {
		if (allActiveUnits.Count <= 0) {
			LostBanner.SetActive (true);
			Time.timeScale = 0.1f;
		}
	}

	public bool isItOurTurn (){
		if(ownerNumber != myTurnManager.playerTurnNumber)
			return false;
		else
			return true;
	}

	public void updateMoveLeftSlider(int moveAmt){
		moveLeftSlider.value = moveAmt;
	}

	public void PassTurn () {
		//activeUnits.Remove (activatedUnit);
		if(activatedUnit!= null)
		activatedUnit.movementLeft = 0;
		activatedUnit = null;
		targetCircle.SetActive (false);
		targetCross.SetActive (false);
		confirmButton.gameObject.SetActive (false);
        if(chargeButton != null)
		chargeButton.gameObject.SetActive (false);

		if(activeUnits.Count <= 0)
			activeUnits = new List<TBSUnit> (allActiveUnits);

		foreach (TBSUnit u in allActiveUnits) {
			u.gameObject.GetComponent<Renderer> ().material = deactiveColor;
		}

		foreach (TBSUnit u in activeUnits) {
			u.gameObject.GetComponent<Renderer> ().material = activeColor;
			u.chargedDuringAction = false;
			u.movementLeft = u.movementMax;
			u.destroyOldChargeArrows ();
			if (!u.gameObject.activeInHierarchy)
				activeUnits.Remove (u);
		}
	}

	public void setTarCircle(Vector3 Pos, bool enabled)
	{
		moveSelectSrpite (Pos, enabled, playerColor, targetCircle);
	}

	public void setTarCross(Vector3 Pos, bool enabled)
	{
		moveSelectSrpite (Pos, enabled, playerColor, targetCross);
	}

	public void setConfirmButton(TBSUnit caller, bool enabled)
	{
		confirmButton.gameObject.SetActive (enabled);
		confirmButton.onClick.RemoveAllListeners ();
		confirmButton.onClick.AddListener (caller.MoveUnitToConfirmedMovement);
	}

	public void setChargeButton(TBSUnit caller, bool enabled)
	{
        if (chargeButton != null)
        {
            chargeButton.gameObject.SetActive(enabled);
            chargeButton.onClick.RemoveAllListeners();
            chargeButton.onClick.AddListener(caller.displayActionGUIHelpers);
        }
	}

	public void RemoveDeadUnit (TBSUnit deadUnit) {
		allActiveUnits.Remove (deadUnit);
		activeUnits.Remove (deadUnit);
		checkIfWeLost ();
	}
}
