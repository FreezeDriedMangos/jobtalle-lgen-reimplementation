using LGen.LSimulate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : Singleton<ButtonController>
{
    public UnityEngine.UI.InputField advanceIterationsInput;

    public GameObject gridDensityUiParent;
    public UnityEngine.UI.Text textPrefab;

    public void AdvanceIterations()
    {
        int numIterations = int.Parse(advanceIterationsInput.text);
        for (int i = 0; i < numIterations; i++)
        {
            Simulation.Instance.Iterate_SecondHalfFirst(Simulation.Instance.state);
        }

        SimInfoReporter.Instance.UpdateInfo();
    }

    public void AddColliders()
    {
        Simulation.Instance.AddColliders(Simulation.Instance.state);
    }

    public void ShowGridDensity()
    {
        Simulation.Instance.ShowGridDensity(Simulation.Instance.state, gridDensityUiParent, textPrefab);
    }
}
