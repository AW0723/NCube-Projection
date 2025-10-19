using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public NCubeController nCubeController;
    public GameObject mainCamera;
    public int initialDimension = 7;

    // Cube settings
    public TMP_Dropdown dimensionDropdown;

    // Translation
    public TMP_Dropdown translationAxisDropdown;
    public Slider translationSlider;

    // Rotation
    public GameObject rotationParent;
    public TMP_Dropdown rotationAxisADropdown;
    public TMP_Dropdown rotationAxisBDropdown;

    private bool positiveRotateHeldDown;
    private bool negativeRotateHeldDown;

    public float keySensitivity = 1.5f;
    public float scrollSensitivity = 20f;

    private const int MAX_DIMENSION = 10;

    // Start is called before the first frame update
    void Start()
    {
        PopulateDimensionDropdown();
        SwitchToDimension(initialDimension);
    }

    private void PopulateDimensionDropdown()
    {
        dimensionDropdown.ClearOptions();
        List<string> options = new();
        for (int i = 3; i <= MAX_DIMENSION; i++)
        {
            options.Add(i.ToString());
        }
        dimensionDropdown.AddOptions(options);
        dimensionDropdown.value = initialDimension - 3;
    }

    private void PopulateDropdowns(int dimension)
    {
        translationAxisDropdown.ClearOptions();
        rotationAxisADropdown.ClearOptions();
        rotationAxisBDropdown.ClearOptions();

        List<string> options = new();
        for (int i = 1; i <= dimension; i++)
        {
            options.Add(i.ToString());
        }
        translationAxisDropdown.AddOptions(options);
        rotationAxisADropdown.AddOptions(options);
        rotationAxisBDropdown.AddOptions(options);

        translationAxisDropdown.value = options.Count - 1;
        rotationAxisADropdown.value = 0;
        rotationAxisBDropdown.value = options.Count - 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || negativeRotateHeldDown)
        {
            PerformRotation(-keySensitivity * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || positiveRotateHeldDown)
        {
            PerformRotation(keySensitivity * Time.deltaTime);
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float moveDistance = Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity * Time.deltaTime;
            Vector3 finalPos = mainCamera.transform.position + mainCamera.transform.forward * moveDistance;
            if (finalPos.magnitude > 2)
            {
                mainCamera.transform.position = finalPos;
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomizeRotation();
        }
    }

    public void OnDimensionDropdownChanged(int value)
    {
        int dimension = int.Parse(dimensionDropdown.options[dimensionDropdown.value].text);
        SwitchToDimension(dimension);
    }

    private void SwitchToDimension(int dimension)
    {
        nCubeController.SetupWithDimension(dimension);
        PopulateDropdowns(dimension);
    }

    public void OnTranslationAxisChanged(int value)
    {
        translationSlider.value = nCubeController.Origin[value];
    }

    public void PerformTranslation(float amount)
    {
        nCubeController.SetTranslation(translationAxisDropdown.value + 1, amount);
        nCubeController.FindIntersection();
    }

    private void PerformRotation(float amount)
    {
        if (rotationAxisADropdown.value != rotationAxisBDropdown.value)
        {
            nCubeController.Rotate(rotationAxisADropdown.value + 1, rotationAxisBDropdown.value + 1, amount);
            nCubeController.FindIntersection();
        }
    }

    public void RandomizeRotation()
    {
        nCubeController.RandomizeRotation();
        nCubeController.FindIntersection();
    }

    public void ResetTranslation()
    {
        nCubeController.ResetTranslation();
        nCubeController.FindIntersection();
        translationSlider.value = 0;
    }

    public void OnPositiveRotateButtonDown() { positiveRotateHeldDown = true; }
    public void OnPositiveRotateButtonUp() { positiveRotateHeldDown = false; }
    public void OnNegativeRotateButtonDown() { negativeRotateHeldDown = true; }
    public void OnNegativeRotateButtonUp() { negativeRotateHeldDown = false; }
}
