﻿using UnityEngine;
using System.Collections;

//It is common to create a class to contain all of your
//extension methods. This class must be static.
public static class ExtensionMethods
{
	//Even though they are used like normal methods, extension
	//methods must be declared static. Notice that the first
	//parameter has the 'this' keyword followed by a Transform
	//variable. This variable denotes which class the extension
	//method becomes a part of.
	public static void ResetTransformation(this Transform trans)
	{
		trans.position = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = new Vector3(1, 1, 1);
	}

	// (Since Vector2 is a struct, it is passed by value)
	public static Vector2 Rotate(this Vector2 v, float degrees) {
		float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
		float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
		
		float tx = v.x;
		float ty = v.y;
		v.x = (cos * tx) - (sin * ty);
		v.y = (sin * tx) + (cos * ty);
		return v;
	}

	// Gidi added: multiplying a vector with a float
	public static Vector2 MultiplyBy (this Vector2 v, float aValue) {
		Vector2 temp = new Vector2();
	
		temp.x = v.x * aValue;
		temp.y = v.y * aValue;

		return temp;
	}

	// Gidi added: dividing a vector by a float
	public static Vector2 DivideBy (this Vector2 v, float aValue) {
		Vector2 temp = new Vector2();
		
		temp.x = v.x / aValue;
		temp.y = v.y / aValue;
		
		return temp;
	}

	public static Vector2 Truncate(this Vector2 v)
    {
		Vector2 temp = new Vector2();

		temp.x = Mathf.RoundToInt(v.x);
		if (v.x > 0 && temp.x > v.x)
		{
			temp.x = Mathf.RoundToInt(v.x) - 1;
		}
		else if (v.x < 0 && temp.x < v.x)
        {
			temp.x = Mathf.RoundToInt(v.x) + 1;
        }
		if (v.y > 0 && temp.y > v.y)
        {
			temp.y = Mathf.RoundToInt(v.x) - 1;
        }
		else if (v.y < 0 && temp.y < v.y)
        {
			temp.y = Mathf.RoundToInt(v.x) + 1;
        }

		return temp;
    }

	public static Vector2 RotateVector(this Vector3 vector, float angleDegrees, float distance)
	{
		// Convert angle from degrees to radians
		float angleRadians = Mathf.Deg2Rad * angleDegrees;

		// Perform rotation
		float xRotated = vector.x * (float)Mathf.Cos(angleRadians) - vector.z * (float)Mathf.Sin(angleRadians);
		float yRotated = vector.x * (float)Mathf.Sin(angleRadians) + vector.z * (float)Mathf.Cos(angleRadians);

		Vector2 v = new Vector2(xRotated, yRotated);
		return v * distance;
	}


	//Changes vector from local to global Vector
	public static Vector2 RelativeVector(this Vector2 input, Vector3 right, Vector3 forward)
	{
		Vector2 temp = new Vector2(0, 0);
		temp.x = input.x * right.x + input.y * right.z;
		temp.y = input.y * forward.z + input.x * forward.x;
		return temp;
	}
}