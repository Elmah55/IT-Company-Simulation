using ITCompanySimulation.Character;
using UnityEngine;

namespace ITCompanySimulation.Render
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
        private SpriteRenderer Renderer;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            CharacterAnimator = GetComponent<Animator>();
            Renderer = GetComponent<SpriteRenderer>();
            AnimationClipHash = new int[CharacterAnimator.runtimeAnimatorController.animationClips.Length];

            for (int i = 0; i < CharacterAnimator.runtimeAnimatorController.animationClips.Length; i++)
            {
                AnimationClipHash[i] = Animator.StringToHash(((CharacterMovement)i).ToString());
            }
        }

        private void Update()
        {
            int sortingOrder = Mathf.RoundToInt(transform.position.y * (-100f));
            Renderer.sortingOrder = sortingOrder;
        }

        /*Public methods*/

        public void SetCharaterDirection(CharacterMovement direction)
        {
            CharacterAnimator.Play(AnimationClipHash[(int)direction]);
        }
    }
}