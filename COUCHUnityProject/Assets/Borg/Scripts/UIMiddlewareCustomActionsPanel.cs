using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class UIMiddlewareCustomActionsPanel : MonoBehaviour {

    public Transform buttonsRoot;// { get; private set; }
    public UIMiddlewareCustomActionButton buttonPrefab;// { get; private set; }


    private List<UIMiddlewareCustomActionButton> customActionButtons;

    void Awake() {
        customActionButtons = new List<UIMiddlewareCustomActionButton>();
    }

    void ClearUnsetButtons(UIMiddlewareCustomActionButton[] keep) {
        foreach (UIMiddlewareCustomActionButton button in customActionButtons) {
            if (!keep.Contains(button)) {
                Destroy(button.gameObject);
            }
        }
        customActionButtons.Clear();
        foreach (UIMiddlewareCustomActionButton button in keep) {
            customActionButtons.Add(button);
        }
    }

    public UIMiddlewareCustomActionButton[] SetCustomActions(UIMiddlewareMoves.UIProtocolCustomAction[] actions, UIMiddlewareMoves.UIProtocolActor actor) {
        List<UIMiddlewareCustomActionButton> buttonsKeep = new List<UIMiddlewareCustomActionButton>();
        if (actions == null) {
            ClearUnsetButtons(buttonsKeep.ToArray());
            return buttonsKeep.ToArray();
        }
        foreach (UIMiddlewareMoves.UIProtocolCustomAction move in actions) {
            buttonsKeep.Add(SetActionButton(move, actor));
        }
        UIMiddlewareCustomActionButton[] res = buttonsKeep.ToArray();
        ClearUnsetButtons(res);
        return res;
    }

    public UIMiddlewareCustomActionButton SetActionButton(UIMiddlewareMoves.UIProtocolCustomAction action, UIMiddlewareMoves.UIProtocolActor actor) {
        UIMiddlewareCustomActionButton button = customActionButtons.FirstOrDefault(b => b.action.command == action.command);
        if (button == null) {
            button = GameObject.Instantiate(buttonPrefab.gameObject).GetComponent<UIMiddlewareCustomActionButton>();
            button.transform.SetParent(buttonsRoot);
            button.transform.localPosition = Vector3.zero;
            button.transform.localRotation = Quaternion.identity;
            button.transform.localScale = Vector3.one;
            button.transform.name = action.command;
            customActionButtons.Add(button);
        }

        button.SetAction(action, actor);
        return button;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
