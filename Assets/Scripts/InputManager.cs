using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public NCubeController nCubeController;
    public GameObject mainCamera;

    // Translation
    public TMP_Dropdown translationAxisDropdown;
    public Slider translationSlider;

    // Rotation
    public GameObject rotationParent;
    public TMP_Dropdown rotationAxisADropdown;
    public TMP_Dropdown rotationAxisBDropdown;

    public float keySensitivity = 1.5f;
    public float scrollSensitivity = 20f;

    // Start is called before the first frame update
    void Start()
    {
        PopulateDropdowns();
    }

    private void PopulateDropdowns()
    {
        translationAxisDropdown.ClearOptions();
        rotationAxisADropdown.ClearOptions();
        rotationAxisBDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 1; i <= nCubeController.dimension; i++)
        {
            options.Add(i.ToString());
        }
        translationAxisDropdown.AddOptions(options);
        rotationAxisADropdown.AddOptions(options);
        rotationAxisBDropdown.AddOptions(options);

        translationAxisDropdown.value = 0;
        rotationAxisADropdown.value = 0;
        rotationAxisBDropdown.value = options.Count - 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            PerformRotation(-keySensitivity * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
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
        nCubeController.FindIntersection();
    }
}
