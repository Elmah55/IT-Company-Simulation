using UnityEngine;

namespace ITCompanySimulation.Character
{
    /// <summary>
    /// Renders character in the game world
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterRenderer : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private Animator CharacterAnimator;
        private int[] AnimationClipHash;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            CharacterAnimator = GetComponent<Animator>();
            AnimationClipHash = new int[CharacterAnimator.runtimeAnimatorController.animationClips.Length];

            for (int i = 0; i < CharacterAnimator.runtimeAnimatorController.animationClips.Length; i++)
            {
                AnimationClipHash[i] = Animator.StringToHash(((CharacterMovement)i).ToString());
            }
        }

        /*Public methods*/

        public void SetCharaterDirection(CharacterMovement direction)
        {
            CharacterAnimator.Play(AnimationClipHash[(int)direction]);
        }
    }
}