using ITCompanySimulation.Render;
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
        private static Dictionary<CharacterMovement, Vector2> DirectionToVector = new Dictionary<CharacterMovement, Vector2>()
        {
            {CharacterMovement.WalkN,(Vector2.up+Vector2.left).normalized},
            {CharacterMovement.WalkS,(Vector2.down+Vector2.right).normalized},
            {CharacterMovement.WalkE,(Vector2.up+Vector2.right).normalized},
            {CharacterMovement.WalkW,(Vector2.down+Vector2.left).normalized},
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            //TODO: Improve avoiding obstacles. Right now character
            //sometimes get stuck or walks into the obstacle

            //This variable will hold longest distance to collider hit by ray.
            //Raycast will be executed in every direction except the one that
            //character is facing currently. When characters hits collider, 
            //direction with longest possible movement without colliding will be
            //found and character will start moving that direction
            float longestDistance = float.MinValue;
            CharacterMovement bestDirection = CharacterMovement.WalkE;

            for (int i = 0; i < 4; i++)
            {
                CharacterMovement direction = (CharacterMovement)i;

                if (CharacterLastDirection == direction)
                {
                    continue;
                }

                ContactPoint2D contact = collision.GetContact(0);
                Vector2 rayDirection = DirectionToVector[direction];

                RaycastHit2D hit = Physics2D.Raycast(contact.point, rayDirection);

                if (hit.distance >= longestDistance)
                {
                    longestDistance = hit.distance;
                    bestDirection = direction;
                }
            }

            ChangeDirection(bestDirection);
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
                StopMoving();
            }

            //Time to change direction
            if ((Time.time - LastChangeOfDirectionTime) >= ChangeOfDirectionTime)
            {
                ChangeDirectionRandom();
            }

            CurrentPosition = CharRigidbody.position;
            Vector2 movementVec = DirectionToVector[CharacterLastDirection];
            CharacterLastVector = CurrentPosition + movementVec * MovementSpeed * Time.fixedDeltaTime;
            CharRenderer.SetCharaterDirection(CharacterLastDirection);
            CharRigidbody.MovePosition(CharacterLastVector);
        }

        /// <summary>
        /// Changes character's to specified direction
        /// </summary>
        private void ChangeDirection(CharacterMovement dir)
        {
            ChangeOfDirectionTime = Random.Range(3f, 5f);
            CharacterLastDirection = dir;
            LastChangeOfDirectionTime = Time.time;
        }

        /// <summary>
        /// Changes character's direction to random direction
        /// </summary>
        private void ChangeDirectionRandom()
        {
            int randomDirectionIndex = Random.Range(0, 4);
            CharacterMovement randomDirection = (CharacterMovement)randomDirectionIndex;
            ChangeDirection(randomDirection);
        }

        private void StopMoving()
        {
            Moving = false;
            NotMovingTime = Random.Range(4f, 7f);
            LastNotMovingStart = Time.time;
            //+4 is offset to get standing movement of given direction i.e (RunW + 4) -> StandW
            CharacterMovement standingDirection = (CharacterMovement)((int)CharacterLastDirection + 4);
            CharRenderer.SetCharaterDirection(standingDirection);
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
