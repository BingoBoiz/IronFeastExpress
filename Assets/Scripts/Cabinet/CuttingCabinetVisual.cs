using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCabinetVisual : MonoBehaviour
{
    private const string CUT = "Cut";

    [SerializeField] private CuttingCabinet cuttingCounter;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        cuttingCounter.OnCutObject += CuttingCounter_OnCutObject;
    }

    private void CuttingCounter_OnCutObject(object sender, System.EventArgs e)
    {
        animator.SetTrigger(CUT);
        SoundManager.Instance.CuttingCabinetOnCut(this.transform);
    }

    

    
}
