using LGen.LSimulate;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ButtonController : Singleton<ButtonController>
{
    public RectTransform UIParent;
    public UnityEngine.UI.InputField advanceIterationsInput;

    public GameObject gridDensityUiParent;
    public UnityEngine.UI.Text textPrefab;
    public UnityEngine.UI.RawImage exposureCameraDisplayPrefab;

    public List<GameObject> exposureCameraDisplays = new List<GameObject>();

    public void AdvanceIterations()
    {
        int numIterations = int.Parse(advanceIterationsInput.text);
        for (int i = 0; i < numIterations; i++)
        {
            Simulation.Instance.Iterate_SecondHalfFirst(Simulation.Instance.state);

            if (Simulation.Instance.state.generation % 50 == 0)
            {
                WriteSimState();
            }
        }

        SimInfoReporter.Instance.UpdateInfo();
    }

    public void ShowLeafExposureTextures()
    {
        if (exposureCameraDisplays.Count == 0)
        {
            foreach(Camera leafExposureCamera in LGen.LRender.RendererResources.Instance.leafExposureCameras)
            {
                UnityEngine.UI.RawImage display = GameObject.Instantiate(exposureCameraDisplayPrefab);
                display.rectTransform.parent = UIParent;
                display.texture = leafExposureCamera.targetTexture;
                float y = -20 - display.rectTransform.sizeDelta.y/2f - exposureCameraDisplays.Count * display.rectTransform.sizeDelta.y;
                display.rectTransform.anchoredPosition = new Vector2(-display.rectTransform.sizeDelta.x/2f, y);

                exposureCameraDisplays.Add(display.gameObject);
                display.gameObject.SetActive(false);
            }
        }

        foreach(GameObject o in exposureCameraDisplays)
        {
            o.SetActive(true);
        }
    }

    public void AddColliders()
    {
        Simulation.Instance.AddColliders(Simulation.Instance.state);
    }

    public void ShowGridDensity()
    {
        Simulation.Instance.ShowGridDensity(Simulation.Instance.state, gridDensityUiParent, textPrefab);
    }

    public void WriteSimState()
    {
        string directory = $"Assets/JSON/{Simulation.Instance.state.simulationID}/generations";
        System.IO.Directory.CreateDirectory(directory);
        StreamWriter writer = new StreamWriter($"{directory}/gen_{Simulation.Instance.state.generation}.json", true);
        Simulation.Instance.JSONtoFile(Simulation.Instance.state, writer);
    }
}
