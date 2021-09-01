using ITCompanySimulation.Character;
using UnityEngine;
using CharacterController = ITCompanySimulation.Character.CharacterController;

namespace ITCompanySimulation.Render
{
    /// <summary>
    /// Renders character in the game world
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterRenderer : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private Animator CharacterAnimator;
        private static int[] AnimationClipHash;
        private SpriteRenderer Renderer;
        private CharacterController CharController;
        /// <summary>
        /// Current direction of character.
        /// </summary>
        private CharacterMovement CharMovement;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            CharacterAnimator = GetComponentInChildren<Animator>();
            Renderer = GetComponentInChildren<SpriteRenderer>();
            CharController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (null == AnimationClipHash)
            {
                AnimationClipHash = new int[CharacterAnimator.runtimeAnimatorController.animationClips.Length];

                for (int i = 0; i < CharacterAnimator.runtimeAnimatorController.animationClips.Length; i++)
                {
                    AnimationClipHash[i] = Animator.StringToHash(((CharacterMovement)i).ToString());
                }
            }

            SetCharaterDirection(CharController.CharacterDirection);
        }

        private void Update()
        {
            int sortingOrder = Mathf.RoundToInt(transform.position.y * (-100f));
            Renderer.sortingOrder = sortingOrder;

            if (CharMovement != CharController.CharacterDirection)
            {
                SetCharaterDirection(CharController.CharacterDirection);
            }
        }

        /*Public methods*/

        public void SetCharaterDirection(CharacterMovement direction)
        {
            CharMovement = direction;
            CharacterAnimator.Play(AnimationClipHash[(int)direction]);
        }
    }
}