using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectable 
{
    public event EventHandler OnSelectedChanged;
}
