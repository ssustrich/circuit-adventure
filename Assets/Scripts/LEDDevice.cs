using UnityEngine;
using System.Linq;

public class LEDDevice : MonoBehaviour
{
    [Header("Refs (can auto-fill)")]
    public LEDLight ledLight;                 // lens emissive controller
    public LeadConnector positiveLead;        // LED/Lead_Pos
    public LeadConnector negativeLead;        // LED/Lead_Neg

    
    // In LEDDevice.cs
    [ContextMenu("DEBUG/Recompute LED")]
    void DebugRecompute() => Recompute();

    public bool logDetails = false;

    void Awake()
    {
        // Auto-fill if missing
        if (!ledLight)
            ledLight = GetComponentInChildren<LEDLight>(true);

        if (!positiveLead || !negativeLead)
        {
            var leads = GetComponentsInChildren<LeadConnector>(true);
            if (!positiveLead)
                positiveLead = leads.FirstOrDefault(l => l.type == LeadConnector.LeadType.Positive);
            if (!negativeLead)
                negativeLead = leads.FirstOrDefault(l => l.type == LeadConnector.LeadType.Negative);
        }
    }

    void OnEnable() => Recompute();

    public void Recompute()
    {
        bool on = false;

        var posOther = positiveLead ? positiveLead.connectedTo : null;
        var negOther = negativeLead ? negativeLead.connectedTo : null;

        if (posOther && negOther)
        {
            var psPos = posOther.GetComponentInParent<PowerSourceController>();
            var psNeg = negOther.GetComponentInParent<PowerSourceController>();

            if (psPos && psNeg && psPos == psNeg)
            {
                on = psPos.IsPowered;
                if (logDetails)
                    Debug.Log($"[LEDDevice:{name}] Wired to {psPos.name}, powered={psPos.IsPowered}");
            }
            else if (logDetails)
            {
                Debug.Log($"[LEDDevice:{name}] Leads connect to different sources or no source. psPos={(psPos ? psPos.name : "null")} psNeg={(psNeg ? psNeg.name : "null")}");
            }
        }
        else if (logDetails)
        {
            Debug.Log($"[LEDDevice:{name}] Not fully wired. pos={(posOther ? posOther.name : "null")} neg={(negOther ? negOther.name : "null")}");
        }

        if (ledLight) ledLight.SetOn(on);
    }
}
