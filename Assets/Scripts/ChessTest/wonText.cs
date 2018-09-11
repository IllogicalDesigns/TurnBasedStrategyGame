using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class wonText : MonoBehaviour {
    public Text m_MyText;
    [SerializeField] GameObject thinking;
    [SerializeField] GameObject playerUI;

    public void WhoWon (string winner) {
        m_MyText.text = winner + " has won";
        Destroy(thinking);
        Destroy(playerUI);
    }
}
