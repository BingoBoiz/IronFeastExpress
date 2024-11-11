using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using static CuttingCabinet;

public class Stove : BaseCabinet,IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }
    public enum State
    {
        Idle,
        Frying,
        Cooked,
        Burned,
    }

    [SerializeField] private CookingRecipeSO[] cookingRecipeSOArray;
    [SerializeField] private BurnedRecipeSO[] burnedRecipeSOArray;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> cookingTimer = new NetworkVariable<float>(0f); //cooking time, make object nicely cooked
    private NetworkVariable<float> overCookingTimer = new NetworkVariable<float>(0f); //overcooked, make object burn
    private CookingRecipeSO cookingRecipeSO;
    private BurnedRecipeSO burnedRecipeSO;

    public override void OnNetworkSpawn()
    {
        cookingTimer.OnValueChanged += CookingTimer_OnValueChanged;
        overCookingTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }
    /*private void Start()
    {
        state.Value = State.Idle;
    }*/

    private void State_OnValueChanged(State previousState, State newState)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            state = state.Value,
        });

        if (state.Value == State.Burned || state.Value == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = 0f
            });
        }
    }
    private void CookingTimer_OnValueChanged(float previousValue, float newValue)
    {
        float cookingTimerMax = cookingRecipeSO != null ? cookingRecipeSO.cookingTimerMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = cookingTimer.Value / cookingTimerMax
        });
    }
    private void BurningTimer_OnValueChanged(float previousValue, float newValue)
    {
        float overcookingTimerMax = burnedRecipeSO != null ? burnedRecipeSO.burningTimerMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = overCookingTimer.Value / overcookingTimerMax
        });
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        if (HasKitchenObject())
        {
            switch (state.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    cookingTimer.Value += Time.deltaTime;
                    if (cookingTimer.Value > cookingRecipeSO.cookingTimerMax)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        KitchenObject.SpawnKitchenObject(cookingRecipeSO.output, this);

                        //Debug.Log("Object Cooked!");
                        state.Value = State.Cooked;
                        overCookingTimer.Value = 0f;
                        int kitchenObjectSOIndex = KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO());
                        SetBurnedRecipeSOClientRpc(kitchenObjectSOIndex);
                    }
                    break;
                case State.Cooked:
                    overCookingTimer.Value += Time.deltaTime;

                    if (overCookingTimer.Value > burnedRecipeSO.burningTimerMax)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        Debug.Log("burnedRecipeSO: " + burnedRecipeSO.output.name);
                        KitchenObject.SpawnKitchenObject(burnedRecipeSO.output, this);
                        state.Value = State.Burned;
                    }
                    break;
                case State.Burned:
                    
                    break;
            }
            Debug.Log(state.Value);
           
        }
    }
    public override void Interact(Player player)
    {
        Debug.Log("Check if the counter is empty");
        if (!HasKitchenObject())
        {
            Debug.Log("Check if the player carrying something");
            if (player.HasKitchenObject())
            {
                if (player == null)
                {
                    Debug.Log("player Null");
                }

                if (player.GetKitchenObject() == null)
                {
                    Debug.Log("GetKitchenObject Null");
                }

                if (player.GetKitchenObject().GetKitchenObjectSO() == null)
                {
                    Debug.Log("GetKitchenObjectSO Null");
                }

                KitchenObject kitchenObject = player.GetKitchenObject();
                KitchenObjectSO playerKitchenObjectSO = kitchenObject.GetKitchenObjectSO();
                

                Debug.Log("Check if the " + kitchenObject.name + " is cookable");

                if (HasRecipeWithInput(playerKitchenObjectSO))
                {
                    Debug.Log("Process to cook the object");
                    kitchenObject.SetKitchenObjectParent(this);
                    int kitchenObjectSoIndex = KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(playerKitchenObjectSO);
                    InteractLogicPlaceObjectServerRpc(kitchenObjectSoIndex);
                }
                else
                {
                    Debug.Log("Can not cook this object");
                }
            }
            // Player not carrying anything
            else
            {
                Debug.Log("There is nothing to interact");
            }
        }
        else // There is a kitchen object on the counter
        {
            // Check if the player carrying something
            if (player.HasKitchenObject())
            {
                //Debug.Log("There is already a kitchen object in this counter");
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    //Debug.Log("Player is holding a plate");
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        SetStateIdleServerRpc();
                    }
                }
            }
            // Player not carrying anything
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);

                SetStateIdleServerRpc();
            }
        }
    }

    public override void InteractAlternate(Player player)
    {
        if (player.HasKitchenObject()) // Player maybe is holding a crate
        {
            Debug.Log("Player is holding a kitchenObject");
        }
        else if (player.gameObject.GetComponent<PlayerPlacingCabinet>().IsHoldingCabinet())
        {
            Debug.Log("Player is holding a cabinet");
        }
        else if (GameManager.Instance.IsTrainStop())
        {
            PlayerPlacingCabinet.LocalInstance.PickUpInterior(GetComponent<PlacedCabinet_Done>());
        }
        else
        {
            Debug.LogWarning("InteractAlternate nothing");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        state.Value = State.Idle;
    }


    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectServerRpc(int kitchenObjectSoIndex)
    {
        Debug.Log("");
        cookingTimer.Value = 0f;
        state.Value = State.Frying;

        SetCookingRecipeSOClientRpc(kitchenObjectSoIndex);
    }

    [ClientRpc]
    private void SetCookingRecipeSOClientRpc(int kitchenObjectSoIndex)
    {
        KitchenObjectSO kitchenObjectSO =  KitchenGameMultiplayer.Instance.GetKitchenObjectSOByIndex(kitchenObjectSoIndex);
        cookingRecipeSO = GetCookingRecipeSWithInput(kitchenObjectSO);
    }

    [ClientRpc]
    private void SetBurnedRecipeSOClientRpc(int kitchenObjectSoIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOByIndex(kitchenObjectSoIndex);
        burnedRecipeSO = GetBurnedRecipeSWithInput(kitchenObjectSO);
    }

    private KitchenObjectSO GetRecipeOutput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (CookingRecipeSO fryingRecipeSO in cookingRecipeSOArray)
        {
            if (fryingRecipeSO.input == inputKitchenObjectSO)
            {
                return fryingRecipeSO.output;
            }
        }
        return null;
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (CookingRecipeSO fryingRecipeSO in cookingRecipeSOArray)
        {
            if (fryingRecipeSO.input == inputKitchenObjectSO)
            {
                return true;
            }
        }
        return false;
    }

    private CookingRecipeSO GetCookingRecipeSWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (CookingRecipeSO fryingRecipeSO in cookingRecipeSOArray)
        {
            if (fryingRecipeSO.input == inputKitchenObjectSO)
            {
                return fryingRecipeSO;
            }
        }
        return null;
    }

    private BurnedRecipeSO GetBurnedRecipeSWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (BurnedRecipeSO burnedRecipeSO in burnedRecipeSOArray)
        {
            if (burnedRecipeSO.input == inputKitchenObjectSO)
            {
                return burnedRecipeSO;
            }
        }
        return null;
    }
}
