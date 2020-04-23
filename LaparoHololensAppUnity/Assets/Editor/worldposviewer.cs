using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class worldposviewer : Editor
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnInspectorGUI(){
        MonoBehaviour gotarget=(MonoBehaviour)target;
        if (gotarget != null){
            EditorGUILayout.BeginHorizontal();
            gotarget.transform.position=EditorGUILayout.Vector3Field("World Pos",gotarget.transform.position);
            EditorGUILayout.EndHorizontal();
        }
        
    }
}
