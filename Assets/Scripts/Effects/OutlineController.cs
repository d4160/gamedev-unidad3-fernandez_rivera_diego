// Unity Game Developer Master - Solución de Script Controlador (Revisión 1)
// Script: OutlineController.cs
// Objetivo: Controlar dinámicamente el shader de resaltado con depuración visual añadida.

using UnityEngine;
using System.Diagnostics;

/// <summary>
/// Gestiona el efecto de resaltado (outline) en un objeto.
/// Utiliza un MaterialPropertyBlock para una modificación eficiente de las propiedades del shader
/// sin instanciar materiales, optimizando el rendimiento.
/// Incluye un Gizmo para depuración visual en el editor.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class OutlineController : MonoBehaviour
{
    #region Variables
    
    [Header("Configuration")]
    [SerializeField]
    [Tooltip("El Renderer al que se aplicará el efecto. Si es nulo, se buscará en el GameObject.")]
    private Renderer targetRenderer;

    [Header("Outline Properties")]
    [SerializeField]
    [ColorUsage(true, true)]
    private Color outlineColor = new Color(1f, 0.5f, 0f, 1f);

    [SerializeField]
    [Range(0f, 0.1f)]
    private float outlineWidth = 0.02f;

    /// <summary>
    /// Propiedad pública para acceder y modificar el color del borde en tiempo de ejecución.
    /// </summary>
    public Color OutlineColor
    {
        get => outlineColor;
        set
        {
            outlineColor = value;
            if (_isOutlineActive)
            {
                ApplyOutlineProperties();
            }
        }
    }

    /// <summary>
    /// Propiedad pública para acceder y modificar el grosor del borde en tiempo de ejecución.
    /// </summary>
    public float OutlineWidth
    {
        get => outlineWidth;
        set
        {
            outlineWidth = Mathf.Clamp(value, 0f, 0.1f);
            if (_isOutlineActive)
            {
                ApplyOutlineProperties();
            }
        }
    }

    private MaterialPropertyBlock _propertyBlock;
    private bool _isOutlineActive = false;

    private static readonly int _OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int _OutlineWidthID = Shader.PropertyToID("_OutlineWidth");
    private static readonly int _OutlineIntensityID = Shader.PropertyToID("_OutlineIntensity");

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        Initialize();
    }

    // --- NUEVO: Gizmo de depuración visual ---
    /// <summary>
    /// Dibuja un gizmo en el editor cuando el objeto está seleccionado para indicar
    /// visualmente el bounds del renderer controlado por este script.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (targetRenderer == null)
        {
            // Intenta obtener el renderer si no está asignado, para visualización pre-runtime.
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null) return;
        }

        Gizmos.color = new Color(outlineColor.r, outlineColor.g, outlineColor.b, 0.6f);
        Gizmos.matrix = targetRenderer.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(targetRenderer.bounds.center - targetRenderer.transform.position, targetRenderer.bounds.size);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Muestra el borde de resaltado en el objeto con las propiedades actuales.
    /// </summary>
    public void ShowOutline()
    {
        if (_isOutlineActive) return;
        _isOutlineActive = true;
        ApplyOutlineProperties(1f);
    }

    /// <summary>
    /// Oculta el borde de resaltado en el objeto.
    /// </summary>
    public void HideOutline()
    {
        if (!_isOutlineActive) return;
        _isOutlineActive = false;
        ApplyOutlineProperties(0f);
    }
    
    #endregion

    #region Private Methods

    private void Initialize()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer == null)
        {
            Log.Error("[OutlineController] No se encontró un componente Renderer en este GameObject. El script no puede funcionar.", this);
            this.enabled = false;
            return;
        }

        _propertyBlock = new MaterialPropertyBlock();
        ApplyOutlineProperties(0f);
    }
    
    private void ApplyOutlineProperties(float intensity = -1f)
    {
        if (targetRenderer == null || _propertyBlock == null)
        {
            Log.Warning("[OutlineController] Intento de aplicar propiedades sin inicialización completa.", this);
            return;
        }
        
        targetRenderer.GetPropertyBlock(_propertyBlock);

        _propertyBlock.SetColor(_OutlineColorID, outlineColor);
        _propertyBlock.SetFloat(_OutlineWidthID, outlineWidth);

        if (intensity >= 0)
        {
            _propertyBlock.SetFloat(_OutlineIntensityID, intensity);
        }
        else
        {
            _propertyBlock.SetFloat(_OutlineIntensityID, _isOutlineActive ? 1f : 0f);
        }

        targetRenderer.SetPropertyBlock(_propertyBlock);
    }
    
    #endregion
}

/// <summary>
/// Clase de utilidad para logging contextualizado. (Sin cambios)
/// </summary>
public static class Log
{
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Info(string message, Object context = null) => UnityEngine.Debug.Log(message, context);
    
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Warning(string message, Object context = null) => UnityEngine.Debug.LogWarning(message, context);

    public static void Error(string message, Object context = null) => UnityEngine.Debug.LogError(message, context);
}