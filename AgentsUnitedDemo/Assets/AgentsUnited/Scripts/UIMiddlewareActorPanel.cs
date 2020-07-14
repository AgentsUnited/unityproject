using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class UIMiddlewareActorPanel : MonoBehaviour {

	public Transform buttonsRoot;// { get; private set; }
	public UIMiddlewareMoveButton buttonPrefab;// { get; private set; }
    public UIMiddlewareMoveInputField inputFieldPrefab;
	public Text actorNameLabel;

	public Toggle thisControlsActorToggle;

	public UIMiddlewareMoves.UIProtocolActor actor { get; private set; }

    private List<UIMiddlewareMoveButton> moveButtons;

    private List<UIMiddlewareMoveInputField> moveInputFields;

    public UIMiddlewareCustomActionsPanel customActionsPanel;

    void Awake()
    {
        moveButtons = new List<UIMiddlewareMoveButton>();
        moveInputFields = new List<UIMiddlewareMoveInputField>();
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

    void ClearUnsetButtons(UIMiddlewareMoveButton[] keep)
    {
        foreach (UIMiddlewareMoveButton button in moveButtons)
        {
            if (!keep.Contains(button))
            {
                Destroy(button.gameObject);
            }
        }
        moveButtons.Clear();
        foreach (UIMiddlewareMoveButton button in keep)
        {
            moveButtons.Add(button);
        }
    }

    void ClearUnsetInputFields(UIMiddlewareMoveInputField[] keep)
    {
        foreach (UIMiddlewareMoveInputField f in moveInputFields)
        {
            if (!keep.Contains(f))
            {
                Destroy(f.gameObject);
            }
        }
        moveInputFields.Clear();
        foreach (UIMiddlewareMoveInputField f in keep)
        {
            moveInputFields.Add(f);
        }
    }

    public void SetMoveSet(UIMiddlewareMoves.MoveSet moveSet)
    {
        List<UIMiddlewareMoveButton> buttonsKeep = new List<UIMiddlewareMoveButton>();
        List<UIMiddlewareMoveInputField> inputFieldsKeep = new List<UIMiddlewareMoveInputField>();
        if (moveSet != null)
        { 
            foreach (UIMiddlewareMoves.Move move in moveSet.moves)
            {
                if (!move.requestUserInput)
                {
                    buttonsKeep.Add(SetMoveButton(move));
                }
                else
                {
                    inputFieldsKeep.Add(SetMoveInputField(move));
                }
            }
        }
        ClearUnsetButtons(buttonsKeep.ToArray());
        ClearUnsetInputFields(inputFieldsKeep.ToArray());
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
        //wtf is going on here...? why iterate over all UI items each time we add a new one?
        bool controlled = actor.controlledBy.Contains(UIMiddlewareMoves.INSTANCE.uiId);
		foreach (UIMiddlewareMoveButton moveButton in moveButtons) {
			moveButton.SetControlled(controlled);
		}
		return button;
	}

    public UIMiddlewareMoveInputField SetMoveInputField(UIMiddlewareMoves.Move move)
    {
        UIMiddlewareMoveInputField inputField = moveInputFields.FirstOrDefault(f => f.move.moveID == move.moveID);
        if (inputField == null)
        {
            inputField = GameObject.Instantiate(inputFieldPrefab.gameObject).GetComponent<UIMiddlewareMoveInputField>();
            inputField.transform.SetParent(buttonsRoot);
            inputField.transform.localPosition = Vector3.zero;
            inputField.transform.localRotation = Quaternion.identity;
            inputField.transform.localScale = Vector3.one;
            inputField.transform.name = move.moveID;
            moveInputFields.Add(inputField);
        }
        inputField.SetMove(move);
        bool controlled = actor.controlledBy.Contains(UIMiddlewareMoves.INSTANCE.uiId);
        foreach (UIMiddlewareMoveInputField moveInputField in moveInputFields)
        {
            moveInputField.SetControlled(controlled);
        }
        return inputField;
    }

    public void SetControlledState(bool controlled) {
		this.thisControlsActorToggle.onValueChanged.RemoveAllListeners();

		thisControlsActorToggle.isOn = controlled;
		thisControlsActorToggle.interactable = true;

        foreach (UIMiddlewareMoveButton moveButton in moveButtons)
        {
            moveButton.SetControlled(controlled);
        }

        foreach (UIMiddlewareMoveInputField moveInputField in moveInputFields)
        {
            moveInputField.SetControlled(controlled);
        }

        this.thisControlsActorToggle.onValueChanged.AddListener(OnSetControlled);
	}

}
