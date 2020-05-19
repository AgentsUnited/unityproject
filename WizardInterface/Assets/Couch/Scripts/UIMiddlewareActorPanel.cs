using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class UIMiddlewareActorPanel : MonoBehaviour {

	public Transform buttonsRoot;// { get; private set; }
	public UIMiddlewareMoveButton buttonPrefab;// { get; private set; }
	public Text actorNameLabel;

	public Toggle thisControlsActorToggle;

	public UIMiddlewareMoves.UIProtocolActor actor { get; private set; }

	private List<UIMiddlewareMoveButton> moveButtons;

    public UIMiddlewareCustomActionsPanel customActionsPanel;

    void Awake() {
		moveButtons = new List<UIMiddlewareMoveButton>();
	}

	private void OnSetControlled(bool value) {
		this.thisControlsActorToggle.interactable = false;
		UIMiddlewareMoves.INSTANCE.OnSetControlled(this.actor, value);
	}

	public void SetActorState(UIMiddlewareMoves.UIProtocolActor actor) {
		this.actor = actor;
		SetControlledState(actor.controlledBy.Contains(UIMiddlewareMoves.INSTANCE.uiId));
        SetCustomActions(actor.customActions);
		// TODO: set label, etc. (this does not include the moves buttons)
		actorNameLabel.text = actor.identifier;
		// TODO: set button states...
	}

    public void SetMoveButtonStatus(string moveId, string status) {
        UIMiddlewareMoveButton mb = moveButtons.FirstOrDefault(b => b.move.moveID == moveId);
        if (mb != null) mb.SetStatus(status);
    }

    private void SetCustomActions(UIMiddlewareMoves.UIProtocolCustomAction[] customActions) {
        customActionsPanel.SetCustomActions(customActions, actor);
    }

    void ClearUnsetButtons(UIMiddlewareMoveButton[] keep) {
		foreach (UIMiddlewareMoveButton button in moveButtons) {
            if (!keep.Contains(button)) {
				Destroy(button.gameObject);
			}
        }
        moveButtons.Clear();
		foreach (UIMiddlewareMoveButton button in keep) {
			moveButtons.Add(button);
		}
	}

	public UIMiddlewareMoveButton[] SetMoveSet(UIMiddlewareMoves.MoveSet moveSet) {
        List<UIMiddlewareMoveButton> buttonsKeep = new List<UIMiddlewareMoveButton>();
		if (moveSet == null) {
			ClearUnsetButtons(buttonsKeep.ToArray());
			return buttonsKeep.ToArray();
		}
        foreach (UIMiddlewareMoves.Move move in moveSet.moves) {
            buttonsKeep.Add(SetMoveButton(move));
        }
		UIMiddlewareMoveButton[] res = buttonsKeep.ToArray();
		ClearUnsetButtons(res);
		return res;
	}

	public UIMiddlewareMoveButton SetMoveButton(UIMiddlewareMoves.Move move) {
		UIMiddlewareMoveButton button = moveButtons.FirstOrDefault(b => b.move.moveID == move.moveID);
		if (button == null) {
			button = GameObject.Instantiate(buttonPrefab.gameObject).GetComponent<UIMiddlewareMoveButton>();
			button.transform.SetParent(buttonsRoot);
			button.transform.localPosition = Vector3.zero;
			button.transform.localRotation = Quaternion.identity;
            button.transform.localScale = Vector3.one;
			button.transform.name = move.moveID;
			moveButtons.Add(button);
		}
		button.SetMove(move);
		bool controlled = actor.controlledBy.Contains(UIMiddlewareMoves.INSTANCE.uiId);
		foreach (UIMiddlewareMoveButton moveButton in moveButtons) {
			moveButton.SetControlled(controlled);
		}
		return button;
	}

	public void SetControlledState(bool controlled) {
		this.thisControlsActorToggle.onValueChanged.RemoveAllListeners();

		thisControlsActorToggle.isOn = controlled;
		thisControlsActorToggle.interactable = true;

		foreach (UIMiddlewareMoveButton moveButton in moveButtons) {
			moveButton.SetControlled(controlled);
		}

		this.thisControlsActorToggle.onValueChanged.AddListener(OnSetControlled);
	}

}
