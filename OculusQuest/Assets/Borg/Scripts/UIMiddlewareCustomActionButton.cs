using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMiddlewareCustomActionButton : MonoBehaviour {

    public string command { get; private set; }
    public UIMiddlewareMoves.UIProtocolCustomAction action { get; private set; }
    public Button button;
    public Text buttonLabel;

    private UIMiddlewareMoves.UIProtocolActor actor;

    public void SetDefault() {
        buttonLabel.text = action.description;
        button.interactable = true;
    }

    public void SetAction(UIMiddlewareMoves.UIProtocolCustomAction action, UIMiddlewareMoves.UIProtocolActor actor) {
        this.actor = actor;
        this.action = action;
        SetDefault();

        this.button.onClick.RemoveAllListeners();
        this.button.onClick.AddListener(delegate {
            UIMiddlewareMoves.INSTANCE.OnCustomActionClicked(action, actor);
        });
    }
}
