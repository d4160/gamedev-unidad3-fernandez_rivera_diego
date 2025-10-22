// Unity Game Developer Master - Solución de Normales para Outline
// Script: NormalsSolver.cs
// Objetivo: Crear una copia de una malla con normales perfectamente suavizadas
// para garantizar una extrusión de borde uniforme en cualquier geometría.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Herramienta de editor para generar una malla con normales suavizadas.
/// Esto es esencial para que los efectos de outline basados en extrusión de vértices
/// funcionen correctamente en modelos con bordes duros (ej. cubos).
/// </summary>
public static class NormalsSolver
{
    /// <summary>
    /// Crea un menú contextual para los assets de tipo Mesh en la ventana del proyecto.
    /// </summary>
    [MenuItem("Assets/Tools/Generate Smoothed Normals Mesh")]
    private static void GenerateSmoothedNormalsMesh()
    {
        // Obtiene la malla seleccionada en la ventana del proyecto.
        Mesh sourceMesh = Selection.activeObject as Mesh;
        if (sourceMesh == null)
        {
            EditorUtility.DisplayDialog("Error", "Por favor, seleccione un asset de tipo Mesh en la ventana del Proyecto.", "OK");
            return;
        }

        // Crea una nueva malla para no modificar la original.
        Mesh newMesh = new Mesh();
        newMesh.name = sourceMesh.name + "_SmoothNormals";

        // Copia los datos básicos de la malla original.
        newMesh.vertices = sourceMesh.vertices;
        newMesh.triangles = sourceMesh.triangles;
        newMesh.uv = sourceMesh.uv;
        newMesh.tangents = sourceMesh.tangents;

        // --- El núcleo de la lógica: Suavizado de Normales ---
        var groups = newMesh.vertices.Select((vertex, index) => new { vertex, index }).GroupBy(x => x.vertex);
        var smoothedNormals = new Vector3[newMesh.vertexCount];

        foreach (var group in groups)
        {
            // Calcula la normal promedio para este grupo de vértices que comparten posición.
            var smoothedNormal = Vector3.zero;
            foreach (var member in group)
            {
                // RecalculateNormals() nos da las normales "duras".
                // Aquí las promediamos.
                sourceMesh.RecalculateNormals(); // Necesitamos las normales originales por cara.
                smoothedNormal += sourceMesh.normals[member.index];
            }
            smoothedNormal.Normalize();

            // Asigna la nueva normal suavizada a todos los vértices del grupo.
            foreach (var member in group)
            {
                smoothedNormals[member.index] = smoothedNormal;
            }
        }
        
        newMesh.normals = smoothedNormals;

        // --- Guardado del nuevo asset de malla ---
        string path = AssetDatabase.GetAssetPath(sourceMesh);
        string newPath = path.Replace(".asset", "_SmoothNormals.asset").Replace(".fbx", "_SmoothNormals.asset").Replace(".obj", "_SmoothNormals.asset");
        
        AssetDatabase.CreateAsset(newMesh, newPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=lime>Malla con normales suavizadas generada en: {newPath}</color>", newMesh);
        EditorUtility.DisplayDialog("Éxito", $"Se ha creado una nueva malla con normales suavizadas en:\n{newPath}", "OK");
    }
}