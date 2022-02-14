using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleManager : MonoBehaviour
{
    //List<string> content;
    protected static ConsoleManager instance = null;

    LinkedList<string> content = null;
    string screenString;
    int maxLines = 20;
    int id = 0;
    public bool isEnabled = true;

    
    public static ConsoleManager Initialize(bool isEnabled = false)
    {
        if (instance == null)
        {
            
            var obj = new GameObject("ConsoleManager");

            var cm = obj.AddComponent<ConsoleManager>();

            cm.content = new LinkedList<string>();
            cm.isEnabled = isEnabled;

            DontDestroyOnLoad(obj);

            instance = cm;
        }

        return instance;
    }
    
    public void Print(string s)
    {
        //if (!isEnabled)
        //    return;

        id++;
        content.AddFirst(s);

        if (content.Count > maxLines)
            content.RemoveLast();

        screenString = "";

        int n = id;
        foreach (var str in content)
        {
            n--;
            screenString += $"{n}: {str}\n";
        }
    }

    void OnGUI()
    {
        if (!isEnabled)
            return;

        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10, Screen.width-10, Screen.height/2), screenString);
    }


}
