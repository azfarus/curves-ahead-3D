using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RopeUIController : MonoBehaviour
{
    [Header("Reference")] 
    [SerializeField] public GameObject curveGenerator;
    private ParametricRopeMesh ropeScript;

    [Header("Basic Settings")]
    public Slider radiusSlider;
    public Slider lengthSlider;

    [Header("Coefficient Lists (Set 3 sliders each)")]
    public Slider[] coeffASliders; 
    public Slider[] coeffBSliders;

    [Header("Value Display")]
    [SerializeField] private TMP_Text valueText;

    public static bool isMenuOpen = false;

    void Start()
    {   
        ropeScript = curveGenerator.GetComponent<ParametricRopeMesh>();
        if (ropeScript == null) return;

        // Initialize sliders
        radiusSlider.value = ropeScript.ropeRadius;
        lengthSlider.value = ropeScript.curveLength;

        for (int i = 0; i < 3; i++)
        {
            int index = i;

            coeffASliders[i].value = ropeScript.coeffA[i];
            coeffBSliders[i].value = ropeScript.coeffB[i];

            coeffASliders[i].onValueChanged.AddListener((val) =>
            {
                ropeScript.coeffA[index] = val;
                UpdateValueText();
            });

            coeffBSliders[i].onValueChanged.AddListener((val) =>
            {
                ropeScript.coeffB[index] = val;
                UpdateValueText();
            });
        }

        radiusSlider.onValueChanged.AddListener((val) =>
        {
            ropeScript.ropeRadius = val;
            UpdateValueText();
        });

        lengthSlider.onValueChanged.AddListener((val) =>
        {
            ropeScript.curveLength = val;
            UpdateValueText();
        });

        // Initial display
        UpdateValueText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isMenuOpen = !isMenuOpen;

            Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isMenuOpen;
        }
    }

    void UpdateValueText()
    {
        if (valueText == null || ropeScript == null) return;

        valueText.text =
            $"Radius: {ropeScript.ropeRadius:0.00}\n" +
            $"Length: {ropeScript.curveLength:0.00}\n\n" +

            $"Coeff A:      " + $"Coeff B:\n"+
            $"A0: {ropeScript.coeffA[0]:0.00}" + $"     B0: {ropeScript.coeffB[0]:0.00}\n" +
            $"A1: {ropeScript.coeffA[1]:0.00}" + $"     B1: {ropeScript.coeffB[1]:0.00}\n" +
            $"A2: {ropeScript.coeffA[2]:0.00}" + $"     B2: {ropeScript.coeffB[2]:0.00}";


    }

    void RequestRegen()
    {
        ropeScript.GenerateRope();
    }
}