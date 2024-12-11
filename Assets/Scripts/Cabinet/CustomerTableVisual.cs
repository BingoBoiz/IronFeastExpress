using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerTableVisual : MonoBehaviour
{
    private const string AWESOME = "Awesome";
    private const string LOSE = "Lose";

    [SerializeField] private CustomerTable customerTable;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        customerTable.OnDeliverCorrectDish += CustomerTable_OnDeliverCorrectDish;
        customerTable.OnDeliverWrongDish += CustomerTable_OnDeliverWrongDish;
    }

    private void CustomerTable_OnDeliverWrongDish(object sender, System.EventArgs e)
    {
        animator.SetTrigger(LOSE);
        SoundManager.Instance.CustomerTableDeliverWrongDish(this.transform); 
    }

    private void CustomerTable_OnDeliverCorrectDish(object sender, System.EventArgs e)
    {
        animator.SetTrigger(AWESOME);
        SoundManager.Instance.CustomerTableDeliverCorrectDish(this.transform);
    }

    private void OnDestroy()
    {
        customerTable.OnDeliverCorrectDish -= CustomerTable_OnDeliverCorrectDish;
        customerTable.OnDeliverWrongDish -= CustomerTable_OnDeliverWrongDish;
    }
}
