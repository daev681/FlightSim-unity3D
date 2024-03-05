// This should be editor only
#if UNITY_EDITOR
using UnityEngine;
using ParrelSync;

public class CustomArgumentExample : MonoBehaviour
{
    void Start()
    {
        //Is this unity editor instance opening a clone project?
        if (ClonesManager.IsClone())
        {
            Debug.Log("This is a clone project.");
            // Get the custom argument for this clone project.  
            string customArgument = ClonesManager.GetArgument();
            // Do what ever you need with the argument string.
            Debug.Log("The custom argument of this clone project is: " + customArgument);
        }
        else
        {
            Debug.Log("This is the original project.");
        }
    }
}
#endif