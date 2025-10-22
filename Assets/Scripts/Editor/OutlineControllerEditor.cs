// Unity Game Developer Master - Solución de Editor Personalizado
// Script: OutlineControllerEditor.cs
// Objetivo: Mejorar la usabilidad del OutlineController en el Inspector de Unity.

using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor personalizado para el componente OutlineController.
/// Añade botones para probar el efecto directamente desde el Inspector,
/// mejorando el flujo de trabajo y la depuración visual.
/// </summary>
[CustomEditor(typeof(OutlineController))]
public class OutlineControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Dibuja el inspector por defecto (variables públicas/serializadas).
        base.OnInspectorGUI();

        // Separador para la sección de herramientas.
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debugging Tools", EditorStyles.boldLabel);

        // Obtiene la referencia al script que estamos inspeccionando.
        OutlineController controller = (OutlineController)target;

        // Dibuja los botones en una fila horizontal.
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Show Outline"))
        {
            // Llama al método público del controlador.
            controller.ShowOutline();
        }

        if (GUILayout.Button("Hide Outline"))
        {
            // Llama al método público del controlador.
            controller.HideOutline();
        }

        EditorGUILayout.EndHorizontal();
    }
}