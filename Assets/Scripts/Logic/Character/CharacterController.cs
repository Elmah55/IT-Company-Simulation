using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

namespace ITCompanySimulation.Character
{
    /// <summary>
    /// This class controls character in game world. None of characters
    /// will be controlled by player, all characters will be controlled by script
    /// </summary>
    public class CharacterController : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        /// <summary>
        /// Scripts that controls object towards target destination.
        /// </summary>
        private AIPath PathWalker;
        private AIDestinationSetter DestinationSetter;
        /// <summary>
        /// Game object that AI will move to.
        /// </summary>
        private GameObject TargetObject;
        /// <summary>
        /// Last time when character reached target object.
        /// </summary>
        private float TimeTargetObjectReached;
        /// <summary>
        /// Time (in seconds) until character will obtain new target
        /// and start moving towards it. This time will be counted from
        /// the moment previous target was reached. Sequence will look
        /// like that:
        /// [Move towards target] -> [Wait TimeUntilNewTarget seconds] -> [Move towards next target]
        /// </summary>
        private float TimeUntilNewTarget;
        [SerializeField]
        private GameObject TargetObjectPrefab;
        /// <summary>
        /// Number of all possible character movements.
        /// </summary>
        private int NumberOfCharacterMovements = Enum.GetValues(typeof(CharacterMovement)).Length;
        /// <summary>
        /// This dicitonary maps vector to character movement. Current character direction may not match perfectly
        /// vectors defined here so best matching vector will be found and character movement will be fetched passing
        /// best matching vector as key value.
        /// </summary>
        private static Dictionary<Vector2, CharacterMovement> VectorToDirection = new Dictionary<Vector2, CharacterMovement>()
        {
            {(Vector2.up+Vector2.left).normalized,CharacterMovement.WalkN},
            {(Vector2.down+Vector2.right).normalized,CharacterMovement.WalkS},
            {(Vector2.up+Vector2.right).normalized,CharacterMovement.WalkE},
            {(Vector2.down+Vector2.left).normalized,CharacterMovement.WalkW},
            {Vector2.up,CharacterMovement.WalkNE},
            {Vector2.left,CharacterMovement.WalkNW},
            {Vector2.down,CharacterMovement.WalkSW},
            {Vector2.right,CharacterMovement.WalkSE}
        };
        /// <summary>
        /// Indicates whether character movement is moving at the moment.
        /// </summary>
        private bool IsMoving;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Current direction of character.
        /// </summary>
        public CharacterMovement CharacterDirection { get; private set; }
        [Tooltip("Bounds that define possible coordinates for character's target object.")]
        public Bounds TargetObjectPositionBounds;

        /*Private methods*/

        private void Update()
        {
            CharacterDirection = GetCharacterMovement();

            if (((Time.time - TimeTargetObjectReached) >= TimeUntilNewTarget) && (false == IsMoving))
            {
                Vector2 newTargetPosition = GenerateTargetPosition();
                TargetObject.transform.position = newTargetPosition;
                DestinationSetter.target = TargetObject.transform;
                PathWalker.canSearch = true;
            }
        }

        private CharacterMovement GetCharacterMovement()
        {
            Vector2 currentDirection = PathWalker.desiredVelocity;
            //If character is standing do not change direction
            CharacterMovement movement = IsMoving ? CharacterMovement.StandN : CharacterDirection;

            if (currentDirection.magnitude >= 0.1f)
            {
                //Predefined vector whose direction best matches
                //current direction of object (has most similar direction).
                //Each predefined vector will be checked to find best
                //matching one so proper character movement can be found
                //(see CharacterMovement enum and VectorToDirection dictionary)
                Vector2 currentDirectionBestMatchVector = (Vector2.up + Vector2.left);
                float bestMatchDotProduct = 0f;

                foreach (var direction in VectorToDirection)
                {
                    float dotProduct = Vector2.Dot(direction.Key, currentDirection);

                    if (dotProduct > bestMatchDotProduct)
                    {
                        bestMatchDotProduct = dotProduct;
                        currentDirectionBestMatchVector = direction.Key;
                    }
                }

                movement = VectorToDirection[currentDirectionBestMatchVector];
                IsMoving = true;
            }
            else if (true == IsMoving)
            {
                //Movement vector magnitude is small enough to assume character
                //is not moving. In this case character will stand facing random
                //direction.
                movement = GetRandomStangindMovement();
                IsMoving = false;
            }

            return movement;
        }

        private void OnReachedTargetObject()
        {
            TimeTargetObjectReached = Time.time;
            TimeUntilNewTarget = UnityEngine.Random.Range(5f, 30f);
            PathWalker.canSearch = false;
        }

        private CharacterMovement GetRandomStangindMovement()
        {
            //8 last enum values are standing movements
            return (CharacterMovement)UnityEngine.Random.Range(NumberOfCharacterMovements - 8,
                                                               NumberOfCharacterMovements - 1);
        }

        private Vector2 GenerateTargetPosition()
        {
            float targetX = 
                UnityEngine.Random.Range(TargetObjectPositionBounds.min.x, TargetObjectPositionBounds.max.x);
            float targetY = 
                UnityEngine.Random.Range(TargetObjectPositionBounds.min.y, TargetObjectPositionBounds.max.y);
            Vector2 targetPosition = new Vector2(targetX, targetY);
            return targetPosition;
        }

        private void Awake()
        {
            DestinationSetter = GetComponent<AIDestinationSetter>();
            PathWalker = GetComponent<AIPath>();
            TargetObject = GameObject.Instantiate(TargetObjectPrefab);
            CharacterDirection = GetRandomStangindMovement();
        }

        private void OnDestroy()
        {
            GameObject.Destroy(TargetObject);
        }

        private void Start()
        {
            PathWalker.DestinationTargetReached += OnReachedTargetObject;
        }

        /*Public methods*/
    }
}