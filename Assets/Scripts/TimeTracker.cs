using UnityEngine;
using TMPro; // Required for TextMeshPro

public class CurveTimer : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Curve Generator Object")]
    [SerializeField] private GameObject curveGeneratorGameObject; 
    
    // Replace 'RopeGenerator' with the actual name of your script
    private CrosshairDetection curveScript;

    private float elapsedTime = 0f;
    private bool isTimerRunning = false;
    private bool timerFinished = false;

    void Start()
    {
        if (curveGeneratorGameObject != null)
        {
            curveScript = curveGeneratorGameObject.GetComponent<CrosshairDetection>();
        }

        if (timerText != null)
        {
            timerText.text = "0.00";
        }
    }

    void Update()
    {
        if (timerFinished) return;
        if (RopeUIController.isMenuOpen)
        {
            elapsedTime = 0f;
            UpdateDisplay();
            return;
        }

        int index = curveScript.CurveIndex;

        // Start condition: index > 0 and not already running
        if (index > 2 && index < curveScript.RopeMesh.curveSegments)
        {
            isTimerRunning = true;
        }

        // End condition: index reaches or exceeds 200
        if (index >= curveScript.RopeMesh.curveSegments-1)
        {
            isTimerRunning = false;
            timerFinished = true;
        }

        // Timer Logic
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {

        // Formats the float to 2 decimal places (e.g., 12.34)
        timerText.text = elapsedTime.ToString("F2");
 
    }
}