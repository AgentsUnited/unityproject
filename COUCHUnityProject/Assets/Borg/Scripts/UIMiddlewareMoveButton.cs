using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMiddlewareMoveButton : MonoBehaviour {

	public string moveID { get; private set; }
	public UIMiddlewareMoves.Move move { get; private set; }
	public Button button;
	public Text buttonLabel;
    public Text statusLabel;


    public void SetStatus(string status) {
        statusLabel.text = status;
    }

	public void SetIsFallback() {

	}

	public void SetIsActive() {

	}

	public void SetControlled(bool controlled) {
		button.interactable = controlled;
	}

	public void SetIsCompleted() {

	}

	public void SetDefault() {
		buttonLabel.text = move.moveID+"\n"+move.opener;
        statusLabel.text = "";
        button.interactable = true;
	}

	public void SetMove(UIMiddlewareMoves.Move move) {
		this.move = move;
		SetDefault();

		this.button.onClick.RemoveAllListeners();
		this.button.onClick.AddListener(delegate {
            StartCoroutine(DebounceButton(0.5F));
            UIMiddlewareMoves.INSTANCE.OnMoveButtonClicked(move);
		});
	}

    IEnumerator DebounceButton(float waitTime) {
        button.interactable = false;
        yield return new WaitForSeconds(waitTime);
        button.interactable = true;
    }
}
