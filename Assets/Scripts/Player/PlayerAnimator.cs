using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerPlacingCabinet playerPlacignCabinet;


    private const string IS_WALKING = "IsWalking";
    private const string IS_CARRYING = "IsCarrying";
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
       
    }
    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        //Play the walking animation base on the 'IsWalking' Bool Function
        animator.SetBool(IS_WALKING, player.IsWalking());
        animator.SetBool(IS_CARRYING, playerPlacignCabinet.IsHoldingCabinet() || player.IsCarryngBarrel());
    }
}
