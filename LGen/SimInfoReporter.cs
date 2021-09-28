using LGen.LSimulate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimInfoReporter : Singleton<SimInfoReporter>
{
    public Text text;

    public void UpdateInfo()
    {
        text.text = "Generation: " + Simulation.Instance.state.generation;
    }
}
