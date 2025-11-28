using UnityEngine;

/// <summary>
/// Stores and manages input values from the Input System.
/// </summary>
public class InputValueStorage
{
    #region Properties
    
    /// <summary>
    /// Gets the current stored input value.
    /// </summary>
    public Vector2 Value { get; private set; }
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// Initializes the storage with zero value.
    /// </summary>
    public InputValueStorage()
    {
        Value = Vector2.zero;
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Updates the stored input value.
    /// </summary>
    /// <param name="value">New input value to store.</param>
    public void UpdateValue(Vector2 value)
    {
        Value = value;
    }
    
    #endregion
}