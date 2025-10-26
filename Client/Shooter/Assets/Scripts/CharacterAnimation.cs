using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private const string Grounded = "Grounded";
    private const string Speed = "Speed";
    private const string Crouch = "isCrouching";

    [SerializeField] private Animator _animatorFoot;
    [SerializeField] private Animator _animatorCrouch;
    [SerializeField] private CheckFly _checkFly;
    [SerializeField] private Character _character;

    private void Update() {

        Vector3 localVelocity = _character.transform.InverseTransformVector(_character.velocity);
        float speed = localVelocity.magnitude / _character.speed;
        float sing = Mathf.Sign(localVelocity.z);

        _animatorFoot.SetFloat(Speed, speed * sing);
        _animatorFoot.SetBool(Grounded,_checkFly.IsFly == false);

        _animatorCrouch.SetBool(Crouch, _character.GetIsCrouching()); 

    }
}
