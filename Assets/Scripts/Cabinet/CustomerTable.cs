using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CustomerTable : BaseCabinet
{
    //[SerializeField] private Transform dishHolder;
    public event EventHandler OnDeliverCorrectDish;
    public event EventHandler OnDeliverWrongDish;
    public event EventHandler<OnCustomerOrderDishEventArgs> OnCustomerOrderDish;
    public class OnCustomerOrderDishEventArgs : EventArgs
    {
        public FinishDishSO customerChosenOrderDish;
    }
    public event EventHandler<OnTimerValueChangedEventArgs> OnTimerValueChanged;
    public class OnTimerValueChangedEventArgs : EventArgs
    {
        public float timerClockNormalized;
    }
    public event EventHandler<OnStateValueChangedEventArgs> OnStateValueChanged;
    public class OnStateValueChangedEventArgs : EventArgs
    {
        public TableState state;
    }

    private List<FinishDishSO> customerRandomChoose;
    private FinishDishSO customerOrderDishSO;
    private FinishDishListSO menuDishListSO;


    private CustomerSO eatingCustomerSO;

    private NetworkVariable<TableState> state = new NetworkVariable<TableState>(TableState.AwaitingCustomer);
    private NetworkVariable<CustomerReview> review = new NetworkVariable<CustomerReview>(CustomerReview.Terrible);

    private NetworkVariable<float> hungerTime = new NetworkVariable<float>(0f);
    private float maxHungerRandomTime = 0f;
    private NetworkVariable<float> orderWaitTimer = new NetworkVariable<float>(0f);
    private float maxOrderingwaitTime = 20f;
    private NetworkVariable<float> foodWaitTimer = new NetworkVariable<float>(0f);
    private float maxFoodWaitTime = 20f;
    private NetworkVariable<float> eatingTime = new NetworkVariable<float>(0f);
    private float maxEatingTime = 10f;
    private NetworkVariable<float> cleaningWaitTime = new NetworkVariable<float>(0f);
    private float maxCleaningWaitTime = 15f;
    private float customerPayment;

    private bool isCorrectDishDeliver = false;
    private bool isLateForCleaning = false;


    // Tabble state: Wait for customer, order, wait for food, eat food, wait to get clean
    public enum TableState
    {
        AwaitingCustomer,
        PlacingOrder,
        AwaitingFoodDelivery,
        EatingFood,
        AwaitingCleanup
    }

    private enum CustomerReview
    {
        Awesome, //5 star, fav food, cleaning fast
        Great, // 4 star, fav food
        Good, // 3 star, deliver correct food, cleaning fast (menu don't have fav food)
        NotRecommend, // 2 star, deliver wrong food, cleaning fast
        Terrible, // 1 star, staff not found/ food not found/ deliver wrong food and not clean up
    }

    public override void OnNetworkSpawn()
    {
        maxHungerRandomTime = UnityEngine.Random.Range(4f, 15f);
        //Debug.Log(maxHungerRandomTime);
        customerRandomChoose = new List<FinishDishSO>();

        orderWaitTimer.OnValueChanged += OrderWaitTimer_OnValueChanged;
        foodWaitTimer.OnValueChanged += FoodWaitTimer_OnValueChanged;
        eatingTime.OnValueChanged += EatingTimer_OnValueChanged;
        cleaningWaitTime.OnValueChanged += CleaningWaitTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
        GameManager.Instance.OnStateChange += GameManager_OnStateChange;
    }

    private void GameManager_OnStateChange(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsTrainStop())
        {
            ResetNewCustomerServerRpc();
        }
    }

    private void State_OnValueChanged(TableState previousState, TableState newState)
    {
        OnStateValueChanged?.Invoke(this, new OnStateValueChangedEventArgs
        {
            state = GetTableState(),
        });
    }

    private void OrderWaitTimer_OnValueChanged(float previousValue, float newValue)
    {
        OnTimerValueChanged?.Invoke(this, new OnTimerValueChangedEventArgs
        {
            timerClockNormalized = orderWaitTimer.Value / maxOrderingwaitTime
        });
    }
    private void FoodWaitTimer_OnValueChanged(float previousValue, float newValue)
    {
        OnTimerValueChanged?.Invoke(this, new OnTimerValueChangedEventArgs
        {
            timerClockNormalized = foodWaitTimer.Value / maxFoodWaitTime
        });
    }
    private void EatingTimer_OnValueChanged(float previousValue, float newValue)
    {
        OnTimerValueChanged?.Invoke(this, new OnTimerValueChangedEventArgs
        {
            timerClockNormalized = eatingTime.Value / maxEatingTime
        });
    }
    private void CleaningWaitTimer_OnValueChanged(float previousValue, float newValue)
    {
        OnTimerValueChanged?.Invoke(this, new OnTimerValueChangedEventArgs
        {
            timerClockNormalized = cleaningWaitTime.Value / maxCleaningWaitTime
        });
    }

    // Temp
    private float debugLogTimer = 0f; // Timer to track logging intervals
    private float logInterval = 3f; // Log every 3 seconds

    private void Update()
    {
        if (!GameManager.Instance.IsTrainRunning())
        {
            return;
        }
        else if (!IsServer)
        {
            return; 
        }
        switch (state.Value)
        {
            case TableState.AwaitingCustomer: // Randomly choose a customer to want order
                if (hungerTime.Value == 0)
                {
                    CustomerShowUpServerRpc();
                }
                
                hungerTime.Value += Time.deltaTime;
                if (hungerTime.Value >= maxHungerRandomTime) // Customer begin to call for order
                {
                    OrderingDish();
                    state.Value = TableState.PlacingOrder;
                } 
                break;
            case TableState.PlacingOrder:
                // If player interact, table will pick a food from the menu and go to next state
                orderWaitTimer.Value += Time.deltaTime;
                if (orderWaitTimer.Value >= maxOrderingwaitTime) // Staff not found
                {
                    review.Value = CustomerReview.Terrible;
                    ProcessCustomerPaymentServerRpc(review.Value);
                    ResetNewCustomerServerRpc();
                }
                break;
            case TableState.AwaitingFoodDelivery:
                foodWaitTimer.Value += Time.deltaTime;
                if (foodWaitTimer.Value >= maxFoodWaitTime) // Food not found
                {
                    review.Value = CustomerReview.Terrible;
                    ProcessCustomerPaymentServerRpc(review.Value); 
                    ResetNewCustomerServerRpc();
                }
                break;
            case TableState.EatingFood:
                eatingTime.Value += Time.deltaTime;
                if (eatingTime.Value >= maxEatingTime) // Customer finish eating
                {
                    CallStaffForCleanup();
                    //ResetTimer();
                }
                break;
            case TableState.AwaitingCleanup:
                if (isLateForCleaning)
                {
                    break;
                }
                cleaningWaitTime.Value += Time.deltaTime;
                if (cleaningWaitTime.Value >= maxCleaningWaitTime) // Not clean up 
                {
                    isLateForCleaning = true;
                }
                break;
        }
        debugLogTimer += Time.deltaTime;
        if (debugLogTimer >= logInterval)
        {
            Debug.Log("Current TableState is " + state.Value + " and customer review is " + review.Value);
            debugLogTimer = 0f; // Reset the timer
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CustomerShowUpServerRpc()
    {
        CustomerShowUpClientRpc();
    }

    [ClientRpc]
    private void CustomerShowUpClientRpc()
    {
        // Get the menu and customer
        menuDishListSO = TodayMenuManager.Instance.GetTodayMenuDishList();
        eatingCustomerSO = LandManager.Instance.GetRandomCustomerSO();
    }

    public override void Interact(Player player)
    {
        switch (state.Value)
        {
            case TableState.AwaitingCustomer: // Nothing change
                break;
            case TableState.PlacingOrder:
                TakeCustomerOrderServerRpc();
                break;
            case TableState.AwaitingFoodDelivery:
                if (player.HasKitchenObject())
                {
                    if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                    {
                        /*CheckDeliverCorrectDish(plateKitchenObject);*/

                        if (IsDeliverCorrectDish(plateKitchenObject))
                        {
                            CorrectDishDeliverLogicServerRpc();
                        }
                        else
                        {
                            WrongDishDeliverLogicServerRpc();
                        }
                        player.GetKitchenObject().SetKitchenObjectParent(this);

                    }
                    else
                    {
                        Debug.LogWarning("Cannot get the plate component from player");
                    }
                }
                else
                {
                    Debug.LogWarning("Player not holding a kitchenObject");
                }
                break;
            case TableState.EatingFood:
                Debug.Log("Customer is eating!!!");
                break;
            case TableState.AwaitingCleanup:
                if (player.HasKitchenObject() || player.GetComponent<PlayerPlacingCabinet>().IsHoldingCabinet())
                {
                    Debug.Log("Cannot clean because player is carrying something");
                    return;
                }

                CustomerDecidingReviewServerRpc();

                Debug.LogWarning("Customer review: " + review.Value);
                ProcessCustomerPaymentServerRpc(review.Value);

                // Player trigger table cleanup
                ResetNewCustomerServerRpc();
                
                break;
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
    private void CorrectDishDeliverLogicServerRpc()
    {
        Debug.Log("CorrectDishDeliverLogicServerRpc");
        isCorrectDishDeliver = true;
        state.Value = TableState.EatingFood;
        eatingTime.Value = 0f;
        DishDeliveredResultClientRpc(isCorrectDishDeliver);
    }

    [ServerRpc(RequireOwnership = false)]
    private void WrongDishDeliverLogicServerRpc()
    {
        Debug.Log("WrongDishDeliverLogicServerRpc");
        isCorrectDishDeliver = false;
        state.Value = TableState.EatingFood;
        eatingTime.Value = 0f;
        DishDeliveredResultClientRpc(isCorrectDishDeliver);
    }

    [ClientRpc]
    private void DishDeliveredResultClientRpc(bool correctDish) // This is for sound and animation (Visual.cs)
    {
        if (correctDish)
        {
            OnDeliverCorrectDish?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            OnDeliverWrongDish?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CustomerDecidingReviewServerRpc()
    {
        Debug.Log("Customer current review: " + review.Value);
        Debug.Log("Customer get served correct dish: " + isCorrectDishDeliver);
        Debug.Log("Customer get clean table fast: " + !isLateForCleaning);
        switch (review.Value)
        {
            case CustomerReview.Awesome: // When customer order their favourite dish in menu
                if (isCorrectDishDeliver)
                {
                    if (isLateForCleaning)
                    {
                        review.Value = CustomerReview.Great;
                    }
                }
                else // Wrong dish
                {
                    if (!isLateForCleaning)
                    {
                        review.Value = CustomerReview.NotRecommend;
                    }
                    else
                    {
                        review.Value = CustomerReview.Terrible;
                    }
                }
                break;
            case CustomerReview.Good: // When customer order some random ok dish in menu
                if (isCorrectDishDeliver)
                {
                    if (isLateForCleaning)
                    {
                        review.Value = CustomerReview.NotRecommend;
                    }
                }
                else // Wrong dish
                {
                    if (!isLateForCleaning)
                    {
                        review.Value = CustomerReview.NotRecommend;
                    }
                    else
                    {
                        review.Value = CustomerReview.Terrible;
                    }
                }
                break;
            case CustomerReview.NotRecommend: // All customer can order is hate dish in menu
                if (!isCorrectDishDeliver)
                {
                    review.Value = CustomerReview.Terrible;
                }
                else if (isLateForCleaning)
                {
                    review.Value = CustomerReview.Terrible;
                }
                break;
        }
    }

    private void OrderingDish()
    {
        bool haveFavouriteDish = false;
        // Give order if contain customer favourite
        for (int i = 0; i < menuDishListSO.finishDishSOList.Count; i++)
        {
            if (eatingCustomerSO.favoriteDish.Contains(menuDishListSO.finishDishSOList[i]))
            {
                customerRandomChoose.Add(menuDishListSO.finishDishSOList[i]);

                haveFavouriteDish = true;
            }
        }

        if (haveFavouriteDish)
        {
            int customerRandomChooseIndex = UnityEngine.Random.Range(0, customerRandomChoose.Count);
            ShowingCustomerOrderServerRpc(customerRandomChooseIndex);
            review.Value = CustomerReview.Awesome;
            Debug.Log("The customer " + eatingCustomerSO.customerOccupation + " choose their favourite dish: " + customerOrderDishSO.finishDishName.ToString());
        }
        else // Don't have favourite dish in the menu
        {
            int hateDishCount = 0;
            // Remove all the hate dish out of the menu list 
            for (int i = 0; i < menuDishListSO.finishDishSOList.Count; i++)
            {
                if (eatingCustomerSO.hateDish.Contains(menuDishListSO.finishDishSOList[i]))
                {
                    //Debug.Log("The custmer " + eatingCustomerSO.customerOccupation + " hate this dish: " + menuDishListSO.finishDishSOList[i]);
                    hateDishCount++;
                }
                else
                {
                    customerRandomChoose.Add(menuDishListSO.finishDishSOList[i]);
                }
            }
            // And order a random dish
            if (hateDishCount == menuDishListSO.finishDishSOList.Count) // The customer hate all the dish in the menu
            {
                // Randomly choose a food from the menu
                customerRandomChoose = new List<FinishDishSO>(menuDishListSO.finishDishSOList);
                int customerRandomChooseIndex = UnityEngine.Random.Range(0, menuDishListSO.finishDishSOList.Count - 1);
                ShowingCustomerOrderServerRpc(customerRandomChooseIndex);
                review.Value = CustomerReview.NotRecommend;

                Debug.Log("The customer " + eatingCustomerSO.customerOccupation + " hate all the Dish so they choose: " + customerOrderDishSO.finishDishName.ToString());
            }
            else // There are some ok dish in the menu
            {
                int customerRandomChooseIndex = UnityEngine.Random.Range(0, customerRandomChoose.Count);
                ShowingCustomerOrderServerRpc(customerRandomChooseIndex);
                review.Value = CustomerReview.Good;
                Debug.Log("The customer " + eatingCustomerSO.customerOccupation + " choose Dish: " + customerOrderDishSO.finishDishName.ToString());
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowingCustomerOrderServerRpc(int customerRandomChooseIndex)
    {
        customerOrderDishSO = customerRandomChoose[customerRandomChooseIndex];
        int menudDishListSOIndex = menuDishListSO.finishDishSOList.IndexOf(customerOrderDishSO);
        ShowingCustomerOrderClientRpc(menudDishListSOIndex);
    }

    [ClientRpc]
    private void ShowingCustomerOrderClientRpc(int customerRandomChooseIndex)
    {
        customerOrderDishSO = menuDishListSO.finishDishSOList[customerRandomChooseIndex];
        OnCustomerOrderDish?.Invoke(this, new OnCustomerOrderDishEventArgs
        {
            customerChosenOrderDish = customerOrderDishSO,
        });
        if (!IsHost)
        {
            Debug.Log("[ClientRPC]: The customer choose dish: " + customerOrderDishSO.finishDishName.ToString());
        }
    }

    [ServerRpc(RequireOwnership = false)]

    private void TakeCustomerOrderServerRpc() 
    {
        state.Value = TableState.AwaitingFoodDelivery;
    }

    private void CheckDeliverCorrectDish(PlateKitchenObject plateKitchenObject)
    {
        List<KitchenObjectSO> plateIngredients = plateKitchenObject.GetKitchenObjectSOList();
        List<KitchenObjectSO> recipeIngredients = customerOrderDishSO.ingridientKitchenObjectSOList;

        bool plateContentMatchesRecipe = plateIngredients.Count == recipeIngredients.Count &&
                                         recipeIngredients.All(ri => plateIngredients.Contains(ri));

        if (plateContentMatchesRecipe)
        {
            isCorrectDishDeliver = true;
            Debug.LogWarning("You delivered the correct dish: " + customerOrderDishSO.finishDishName);
        }

        state.Value = TableState.EatingFood;
        eatingTime.Value = 0f;
    }

    private bool IsDeliverCorrectDish(PlateKitchenObject plateKitchenObject)
    {
        List<KitchenObjectSO> plateIngredients = plateKitchenObject.GetKitchenObjectSOList();
        List<KitchenObjectSO> recipeIngredients = customerOrderDishSO.ingridientKitchenObjectSOList;

        bool plateContentMatchesRecipe = plateIngredients.Count == recipeIngredients.Count &&
                                         recipeIngredients.All(ri => plateIngredients.Contains(ri));
        return plateContentMatchesRecipe;
    }


    private void CallStaffForCleanup()
    {
        state.Value = TableState.AwaitingCleanup;
        cleaningWaitTime.Value = 0f;
        
        //Debug.Log(maxHungerRandomTime);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ProcessCustomerPaymentServerRpc(CustomerReview review) // Payment logic when food is delivered
    {
        // Calculate the payment based on customer preferences
        float bonusTip;
        switch (review)
        {
            default:
            case CustomerReview.Awesome:
                bonusTip = customerOrderDishSO.finishDishPrice * 0.15f;
                customerPayment = customerOrderDishSO.finishDishPrice + bonusTip;
                break;
            case CustomerReview.Great:
                bonusTip = customerOrderDishSO.finishDishPrice * 0.05f;
                customerPayment = customerOrderDishSO.finishDishPrice + bonusTip;
                break;
            case CustomerReview.Good:
                customerPayment = customerOrderDishSO.finishDishPrice;
                break;
            case CustomerReview.NotRecommend:
                customerPayment = 0f;
                break;
            case CustomerReview.Terrible:
                customerPayment = 0f;
                break;
        }
        FinanceSystem.Instance.AddingToRevenue(customerPayment);
        Debug.Log("Customer think the dish is " + review.ToString() + " and pay: " + customerPayment);
        CustomerLeaveReview(review);
    }

    private void ResetTimer()
    {
        hungerTime.Value = 0f;
        foodWaitTimer.Value = 0f;
        orderWaitTimer.Value = 0f;
        eatingTime.Value = 0f;
        cleaningWaitTime.Value = 0f;
        isCorrectDishDeliver = false;
        isLateForCleaning = false;
        maxHungerRandomTime = UnityEngine.Random.Range(4f, 15f);

        if (HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void CustomerLeaveReview(CustomerReview customerReview)
    {
        switch (review.Value)
        {
            default:
            case CustomerReview.Awesome:
                Debug.Log("Customer leaves a " +  review.ToString()  +  " review.");
                break;
            case CustomerReview.Great:
                Debug.Log("Customer leaves a " + review.ToString() + " review.");
                break;
            case CustomerReview.Good:
                Debug.Log("Customer leaves a " + review.ToString() + " review.");
                break;
            case CustomerReview.NotRecommend:
                Debug.Log("Customer leaves a " + review.ToString() + " review.");
                break;
            case CustomerReview.Terrible:
                Debug.Log("Customer leaves a " + review.ToString() + " review.");
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetNewCustomerServerRpc()
    {
        state.Value = TableState.AwaitingCustomer;
        
        ResetTimer();
    }

    private TableState GetTableState()
    {
        return state.Value;
    }

    /*private float GetTimerClockProgress(float timerValue, float timerMax)
    {
        return timerValue / timerMax;
    }*/

    public bool IsAwaitingFoodDelivery()
    {
        return state.Value == TableState.AwaitingFoodDelivery;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DebugingServerRpc(string debugingText)
    {
        DebugingClientRpc(debugingText);
    }

    [ClientRpc]
    private void DebugingClientRpc(string debugingText)
    {
        Debug.Log(debugingText);
    }

}
