using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {

	public void loadLevel (string lvl)
    {
        Application.LoadLevel(lvl);
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
