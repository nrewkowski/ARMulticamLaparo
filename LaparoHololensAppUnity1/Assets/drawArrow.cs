using UnityEngine;
using System.Collections;

//from https://wiki.unity3d.com/index.php/DrawArrow
public static class DrawArrow
{
	public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
	{
		Gizmos.DrawRay(pos, direction);
 
		Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
		Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
		Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
		Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
	}
 
	public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
	{
		Gizmos.color = color;
		Gizmos.DrawRay(pos, direction);
 
		Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
		Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
		Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
		Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
	}
 
	public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
	{
		Debug.DrawRay(pos, direction);
 
		Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
		Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
		Debug.DrawRay(pos + direction, right * arrowHeadLength);
		Debug.DrawRay(pos + direction, left * arrowHeadLength);
	}
	public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
	{
		Debug.DrawRay(pos, direction, color);
 
		Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
		Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
		Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
		Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
	}

    public static void ForDebug(Vector3 pos, Vector3 direction, float duration)
	{
		Debug.DrawRay(pos, direction, Color.green,duration);
 
		Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+20,0) * new Vector3(0,0,1);
		Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-20,0) * new Vector3(0,0,1);
		Debug.DrawRay(pos + direction, right * 0.25f, Color.green,duration);
		Debug.DrawRay(pos + direction, left * 0.25f, Color.green,duration);
	}

    public static void ForDebug(Vector3 pos, Vector3 direction, Color color,float duration)
	{
		Debug.DrawRay(pos, direction, color,duration);
 
		Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+20,0) * new Vector3(0,0,1);
		Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-20,0) * new Vector3(0,0,1);
		Debug.DrawRay(pos + direction, right * 0.25f, color,duration);
		Debug.DrawRay(pos + direction, left * 0.25f, color,duration);
	}
}