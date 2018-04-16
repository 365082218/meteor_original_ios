//----------------------------------------------
//			  NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
// Multi-objects editing support added by 
// Bardelot 'Cripple' Alexandre / Graphicstream.
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Transform))]
[CanEditMultipleObjects]
public class NGUITransformInspector : Editor
{
	SerializedProperty _rotProp;
	SerializedProperty _posProp;
	SerializedProperty _sclProp;

	#region Helpers structs/enum

	// Enumeration of axes.
	public enum Vector3Axe
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,
		All = X | Y | Z
	}

	/// <summary>
	/// Describes a Vector update. 
	/// We track axes modification using Set#() methods and update a final vector on modified axes only.
	/// We use this because of Multi-editing Objects. We want to update the axes one by one.
	/// </summary>

	public struct Vector3Update
	{
		public Vector3Axe UpdatedAxes;
		public Vector3 Value;

		/// <summary>
		/// Force the update of the vector on all Axes using the given value.
		/// </summary>

		public Vector3 ForceSet(Vector3 newValue)
		{
			UpdatedAxes = Vector3Axe.All;
			Value = newValue;
			return Value;
		}

		/// <summary>
		/// Sets the value of the Vector on modified axes.
		/// </summary>

		public Vector3 Set(Vector3 newValue)
		{
			SetX(newValue.x);
			SetY(newValue.y);
			SetZ(newValue.z);
			return Value;
		}

		/// <summary>
		/// Sets the value of the X axe.
		/// </summary>

		public Vector3 SetX(float x)
		{
			if (x != Value.x)
			{
				Value.x = x;
				UpdatedAxes |= Vector3Axe.X;
			}
			return Value;
		}

		/// <summary>
		/// Sets the value of the Y axe.
		/// </summary>

		public Vector3 SetY(float y)
		{
			if (y != Value.y)
			{
				Value.y = y;
				UpdatedAxes |= Vector3Axe.Y;
			}
			return Value;
		}

		/// <summary>
		/// Sets the value of the Z axe.
		/// </summary>

		public Vector3 SetZ(float z)
		{
			if (z != Value.z)
			{
				Value.z = z;
				UpdatedAxes |= Vector3Axe.Z;
			}
			return Value;
		}

		/// <summary>
		/// Checks if an axe has been modified.
		/// </summary>

		public bool IsAxeUpdated(Vector3Axe axe)
		{
			return (UpdatedAxes & axe) == axe;
		}

		/// <summary>
		/// Validates the current Vector.
		/// </summary>

		public Vector3Update Validate()
		{
			Vector3 vector = NGUITransformInspector.Validate(Value);
			Set(vector);
			return this;
		}

		/// <summary>
		/// Gets the vector updated on modified axes only.
		/// </summary>

		public Vector3 GetUpdatedVector3(Vector3 vector)
		{
			if (UpdatedAxes == Vector3Axe.All) return Value;

			if (IsAxeUpdated(Vector3Axe.X)) vector.x = Value.x;
			if (IsAxeUpdated(Vector3Axe.Y)) vector.y = Value.y;
			if (IsAxeUpdated(Vector3Axe.Z)) vector.z = Value.z;

			return vector;
		}
	}

	#endregion

	void OnEnable()
	{
		_rotProp = serializedObject.FindProperty("m_LocalRotation");
		_posProp = serializedObject.FindProperty("m_LocalPosition");
		_sclProp = serializedObject.FindProperty("m_LocalScale");
	}

	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI()
	{
		Transform trans = target as Transform;
		EditorGUIUtility.LookLikeControls(15f);

		bool forceValue = false; // Need this in unity 3.5 to fix a weird bug with GUI.changed 

		Vector3Update pos;
		Vector3Update rot;
		Vector3Update scale;
		Vector3Axe axe;

		serializedObject.Update();

		// Position
		EditorGUILayout.BeginHorizontal();
		{
			Vector3Update forcePos = new Vector3Update() { UpdatedAxes = Vector3Axe.None };
			axe = GetMultipleValuesAxes(_posProp);

			if (DrawButton("P", "Reset Position", axe != Vector3Axe.None || IsResetPositionValid(trans), 20f))
			{
				NGUIEditorTools.RegisterUndo("Reset Position", serializedObject.targetObjects);
				forcePos.ForceSet(Vector3.zero);
				trans.localPosition = Vector3.zero;
				forceValue = true;
			}

			pos = DrawVector3(trans.localPosition, axe);

			if (forcePos.UpdatedAxes == Vector3Axe.All)
			{
				pos = forcePos;
			}
		}
		EditorGUILayout.EndHorizontal();

		// Rotation
		EditorGUILayout.BeginHorizontal();
		{
			Vector3Update forceRot = new Vector3Update() { UpdatedAxes = Vector3Axe.None };
			axe = GetMultipleValuesAxes(_rotProp);

			if (DrawButton("R", "Reset Rotation", axe != Vector3Axe.None ||  IsResetRotationValid(trans), 20f))
			{
				NGUIEditorTools.RegisterUndo("Reset Rotation", serializedObject.targetObjects);
				forceRot.ForceSet(Vector3.zero);
				trans.localEulerAngles = Vector3.zero;
				forceValue = true;
			}
			rot = DrawVector3(trans.localEulerAngles, axe);

			if (forceRot.UpdatedAxes == Vector3Axe.All)
			{
				rot = forceRot;
			}
		}
		EditorGUILayout.EndHorizontal();

		// Scale
		EditorGUILayout.BeginHorizontal();
		{
			Vector3Update forceScale = new Vector3Update() { UpdatedAxes = Vector3Axe.None };
			axe = GetMultipleValuesAxes(_sclProp);

			if (DrawButton("S", "Reset Scale", axe != Vector3Axe.None || IsResetScaleValid(trans), 20f))
			{
				NGUIEditorTools.RegisterUndo("Reset Scale", serializedObject.targetObjects);
				forceScale.ForceSet(Vector3.one);
				trans.localScale = Vector3.one;
				forceValue = true;
			}
			scale = DrawVector3(trans.localScale, axe);

			if (forceScale.UpdatedAxes == Vector3Axe.All)
			{
				scale = forceScale;
			}

		}
		EditorGUILayout.EndHorizontal();

		// If something changes, set the transform values for each object selected.
		// Apply changes on modified axes only.
		if (GUI.changed || forceValue)
		{
			NGUIEditorTools.RegisterUndo("Transform Change", serializedObject.targetObjects);

			pos.Validate();
			rot.Validate();
			scale.Validate();

			foreach (Object obj in serializedObject.targetObjects)
			{
				trans = obj as Transform;
				if (trans != null)
				{
					trans.localPosition = pos.GetUpdatedVector3(trans.localPosition);
					trans.localEulerAngles = rot.GetUpdatedVector3(trans.localEulerAngles);
					trans.localScale = scale.GetUpdatedVector3(trans.localScale);
				}
			}
		}
	}

	/// <summary>
	/// Helper function that draws a button in an enabled or disabled state.
	/// </summary>

	static bool DrawButton(string title, string tooltip, bool enabled, float width)
	{
		if (enabled)
		{
			// Draw a regular button
			return GUILayout.Button(new GUIContent(title, tooltip), GUILayout.Width(width));
		}
		else
		{
			// Button should be disabled -- draw it darkened and ignore its return value
			Color color = GUI.color;
			GUI.color = new Color(1f, 1f, 1f, 0.25f);
			GUILayout.Button(new GUIContent(title, tooltip), GUILayout.Width(width));
			GUI.color = color;
			return false;
		}
	}

	/// <summary>
	/// Helper function that draws a field of 3 floats.
	/// </summary>

	static Vector3Update DrawVector3(Vector3 value, Vector3Axe multipleValueAxes)
	{
		GUILayoutOption opt = GUILayout.MinWidth(30f);
		Vector3Update update = new Vector3Update() { Value = value };

		update.SetX(DrawFloatField("X", value.x, ((multipleValueAxes & Vector3Axe.X) == Vector3Axe.None), opt));
		update.SetY(DrawFloatField("Y", value.y, ((multipleValueAxes & Vector3Axe.Y) == Vector3Axe.None), opt));
		update.SetZ(DrawFloatField("Z", value.z, ((multipleValueAxes & Vector3Axe.Z) == Vector3Axe.None), opt));

		return update;
	}

	static float DrawFloatField(string name, float value, bool show, GUILayoutOption opt)
	{
		float result = value;
		if (show)
		{
			result = EditorGUILayout.FloatField(name, value, opt);
		}
		else
		{
			if (!float.TryParse(EditorGUILayout.TextField(name, "-", opt), out result))
			{
				result = value;
			}
		}
		return result;
	}

	/// <summary>
	/// Helper function that determines whether its worth it to show the reset position button.
	/// </summary>

	static bool IsResetPositionValid(Transform targetTransform)
	{
		Vector3 v = targetTransform.localPosition;
		return (v.x != 0f || v.y != 0f || v.z != 0f);
	}

	/// <summary>
	/// Helper function that determines whether its worth it to show the reset rotation button.
	/// </summary>

	static bool IsResetRotationValid(Transform targetTransform)
	{
		Vector3 v = targetTransform.localEulerAngles;
		return (v.x != 0f || v.y != 0f || v.z != 0f);
	}

	/// <summary>
	/// Helper function that determines whether its worth it to show the reset scale button.
	/// </summary>

	static bool IsResetScaleValid(Transform targetTransform)
	{
		Vector3 v = targetTransform.localScale;
		return (v.x != 1f || v.y != 1f || v.z != 1f);
	}

	/// <summary>
	/// Helper function that removes not-a-number values from the vector.
	/// </summary>

	static Vector3 Validate(Vector3 vector)
	{
		vector.x = float.IsNaN(vector.x) ? 0f : vector.x;
		vector.y = float.IsNaN(vector.y) ? 0f : vector.y;
		vector.z = float.IsNaN(vector.z) ? 0f : vector.z;
		return vector;
	}

	/// <summary>
	/// Gets the axes of a Vector3 which have multiple values.
	/// </summary>

	Vector3Axe GetMultipleValuesAxes(SerializedProperty property)
	{
		Vector3Axe axes = Vector3Axe.None;

		if (!property.hasMultipleDifferentValues)
		{
			return Vector3Axe.None;
		}

		// We know that we have at least one serialized object when this is called.
		Vector3 current = GetVector(property, serializedObject.targetObjects[0] as Transform);
		Vector3 next;

		// We check that the value of the axe are all the same.
		foreach (Object obj in serializedObject.targetObjects)
		{
			next = GetVector(property, obj as Transform);

			if (next.x != current.x) axes |= Vector3Axe.X;
			if (next.y != current.y) axes |= Vector3Axe.Y;
			if (next.z != current.z) axes |= Vector3Axe.Z;

			if (axes == Vector3Axe.All) return axes;
		}
		return axes;
	}

	/// <summary>
	/// Gets the vector of a transform (scale, position or eulerAngles) corresponding to a given property.
	/// </summary>

	public Vector3 GetVector(SerializedProperty property, Transform transform)
	{
		if (property == _rotProp) return transform.localEulerAngles;
		if (property == _posProp) return transform.localPosition;
		if (property == _sclProp) return transform.localScale;

		return Vector3.zero;
	}
}
