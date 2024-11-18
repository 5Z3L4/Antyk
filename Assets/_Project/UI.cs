using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Text text;

    void Update()
    {
        var tests = FindObjectsOfType<Text>();
        foreach (var text in tests)
        {
            if (double.TryParse(text.text, out _))
            {
                text.text = Time.time.ToString();
            }
        }
    }

    void Stop() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Debug.Log("Level complete, stopping playmode\n");
    }
}
