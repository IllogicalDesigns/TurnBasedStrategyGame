using UnityEngine;
using UnityEngine.UI;

public class NodeFlagHelpers : MonoBehaviour {
    [SerializeField] Text weightText;
    [SerializeField] GameObject Camera;

    // Use this for initialization
    void Start () {
        //SetWeightText("Default Text", Color.white);
        Camera = GameObject.Find("Main Camera");
    }

    public void SetWeightText (string txt, Color clr)
    {
        Camera = GameObject.Find("Main Camera");
        weightText.text = txt;
        weightText.color = clr;
        weightText.transform.LookAt(2 * transform.position - Camera.transform.position);
    }
}
