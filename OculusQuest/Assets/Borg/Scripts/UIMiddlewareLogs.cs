using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIMiddlewareLogs : MonoBehaviour {

    public Text logContainer;
    public ScrollRect scrollRect;

    public void Log(UIMiddlewareMoves.UIProtocolLogMessage logMsg) {
        logContainer.text = logContainer.text + "\n" + logMsg.logString;
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
}
