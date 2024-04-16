using UnityEditor;
using UnityEngine;

public class RandomRotation : EditorWindow
{
    const int numMaterials = 6;
    Material[] allMaterials = new Material[numMaterials];
    Vector2 scrollPosition;

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Omiya Games/Rotation Randomizer")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(RandomRotation));
    }
    
    void OnGUI()
    {
        scrollPosition = Vector2.zero;
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int index = 0; index < numMaterials; ++index)
        {
            allMaterials[index] = (Material)EditorGUILayout.ObjectField("Material " + index, allMaterials[index], typeof(Material));
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Randomize Materials on Selected Transforms") == true)
        {
            Renderer renderer;
            foreach (Transform transform in Selection.transforms)
            {
                renderer = transform.GetComponent<Renderer>();
                if(renderer != null)
                {
                    renderer.material = allMaterials[Random.RandomRange(0, numMaterials)];
                }
            }
        }

        if(GUILayout.Button("Randomize Rotation on Selected Transforms") == true)
        {
            foreach(Transform transform in Selection.transforms)
            {
                Vector3 eular = transform.rotation.eulerAngles;
                eular.y = Random.RandomRange(0f, 360f);
                transform.rotation = Quaternion.Euler(eular);
            }
        }
    }
}
