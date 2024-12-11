using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    public static Player LocalInstance { get; private set; } //Using a properties for singleton pattern

    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPlayerPickedSomething;

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
        OnAnyPlayerPickedSomething = null;
    }

    public event EventHandler OnPickedSomething;

    public event EventHandler<OnSelectedCabinetChangedEventArgs> OnSelectedCabinetChanged;
    public class OnSelectedCabinetChangedEventArgs : EventArgs 
    {
        public BaseCabinet selectedCabinet;
    }

    public event EventHandler<OnSelectedDeskChangedEventArgs> OnSelectedDeskChanged;
    public class OnSelectedDeskChangedEventArgs : EventArgs
    {
        public BaseDesk selectedDesk;
    }

    public event EventHandler<OnSelectedSignChangedEventArgs> OnSelectedSignChanged;
    public class OnSelectedSignChangedEventArgs : EventArgs
    {
        public BaseSign selectedSign;
    }

    public event EventHandler<OnSelectedStoreItemChangedEventAgrs> OnSelectedStoreItemChanged; 
    public class OnSelectedStoreItemChangedEventAgrs : EventArgs
    {
        public ISelectable selectedStoreItem;
    }

    [SerializeField] private Transform placeHolder;
    [SerializeField] private Transform heavyPlaceHolder;
    [SerializeField] private Transform barrelPlaceHolder;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] private LayerMask collisionLayerMask;
    [SerializeField] private List<Vector3> spawnPositionList;
    [SerializeField] private PlayerVisual playerVisual;

    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    /*[SerializeField] private Camera playerCamera; // just to turn off the canvas in non owner player
    [SerializeField] private Canvas worldSpaceCameraCanvas; // just to turn off the canvas in non owner player*/


    //= new LayerMask();
    //[SerializeField] private Transform pointer;


    private bool isWalking;
    private float interactDistance = 2f;
    private BaseCabinet selectedCabinet;
    private BaseDesk selectedDesk;
    private BaseSign selectedSign;
    private GameObject selectedStoreItem;
    private KitchenObject kitchenObject;

    private float playerRadius = 0.6f;
    private float playerHeight = 2f;
    private float playerRayCastStartingPoint = 0.2f; // Because the train floor also have a collider, this point will change the starting position of the ray cast
    private float placingDistance = 1.25f;

    //just to make sure if the character not moving, the direction the character facing is still the same
    private Vector3 lastInteracDir;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
            virtualCamera.Priority = 1;
        }
        else
        {
            virtualCamera.Priority = 0;
        }
        transform.position = spawnPositionList[KitchenGameMultiplayer.Instance.GetPlayerIndexFromClientId(OwnerClientId)];
        //transform.position = spawnPositionList[(int)OwnerClientId];

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += Player_OnClientDisconnectCallback;
        }
    }

    private void Player_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;

        PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerCharacter(playerData.characterModelIndex);
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!IsOwner)
        {
            return;
        }
        if (PlayerPlacingCabinet.LocalInstance.IsHoldingCabinet())
        {
            Debug.Log("GameInput_OnGrabAction");
            PlayerPlacingCabinet.LocalInstance.HandlePlacingCabinet();
        }
        else if (selectedCabinet != null)
        {
            selectedCabinet.InteractAlternate(this);
        }
        else if (selectedDesk != null)
        {
            selectedDesk.InteractAlternate(this);
        }
        
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (selectedCabinet != null)
        {
            selectedCabinet.Interact(this);
        }
        else if(selectedStoreItem != null && selectedStoreItem.TryGetComponent(out Barrel selectedBarrel))
        {
            selectedBarrel.Interact(this);
        }
        else if (selectedStoreItem != null && selectedStoreItem.TryGetComponent(out Crate selectedCrate))
        {
            selectedCrate.Interact(this);
        }
        else if (selectedDesk != null)
        {
            selectedDesk.Interact(this);
        }
        else if (selectedSign != null)
        {
            

            selectedSign.Interact(this);
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleInteractions()
    {
        if (GameManager.Instance.IsShowingResult())
        {
            return;
        }
        if (GameManager.Instance.IsWaitingForPlayersToReady())
        {
            return;
        }
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        //if the character not moving, the direction the character facing is still the same
        if (moveDir != Vector3.zero) 
        {
            //Debug.Log("Direction the character facing is still the same");
            lastInteracDir = moveDir;
        }

        if (Physics.Raycast(transform.position, lastInteracDir, out RaycastHit raycastHit, interactDistance, interactLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCabinet baseCabinet))
            {
                //Debug.Log("Raycast hit something that is BaseCounter class");
                if (baseCabinet != selectedCabinet) //check if new Cabinet is not being selected
                {
                    SetSelectedCabinet(baseCabinet);
                }
            }
            else if (raycastHit.transform.TryGetComponent(out BaseDesk baseDesk))
            {
                if (baseDesk != selectedDesk) //check if new Desk is not being selected
                {
                    SetSelectedDesk(baseDesk);
                }
            }
            else if (raycastHit.transform.TryGetComponent(out Barrel barrel))
            {
                if (barrel != selectedStoreItem)
                {
                    SetSelectedStoreItem(barrel);
                }
            }
            else if (raycastHit.transform.TryGetComponent(out Crate crate))
            {
                if (crate != selectedStoreItem)
                {
                    SetSelectedStoreItem(crate);
                }
            }
            else if (raycastHit.transform.TryGetComponent(out BaseSign sign))
            {
                if (sign != selectedSign)
                {
                    SetSelectedSign(sign);
                }
            }
            else
            {
                //Debug.Log("Raycast hit undefined object");
                SetSelectedCabinet(null);
                SetSelectedStoreItem(null);
                SetSelectedDesk(null);
                SetSelectedSign(null);
            }
        }
        else 
        {
            //Debug.Log("Raycast hit nothing");
            SetSelectedCabinet(null);
            SetSelectedStoreItem(null);
            SetSelectedDesk(null);
            SetSelectedSign(null);

        }
        //Debug.Log(selectedCounter);
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        // Check if the player hit something or not

        float moveDistance = moveSpeed * Time.deltaTime;

        bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, collisionLayerMask);

        if (!canMove) // If hit something
        {
            // Check if there something on X vector
            Vector3 moveDirX = new Vector3(moveDir.x, 0f, 0f);
            //canMove = moveDir.x != 0! && Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);
            canMove = moveDir.x != 0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, collisionLayerMask);
            if (canMove)
            {
                transform.position += moveDirX * moveDistance;
            }
            else // Check if there something on Z vector
            {
                Vector3 moveDirZ = new Vector3(0f, 0f, moveDir.z);
                canMove = moveDir.z !=0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisionLayerMask);
                if (canMove)
                {
                    transform.position += moveDirZ * moveDistance;
                }
                else
                {
                    // Not move at all
                }
            }
        }

        else
        {
            transform.position += moveDir * moveDistance;
        }

        //Check if the character are moving or not
        isWalking = moveDir != Vector3.zero;

        float rotatedSpeed = 10f;

        // Slerp for smooth out when character rotate 
        if (moveDir != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotatedSpeed);
        }
        

        //Debug.Log(Time.deltaTime);
    }
    
    private void SetSelectedCabinet (BaseCabinet baseCounter)
    {
        this.selectedCabinet = baseCounter;
        
        //Firing an event to change the visual of the counter
        OnSelectedCabinetChanged?.Invoke(this, new OnSelectedCabinetChangedEventArgs
        {
            selectedCabinet = selectedCabinet,
        });
    }

    private void SetSelectedDesk(BaseDesk baseDesk)
    {
        selectedDesk = baseDesk;

        //Firing an event to change the visual of the counter
        OnSelectedDeskChanged?.Invoke(this, new OnSelectedDeskChangedEventArgs
        {
            selectedDesk = selectedDesk,
        });
    }
    private void SetSelectedSign(BaseSign baseSign)
    {
        selectedSign = baseSign;
        
        //Firing an event to change the visual of the counter
        OnSelectedSignChanged?.Invoke(this, new OnSelectedSignChangedEventArgs
        {
            selectedSign = selectedSign,
        });
    }

    private void SetSelectedStoreItem(ISelectable storeItem)
    {
        if (storeItem is MonoBehaviour storeItemMono)
        {
            selectedStoreItem = storeItemMono.gameObject;
        }
        else
        {
            selectedStoreItem = null;
        }
        //Firing an event to change the visual of the barrel
        OnSelectedStoreItemChanged?.Invoke(this, new OnSelectedStoreItemChangedEventAgrs
        {
            selectedStoreItem = storeItem,
        });
    }

    public Ray CalculateInteractionRayForPlacing()
    {
        Vector3 headPosition = transform.position + Vector3.up * playerHeight;
        // Pitago Algorithm 
        float horizontalDistance = Mathf.Sqrt(playerHeight * playerHeight - placingDistance * placingDistance);
        // Calculate the direction
        Vector3 rayDirection = new Vector3(0, -playerHeight, transform.forward.z * horizontalDistance).normalized;

        Ray ray = new Ray(headPosition, rayDirection);
        //Debug.Log(transform.forward.z * horizontalDistance);
        return ray;
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return placeHolder;
    }

    public Transform GetHeavyPlaceHolderTransform()
    {
        return heavyPlaceHolder; 
    }

    public Transform GetBarrelPlaceHolderTransform()
    {
        return barrelPlaceHolder;
    }
    public float GetPlacingDistance()
    {
        return placingDistance;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPlayerPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }
    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }
    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }
    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    public bool IsCarryngBarrel()
    {
        bool isCarryingBarrel = false;
        if (HasKitchenObject())
        {
            if (GetKitchenObject().TryGetComponent(out Barrel barrel))
            {
                isCarryingBarrel = true;
            }
        }
        return isCarryingBarrel;
    }
    public Vector3 GetPlayerDirection()
    {
        return lastInteracDir;
    }
    public NetworkObject GetNetworkObject()
    {
        //Debug.Log("Player network object: " + GetComponent<NetworkObject>());
        return NetworkObject;
        //return GetComponent<NetworkObject>();
    }
    public List<Vector3> GetPlayerSpawnPositionList()
    {
        return spawnPositionList;
    }
}
