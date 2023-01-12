using System;
using System.Collections.Generic;
using UnityEngine;
using ITCompanySimulation.Utilities;
using ITCompanySimulation.Project;
using ITCompanySimulation.Core;

namespace ITCompanySimulation.Character
{
    public class SharedWorker : Character
    {
        /*Private consts fields*/

        /*Private fields*/

        private int m_Salary;
        private int m_ExpierienceTime;
        protected static WorkerGenerationData GenerationData;

        /*Public consts fields*/

        public const int BASE_SALARY = 800;
        /// <summary>
        /// The maximum value of one ability that worker
        /// can have
        /// </summary>
        public const float MAX_ABILITY_VALUE = 10.0f;

        /*Public fields*/

        /// <summary>
        /// Shows how much of time of experience working in project worker has.
        /// This values is days in game time
        /// </summary>
        public int ExperienceTime
        {
            get
            {
                return m_ExpierienceTime;
            }

            set
            {
                if (value != m_ExpierienceTime)
                {
                    m_ExpierienceTime = value;
                    ExpierienceTimeChanged?.Invoke(this);
                }
            }
        }
        public virtual int Salary
        {
            get
            {
                return m_Salary;
            }

            set
            {
                if (m_Salary != value)
                {
                    m_Salary = value;
                    SalaryChanged?.Invoke(this);
                }
            }
        }
        /// <summary>
        /// Salary that needs to be offered to worker to hire him
        /// from other player's company
        /// </summary>
        public int HireSalary
        {
            get
            {
                return (int)(Salary * 1.25f);
            }
        }
        /// <summary>
        /// List of abilites that worker has. Each ability can be on different level
        /// depending on worker experience. Ability level is indicated by value assigned
        /// to each of abilities key
        /// </summary>
        public Dictionary<ProjectTechnology, SafeFloat> Abilites { get; set; }
        /// <summary>
        /// Representation of character in game world
        /// </summary>
        public GameObject PhysicalCharacter { get; set; }
        /// <summary>
        /// Avatar to visualize worker in UI
        /// </summary>
        public Sprite Avatar { get; set; }
        /// <summary>
        /// Indexes will be used for serialization to reduce
        /// amount of data sent to other clients
        /// </summary>
        public int NameIndex { get; set; }
        public int SurenameIndex { get; set; }
        public int AvatarIndex { get; set; }
        public event SharedWorkerAction SalaryChanged;
        public event SharedWorkerAction ExpierienceTimeChanged;
        public event WorkerAbilityAction AbilityUpdated;

        /*Private methods*/

        /*Public methods*/

        public SharedWorker(string name, string surename, Gender gender) : base(name, surename, gender)
        {
            if (null == GenerationData)
            {
                GenerationData = SimulationManager.Instance.gameObject.GetComponent<WorkersMarket>().GenerationData;
            }

            this.Salary = BASE_SALARY;
        }

        public SharedWorker(SharedWorker worker) : this(worker.Name, worker.Surename, worker.Gender)
        {
            this.ID = worker.ID;
            this.Salary = worker.Salary;
            this.Abilites = worker.Abilites;
            this.ExperienceTime = worker.ExperienceTime;
            this.Avatar = worker.Avatar;
            this.NameIndex = worker.NameIndex;
            this.SurenameIndex = worker.SurenameIndex;
            this.AvatarIndex = worker.AvatarIndex;
        }

        /// <summary>
        /// Increases given ability of worker by provided value. In case worker does
        /// not have given ability it will be added to his abilites list and value of
        /// ability will be set to provided value
        /// </summary>
        public void UpdateAbility(ProjectTechnology ability, float abilityValue)
        {
            if (true == Abilites.ContainsKey(ability))
            {
                SafeFloat abilityCurrentValue = Abilites[ability];
                float abilityNewValue = abilityCurrentValue.Value + abilityValue;
                abilityNewValue = Mathf.Clamp(abilityNewValue, 0f, MAX_ABILITY_VALUE);
                Abilites[ability] = new SafeFloat(abilityNewValue);
            }
            else
            {
                Abilites.Add(ability, new SafeFloat(abilityValue));
            }

            AbilityUpdated?.Invoke(this, ability, abilityValue);
        }

        public static byte[] Serialize(object workerObject)
        {
            SharedWorker workerToSerialize = (SharedWorker)workerObject;

            byte[] nameIndexBytes = BitConverter.GetBytes(workerToSerialize.NameIndex);
            byte[] surenameIndexBytes = BitConverter.GetBytes(workerToSerialize.SurenameIndex);
            byte[] avatarIndexBytes = BitConverter.GetBytes(workerToSerialize.AvatarIndex);
            byte[] expierienceTimeBytes = BitConverter.GetBytes(workerToSerialize.ExperienceTime);
            byte[] salaryBytes = BitConverter.GetBytes(workerToSerialize.Salary);
            byte[] abilitiesBytes;
            byte[] IDBytes = BitConverter.GetBytes(workerToSerialize.ID);
            byte[] genderBytes = BitConverter.GetBytes((int)workerToSerialize.Gender);

            //This is used so receiving client knows how much bytes of name and surename should be read
            byte[] abilitiesSize = BitConverter.GetBytes(workerToSerialize.Abilites.Count);
            //Serialize ability type and its value (2*size)
            int abilitiesSizeInBytes = workerToSerialize.Abilites.Count * 2 * sizeof(int);
            abilitiesBytes = new byte[abilitiesSizeInBytes];

            int abilitiesBytesOffset = 0;
            foreach (var ability in workerToSerialize.Abilites)
            {
                byte[] abilityTypeBytes = BitConverter.GetBytes((int)ability.Key);
                byte[] abilityValueBytes = BitConverter.GetBytes(ability.Value.Value);

                Array.Copy(abilityTypeBytes, 0, abilitiesBytes, abilitiesBytesOffset, abilityTypeBytes.Length);
                abilitiesBytesOffset += abilityTypeBytes.Length;
                Array.Copy(abilityValueBytes, 0, abilitiesBytes, abilitiesBytesOffset, abilityValueBytes.Length);
                abilitiesBytesOffset += abilityValueBytes.Length;
            }

            int workerBytesSize = nameIndexBytes.Length
                                + surenameIndexBytes.Length
                                + avatarIndexBytes.Length
                                + abilitiesSize.Length
                                + abilitiesBytes.Length
                                + expierienceTimeBytes.Length
                                + salaryBytes.Length
                                + IDBytes.Length
                                + genderBytes.Length;

            byte[] workerBytes = new byte[workerBytesSize];
            int offset = 0;

            Array.Copy(nameIndexBytes, 0, workerBytes, offset, nameIndexBytes.Length);
            offset += nameIndexBytes.Length;
            Array.Copy(surenameIndexBytes, 0, workerBytes, offset, surenameIndexBytes.Length);
            offset += surenameIndexBytes.Length;
            Array.Copy(avatarIndexBytes, 0, workerBytes, offset, avatarIndexBytes.Length);
            offset += avatarIndexBytes.Length;
            Array.Copy(abilitiesSize, 0, workerBytes, offset, abilitiesSize.Length);
            offset += abilitiesSize.Length;
            Array.Copy(abilitiesBytes, 0, workerBytes, offset, abilitiesBytes.Length);
            offset += abilitiesBytes.Length;
            Array.Copy(expierienceTimeBytes, 0, workerBytes, offset, expierienceTimeBytes.Length);
            offset += expierienceTimeBytes.Length;
            Array.Copy(salaryBytes, 0, workerBytes, offset, expierienceTimeBytes.Length);
            offset += salaryBytes.Length;
            Array.Copy(IDBytes, 0, workerBytes, offset, IDBytes.Length);
            offset += IDBytes.Length;
            Array.Copy(genderBytes, 0, workerBytes, offset, genderBytes.Length);

            return workerBytes;
        }

        public static object Deserialize(byte[] workerBytes)
        {
            if (null == GenerationData)
            {
                GenerationData = SimulationManager.Instance.gameObject.GetComponent<WorkersMarket>().GenerationData;
            }

            int offset = 0;

            int nameIndex = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);
            int surenameIndex = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);
            int avatarIndex = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);
            int abilitiesSize = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);

            Dictionary<ProjectTechnology, SafeFloat> abilities = new Dictionary<ProjectTechnology, SafeFloat>();

            for (int i = 0; i < abilitiesSize; i++)
            {
                ProjectTechnology abilityType = (ProjectTechnology)BitConverter.ToInt32(workerBytes, offset);
                offset += sizeof(int);
                float abilityValue = BitConverter.ToSingle(workerBytes, offset);
                offset += sizeof(float);
                abilities.Add(abilityType, new SafeFloat(abilityValue));
            }

            int expierienceTime = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);
            int salary = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);
            int workerID = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);
            Gender workerGender = (Gender)BitConverter.ToInt32(workerBytes, offset);

            string name = (Gender.Male == workerGender) ? GenerationData.MaleNames[nameIndex] : GenerationData.FemaleNames[nameIndex];
            string surename = GenerationData.Surenames[surenameIndex];
            Sprite avatar = (Gender.Male == workerGender) ? GenerationData.MaleCharactersAvatars[avatarIndex] : GenerationData.FemaleCharactersAvatars[avatarIndex];

            SharedWorker deserializedWorker = new SharedWorker(name, surename, workerGender);

            deserializedWorker.Abilites = abilities;
            deserializedWorker.ExperienceTime = expierienceTime;
            deserializedWorker.Salary = salary;
            deserializedWorker.ID = workerID;
            deserializedWorker.NameIndex = nameIndex;
            deserializedWorker.SurenameIndex = surenameIndex;
            deserializedWorker.AvatarIndex = avatarIndex;
            deserializedWorker.Avatar = avatar;

            return deserializedWorker;
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }
}
