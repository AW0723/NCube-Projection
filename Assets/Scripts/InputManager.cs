using TMPro;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public NCubeController nCubeController;
    public TMP_Dropdown transformationMethodDropdown;

    // Translation
    public GameObject translationParent;
    public TMP_Text translationAxisText;

    // Rotation
    public GameObject rotationParent;
    public TMP_Dropdown rotationAxisADropdown;
    public TMP_Dropdown rotationAxisBDropdown;

    public float keySensitivity = 0.1f;
    public float scrollSensitivity = 0.1f;

    private TransformationMethod selectedMethod;
    private int selectedAxis;

    // Start is called before the first frame update
    void Start()
    {
        SetTranslationAxis(1);


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SetTranslationAxis(selectedAxis + 1);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SetTranslationAxis(selectedAxis - 1);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            //nCubeController.Translate(selectedAxis, -keySensitivity);
            nCubeController.Rotate(2, 3, -0.01f);

        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            //nCubeController.Translate(selectedAxis, keySensitivity);
            nCubeController.Rotate(1, 3, 0.01f);
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            nCubeController.Translate(selectedAxis, Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity);
        }
    }

    public void OnTransformationMethodChanged()
    {
        switch (transformationMethodDropdown.value)
        {
            case 0:
                selectedMethod = TransformationMethod.Translation;
                translationParent.SetActive(true);
                rotationParent.SetActive(false);
                break;
            case 1:
                selectedMethod = TransformationMethod.Rotation;
                translationParent.SetActive(false);
                rotationParent.SetActive(true);
                break;
        }
    }

    private void SetTranslationAxis(int axis)
    {
        if (axis > 0 && axis <= nCubeController.dimension)
        {
            selectedAxis = axis;
            translationAxisText.text = "Current Axis: " + axis;
        }
    }

    private enum TransformationMethod
    {
        Translation,
        Rotation,
    }
}
