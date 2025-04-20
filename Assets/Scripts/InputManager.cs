using TMPro;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public NCubeController nCubeController;
    public TMP_Text text;

    public float keySensitivity = 0.1f;
    public float scrollSensitivity = 0.1f;

    private int selectedAxis;

    // Start is called before the first frame update
    void Start()
    {
        SetAxis(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SetAxis(selectedAxis + 1);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SetAxis(selectedAxis - 1);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            nCubeController.Translate(selectedAxis, -keySensitivity);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            nCubeController.Translate(selectedAxis, keySensitivity);
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            nCubeController.Translate(selectedAxis, Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity);
        }
    }

    private void SetAxis(int axis)
    {
        if (axis > 0 && axis <= nCubeController.dimension)
        {
            selectedAxis = axis;
            text.text = "Current Axis: " + axis;
        }
    }
}
