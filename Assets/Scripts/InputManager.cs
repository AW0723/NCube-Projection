using System.Collections.Generic;
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

    public float keySensitivity = 1.5f;
    public float scrollSensitivity = 20f;

    private TransformationMethod SelectedMethod;
    private int SelectedAxis;

    // Start is called before the first frame update
    void Start()
    {
        SetTranslationAxis(nCubeController.dimension);
        SelectedMethod = TransformationMethod.Rotation;

        rotationAxisADropdown.ClearOptions();
        rotationAxisBDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 1; i <= nCubeController.dimension; i++)
        {
            options.Add(i.ToString());
        }
        rotationAxisADropdown.AddOptions(options);
        rotationAxisBDropdown.AddOptions(options);
        rotationAxisBDropdown.value = options.Count - 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (SelectedMethod == TransformationMethod.Translation)
            {
                SetTranslationAxis(SelectedAxis + 1);
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (SelectedMethod == TransformationMethod.Translation)
            {
                SetTranslationAxis(SelectedAxis - 1);
            }
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            PerformTransformation(-keySensitivity * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            PerformTransformation(keySensitivity * Time.deltaTime);
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            PerformTransformation(Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomizeRotation();
        }
    }

    public void OnTransformationMethodChanged()
    {
        switch (transformationMethodDropdown.value)
        {
            case 0:
                SelectedMethod = TransformationMethod.Rotation;
                translationParent.SetActive(false);
                rotationParent.SetActive(true);
                break;
            case 1:
                SelectedMethod = TransformationMethod.Translation;
                translationParent.SetActive(true);
                rotationParent.SetActive(false);
                break;
        }
    }

    private void PerformTransformation(float amount)
    {
        switch (SelectedMethod)
        {
            case TransformationMethod.Translation:
                nCubeController.Translate(SelectedAxis, amount);
                break;
            case TransformationMethod.Rotation:
                if (rotationAxisADropdown.value != rotationAxisBDropdown.value)
                {
                    nCubeController.Rotate(rotationAxisADropdown.value + 1, rotationAxisBDropdown.value + 1, amount);
                }
                break;
        }
    }

    private void RandomizeRotation()
    {
        for (int i = 1; i < nCubeController.dimension; i++)
        {
            for (int j = i + 1; j <= nCubeController.dimension; j++)
            {
                if (i == j) { continue; }
                nCubeController.Rotate(i, j, Random.Range(0, 2 * Mathf.PI));
            }
        }
    }

    private void SetTranslationAxis(int axis)
    {
        if (axis > 0 && axis <= nCubeController.dimension)
        {
            SelectedAxis = axis;
            translationAxisText.text = "Current Axis: " + axis;
        }
    }

    private enum TransformationMethod
    {
        Translation,
        Rotation,
    }
}
