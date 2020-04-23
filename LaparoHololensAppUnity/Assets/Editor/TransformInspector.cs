// Reverse engineered UnityEditor.TransformInspector
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CanEditMultipleObjects, CustomEditor(typeof(Transform))]
public class TransformInspector : Editor {
 
    private const float FIELD_WIDTH = 212.0f;
    private const bool WIDE_MODE = true;
 
    private const float POSITION_MAX = 100000.0f;
 
    private static GUIContent positionGUIContent = new GUIContent(LocalString("Position")
                                                                 ,LocalString("The local position of this Game Object relative to the parent."));
    private static GUIContent rotationGUIContent = new GUIContent(LocalString("Rotation")
                                                                 ,LocalString("The local rotation of this Game Object relative to the parent."));
    private static GUIContent scaleGUIContent    = new GUIContent(LocalString("Scale")
                                                                 ,LocalString("The local scaling of this Game Object relative to the parent."));

    private static string positionWarningText = LocalString("Due to floating-point precision limitations, it is recommended to bring the world coordinates of the GameObject within a smaller range.");
 
    private SerializedProperty positionProperty;
    private SerializedProperty rotationProperty;
    private SerializedProperty scaleProperty;

 
    private static string LocalString(string text) {
        return text;
    }
 
    public void OnEnable() {
        this.positionProperty = this.serializedObject.FindProperty("m_LocalPosition");
        this.rotationProperty = this.serializedObject.FindProperty("m_LocalRotation");
        this.scaleProperty    = this.serializedObject.FindProperty("m_LocalScale");
        //Transform targetTrans=((Transform) this.target);
//        this.gpositionProperty = new SerializableVector3(targetTrans.position.x,targetTrans.position.y,targetTrans.position.z);
        //this.grotationProperty = this.serializedObject.FindProperty("m_LocalRotation");
        //this.gscaleProperty    = this.serializedObject.FindProperty("m_LocalScale");
        //SerializedProperty it = this.positionProperty.Copy ();
        //while (it.Next (true)) { // or NextVisible, also, the bool argument specifies whether to enter on children or not
            //Debug.Log ("ITERATE: "+it.name);
        //}

        //this.gpositionProperty = this.serializedObject.FindProperty("m_GlobalPosition");
        //this.grotationProperty = this.serializedObject.FindProperty("m_GlobalRotation");
        //this.gscaleProperty    = this.serializedObject.FindProperty("m_GlobalScale");
    }
 
    public override void OnInspectorGUI() {
        EditorGUIUtility.wideMode = TransformInspector.WIDE_MODE;
        EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - TransformInspector.FIELD_WIDTH; // align field to right of inspector
 
        this.serializedObject.Update();
 
        EditorGUILayout.PropertyField(this.positionProperty, positionGUIContent);
        this.RotationPropertyField(   this.rotationProperty, rotationGUIContent);
        EditorGUILayout.PropertyField(this.scaleProperty,    scaleGUIContent);
 
        if (!ValidatePosition(((Transform) this.target).position)) {
            EditorGUILayout.HelpBox(positionWarningText, MessageType.Warning);
        }

        
        Transform gtarget=(Transform) target;
        if (gtarget !=null){
            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Vector3Field("World Pos",gtarget.position);
            EditorGUILayout.Vector3Field("World Rot",gtarget.eulerAngles);
            EditorGUILayout.Vector3Field("World Scale",gtarget.lossyScale);
        }
        this.serializedObject.ApplyModifiedProperties();
    }
 
    private bool ValidatePosition(Vector3 position) {
        if (Mathf.Abs(position.x) > TransformInspector.POSITION_MAX) return false;
        if (Mathf.Abs(position.y) > TransformInspector.POSITION_MAX) return false;
        if (Mathf.Abs(position.z) > TransformInspector.POSITION_MAX) return false;
        return true;
    }
 
    private void RotationPropertyField(SerializedProperty rotationProperty, GUIContent content) {
        Transform transform = (Transform) this.targets[0];
        Quaternion localRotation = transform.localRotation;
        foreach (UnityEngine.Object t in (UnityEngine.Object[]) this.targets) {
            if (!SameRotation(localRotation, ((Transform) t).localRotation)) {
                EditorGUI.showMixedValue = true;
                break;
            }
        }
 
        EditorGUI.BeginChangeCheck();
 
        Vector3 eulerAngles = EditorGUILayout.Vector3Field(content, localRotation.eulerAngles);
 
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObjects(this.targets, "Rotation Changed");
            foreach (UnityEngine.Object obj in this.targets) {
                Transform t = (Transform) obj;
                t.localEulerAngles = eulerAngles;
            }
            rotationProperty.serializedObject.SetIsDifferentCacheDirty();
        }
 
        EditorGUI.showMixedValue = false;
    }
 
    private bool SameRotation(Quaternion rot1, Quaternion rot2) {
        if (rot1.x != rot2.x) return false;
        if (rot1.y != rot2.y) return false;
        if (rot1.z != rot2.z) return false;
        if (rot1.w != rot2.w) return false;
        return true;
    }
}

 