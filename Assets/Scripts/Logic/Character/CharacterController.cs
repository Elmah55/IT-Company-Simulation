using System.Collections.Generic;
using UnityEngine;

namespace ITCompanySimulation.Character
{
    /// <summary>
    /// This class controls character in game world. None of characters
    /// will be controlled by player, all characters will be controlled by script
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CharacterRenderer))]
    public class CharacterController : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private Rigidbody2D CharRigidbody;
        private CharacterRenderer CharRenderer;

        #region Character movement related variables 

        /// <summary>
        /// This dicitonary maps direction in game world to 2D vector. Since only 4 vector movements will be allowed
        /// (no diagonal movement) and X and Y vector are diagonal, result vector will be sum of X and Y vector. Sum vector
        /// is considered straight vector in game world coordinates (front, back, rigth and left of character point of view)
        /// 
        ///                                   X                    Sum
        /// 
        ///                                    ^                   /
        ///                                    |                /
        ///                                    |             /
        ///                                    |          /
        ///                                    |       /
        ///                                    |    /
        ///                                    | /
        ///                                    |---------------------> Y
        ///                                    
        /// TODO: Calculate these vector based on tilemap tile's postion (vector between 2 tiles positon)
        /// </summary>
        private static Dictionary<CharacterMovement, Vector2> DirectionToVector2 = new Dictionary<CharacterMovement, Vector2>()
        {
            {CharacterMovement.RunN,(Vector2.up+Vector2.left).normalized},
            {CharacterMovement.RunS,(Vector2.down+Vector2.right).normalized},
            {CharacterMovement.RunE,(Vector2.up+Vector2.right).normalized},
            {CharacterMovement.RunW,(Vector2.down+Vector2.left).normalized},
            {CharacterMovement.StandE,Vector2.zero },
            {CharacterMovement.StandN,Vector2.zero },
            {CharacterMovement.StandS,Vector2.zero },
            {CharacterMovement.StandW,Vector2.zero }
        };
        /// <summary>
        /// Vector that will be applied to character's rigidbody
        /// </summary>
        private Vector2 CurrentMovementVector;
        /// <summary>
        /// Current's postion of character's rigidbody
        /// </summary>
        private Vector2 CurrentPosition;
        [SerializeField]
        [Range(0.1f, 10f)]
        private float MovementSpeed = 5f;
        /// <summary>
        /// Whether character should not move
        /// </summary>
        private bool Moving;
        /// <summary>
        /// Time (in seconds) for how long character should not move
        /// </summary>
        private float NotMovingTime;
        /// <summary>
        /// After this time from 'LastNotMovingStop' character should 
        /// start not moving for 'NotMovingTime' amount of time
        /// </summary>
        private float StartNotMovingTime;
        /// <summary>
        /// Seconds since game start when character started not moving last time
        /// </summary>
        private float LastNotMovingStart;
        /// <summary>
        /// Seconds since game start when character stopped not moving last time
        /// </summary>
        private float LastNotMovingStop;
        /// <summary>
        /// How long ago (in seconds) character changed his direction
        /// </summary>
        private float LastChangeOfDirectionTime;
        /// <summary>
        /// How many seconds should pass from 'LastChangeOfDirectionTime' to character
        /// change direction
        /// </summary>
        private float ChangeOfDirectionTime;
        /// <summary>
        /// The direction that character should face when standing
        /// </summary>
        private CharacterMovement CharacterLastDirection;
        private Vector2 CharacterLastVector;

        #endregion

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void OnCollisionEnter(Collision collision)
        {
            //TODO: Add avoiding obstacles
        }

        private void FixedUpdate()
        {
            if (true == Moving)
            {
                HandleMovementMoving();
            }
            else
            {
                HandleMovementNotMoving();
            }
        }

        private void HandleMovementNotMoving()
        {
            //If not moving time is exceeded its time to move
            if ((Time.time - LastNotMovingStart) >= NotMovingTime)
            {
                LastNotMovingStop = Time.time;
                StartNotMovingTime = Random.Range(4f, 9f);
                Moving = true;
            }
        }

        private void HandleMovementMoving()
        {
            //Its time to stand for a while
            if ((Time.time - LastNotMovingStop) >= StartNotMovingTime)
            {
                Moving = false;
                NotMovingTime = Random.Range(4f, 7f);
                LastNotMovingStart = Time.time;
                //+4 is offset to get standing movement of given direction i.e (RunW + 4) -> StandW
                CharacterMovement standingDirection = (CharacterMovement)((int)CharacterLastDirection + 4);
                CharRenderer.SetCharaterDirection(standingDirection);
            }

            //Time to change direction
            if ((Time.time - LastChangeOfDirectionTime) >= ChangeOfDirectionTime)
            {
                ChangeOfDirectionTime = Random.Range(3f, 5f);
                int randomDirectionIndex = Random.Range(0, 4);
                CharacterMovement randomDirection = (CharacterMovement)randomDirectionIndex;
                CharacterLastDirection = randomDirection;
                LastChangeOfDirectionTime = Time.time;
            }

            CurrentPosition = CharRigidbody.position;
            Vector2 movementVec = DirectionToVector2[CharacterLastDirection];
            CharacterLastVector = CurrentPosition + movementVec * MovementSpeed * Time.fixedDeltaTime;
            CharRenderer.SetCharaterDirection(CharacterLastDirection);
            CharRigidbody.MovePosition(CharacterLastVector);
        }

        private void Start()
        {
            CharRigidbody = GetComponent<Rigidbody2D>();
            CharRenderer = GetComponent<CharacterRenderer>();
            CharRenderer.SetCharaterDirection(CharacterMovement.StandN);
        }

        /*Public methods*/
    }
}
