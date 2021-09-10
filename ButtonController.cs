using LGen.LSimulate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public UnityEngine.UI.InputField advanceIterationsInput;

    public void AdvanceIterations()
    {
        int numIterations = int.Parse(advanceIterationsInput.text);
        for (int i = 0; i < numIterations; i++)
        {
            Simulation.Instance.Iterate_SecondHalfFirst(Simulation.Instance.state);
        }
    }

    public void AddColliders()
    {
        Simulation.Instance.AddColliders(Simulation.Instance.state);
    }
}
