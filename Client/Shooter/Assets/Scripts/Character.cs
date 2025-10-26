using UnityEngine;
using UnityEngine.TextCore.Text;

public abstract class Character : MonoBehaviour
{
    [field: SerializeField] public float speed { get; protected set; } = 2f;
    public Vector3 velocity { get; protected set; }

    protected bool _isCrouching = false;
    public virtual bool GetIsCrouching() => _isCrouching;
}
