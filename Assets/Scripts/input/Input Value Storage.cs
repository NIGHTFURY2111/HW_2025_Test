using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Stores and manages input values from the Input System.
/// </summary>
public class InputValueStorage
{
    public Vector2 Value { get; private set; }

    public InputValueStorage()
    {
        Value = Vector2.zero;
    }

    public void UpdateValue(Vector2 value) => Value = value;

    public Vector2 GetValue => Value;
}