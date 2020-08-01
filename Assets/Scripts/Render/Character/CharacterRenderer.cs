using UnityEngine;
using UnityEngine.Rendering;

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
        private SortingGroup CharacterSortingGroup;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            CharacterAnimator = GetComponent<Animator>();
            CharacterSortingGroup = GetComponent<SortingGroup>();
            AnimationClipHash = new int[CharacterAnimator.runtimeAnimatorController.animationClips.Length];

            for (int i = 0; i < CharacterAnimator.runtimeAnimatorController.animationClips.Length; i++)
            {
                AnimationClipHash[i] = Animator.StringToHash(((CharacterMovement)i).ToString());
            }
        }

        private void Update()
        {
            int sortingOrder = Mathf.RoundToInt(transform.position.y * (-100f));
            CharacterSortingGroup.sortingOrder = sortingOrder;
        }

        /*Public methods*/

        public void SetCharaterDirection(CharacterMovement direction)
        {
            CharacterAnimator.Play(AnimationClipHash[(int)direction]);
        }
    }
}