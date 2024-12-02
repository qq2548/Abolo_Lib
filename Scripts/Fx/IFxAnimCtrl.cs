using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFxAnimCtrl
{
    void Play();
    void Stop(Action callback);
}
