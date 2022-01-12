using LGen.LSimulate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationAutoRunner : Singleton<SimulationAutoRunner>
{

    private bool readyForNextIteration = true;
    public bool paused = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (readyForNextIteration && !paused)
        {
            readyForNextIteration = false;
            Simulation.Instance.Iterate_SecondHalfFirst(Simulation.Instance.state);

            if (Simulation.Instance.state.generation % 50 == 0)
            {
                ButtonController.Instance.WriteSimState();
            }
            SimInfoReporter.Instance.UpdateInfo();

            StartCoroutine("WaitThenReady");
        }
    }

    IEnumerator WaitThenReady()
    {
        yield return new WaitForSeconds(0.5f);

        this.readyForNextIteration = true;
    }
}
