using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ArticleOutcomeSO : ScriptableObject
{
    public Sprite picture;
    public string headline;
    [TextArea(minLines: 3, maxLines: 10)]
    public string content;

    [TextArea(minLines: 2, maxLines: 5)]
    public string effect;

    [System.Serializable]
    public class KitchenObjectPriceChange
    {
        public KitchenObjectSO kitchenObject;
        [Range(-100, 100)]
        public int priceChange;
    }

    public List<KitchenObjectPriceChange> kitchenObjectPriceChanges; // List of kitchen objects and their price changes

    [System.Serializable]
    public class CustomerTrafficChange
    {
        public CustomerSO customer;
        [Range(-100, 100)]
        public int trafficChange;
    }

    public List<CustomerTrafficChange> customerTrafficChanges;

}
