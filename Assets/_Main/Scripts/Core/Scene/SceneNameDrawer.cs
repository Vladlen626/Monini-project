using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace _Main.Scripts.Core
{
	public class SceneNameAttribute : PropertyAttribute
	{
	}
	
	[CustomPropertyDrawer(typeof(SceneNameAttribute))]
	public class SceneNameDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var constants = typeof(SceneNames)
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(f => f.FieldType == typeof(string))
				.Select(f => f.GetValue(null) as string)
				.ToArray();

			if (constants.Length == 0)
			{
				EditorGUI.PropertyField(position, property, label);
				return;
			}

			int currentIndex = Array.IndexOf(constants, property.stringValue);
			if (currentIndex < 0)
			{
				currentIndex = 0;
			}

			int newIndex = EditorGUI.Popup(position, label.text, currentIndex, constants);

			property.stringValue = constants[newIndex];
		}
	}
}