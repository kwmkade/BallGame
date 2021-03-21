using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public bool IsClicked { set; get; }

    void Start()
    {
        IsClicked = false;
    }

    public void OnClick()
    {
        IsClicked = true;
    }
}
