using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
public class FindMissingImagesRecursively : EditorWindow 
{
    static int go_count = 0, components_count = 0, missing_count = 0;
 
    [MenuItem("Window/FindMissingImages")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FindMissingImagesRecursively));
    }
 
    public void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in selected GameObjects"))
        {
            FindInSelected();
        }
    }
    private static void FindInSelected()
    {
        GameObject[] go = Selection.gameObjects;
        go_count = 0;
		components_count = 0;
		missing_count = 0;
        foreach (GameObject g in go)
        {
   			FindInGO(g);
        }
        Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
    }
 
    private static void FindInGO(GameObject g)
    {
        go_count++;
        Component[] components = g.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] != null)
            {
                components_count++;
                Image image = components[i].GetComponent<Image>();

				if(image != null && image.sprite == null){
                    missing_count++;
                    string s = g.name;
                    Transform t = g.transform;
                    while (t.parent != null) 
                    {
                        s = t.parent.name +"/"+s;
                        t = t.parent;
                    }
                    Debug.Log (s + " has an empty image sprite attached in position: " + i, g);
                }
            }
        }
        // Now recurse through each child GO (if there are any):
        foreach (Transform childT in g.transform)
        {
            //Debug.Log("Searching " + childT.name  + " " );
            FindInGO(childT.gameObject);
        }
    }
}