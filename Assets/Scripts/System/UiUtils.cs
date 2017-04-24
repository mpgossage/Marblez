using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Gossage.System
{
    /// <summary>
    /// Helper func to find Gui componentns by name & other UI related tasks
    /// </summary>
    public static class UiUtils
    {
        // hunts through a object & its children, looking for a certain named object
        public static bool GetGameObject(GameObject parent, string findName, out GameObject found)
        {
            Transform t = parent.transform.Find(findName);
            if (t == null)
            {
                found = null;
                return false;
            }
            found = t.gameObject;
            return true;
        }
        // hunts through a object & its children, looking for a certain named object, and then get the component on it.
        public static bool GetGameObjectComponent<T>(GameObject parent, string findName, out T found) where T : Component
        {
            GameObject go = null;
            if (!GetGameObject(parent, findName, out go))
            {
                Debug.LogError("GetGameObjectComponent cannot find: " + findName);
                found = null;
                return false;
            }
            found = go.GetComponent<T>();
            if (found == null)
            {
                Debug.LogError("GetGameObjectComponent: " + findName + " does not have a " + typeof(T).Name);
                return false;
            }
            return true;
        }
        // adds a delegate to a button
        public static void AddDelegateToButton(Button button, UnityAction call)
        {
            button.onClick.AddListener(call);
        }
    }

}