using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMiddlewareMoveInputField : MonoBehaviour
{

    public string moveID { get; private set; }
    public UIMiddlewareMoves.Move move { get; private set; }
    public InputField inputField;

    public void Start()
    {
    }

    public void SetStatus(string status)
    {
    }

    public void SetControlled(bool controlled)
    {
        inputField.enabled = controlled;
    }

    public void SetIsCompleted()
    {

    }

    public void SetDefault()
    {
        inputField.placeholder.GetComponent<Text>().text = move.moveID + ": enter " + move.storeInSKBVariable;
    }

    public void SetMove(UIMiddlewareMoves.Move move)
    {
        this.move = move;
        SetDefault();
        inputField.onEndEdit.AddListener(delegate { SendUserInput(); });
    }

    public void SendUserInput()
    {
        if (inputField.text != "" && Input.GetKeyDown(KeyCode.Return))
        {
            move.userInput = inputField.text;
            UIMiddlewareMoves.INSTANCE.OnMoveButtonClicked(move);
            inputField.text = "";
        }
    }
    
}
