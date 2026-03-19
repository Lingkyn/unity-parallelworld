using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Button startBtn;      
    public RawImage startImg;    
    public RawImage mainImg;     

    void Start()
    {
        startBtn = GameObject.Find("BTN_ENTER").GetComponent<Button>();
        startImg = GameObject.Find("START").GetComponent<RawImage>();
        mainImg = GameObject.Find("MAIN").GetComponent<RawImage>();

        startBtn.onClick.AddListener(startBtnClick);
    }

    public void startBtnClick()
    {
        mainImg.transform.position = startImg.transform.position;
    }

    void Update()
    {

    }
}

