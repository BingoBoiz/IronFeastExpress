using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IContainAmount 
{
    public event EventHandler<OnAmountChangedEventArgs> OnAmountChanged;

    public class OnAmountChangedEventArgs : EventArgs
    {
        public int amount;
    }
}

