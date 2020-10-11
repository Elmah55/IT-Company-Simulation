using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace ITCompanySimulation.Character
{
    public class SharedWorker : Character
    {
        /*Private consts fields*/

        /*Private fields*/

        private int m_Salary;
        private int m_ExpierienceTime;

        /*Public consts fields*/

        public const int BASE_SALARY = 800;

        /*Public fields*/

        /// <summary>
        /// Unique ID of worker
        /// </summary>
        public int ID { get; set; }
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
        public Dictionary<ProjectTechnology, float> Abilites { get; set; }
        /// <summary>
        /// Representation of character in game world
        /// </summary>
        public GameObject PhysicalCharacter { get; set; }
        public event SharedWorkerAction SalaryChanged;
        public event SharedWorkerAction ExpierienceTimeChanged;
        public event WorkerAbilityAction AbilityUpdated;

        /*Private methods*/

        /*Public methods*/

        public SharedWorker(string name, string surename, Gender gender) : base(name, surename, gender)
        {
            this.Salary = BASE_SALARY;
        }

        public SharedWorker(SharedWorker worker) : this(worker.Name, worker.Surename, worker.Gender)
        {
            this.ID = worker.ID;
            this.Salary = worker.Salary;
            this.Abilites = worker.Abilites;
            this.ExperienceTime = worker.ExperienceTime;
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
                Abilites[ability] += abilityValue;
            }
            else
            {
                Abilites.Add(ability, abilityValue);
            }

            AbilityUpdated?.Invoke(this, ability, abilityValue);
        }

        public static byte[] Serialize(object workerObject)
        {
            SharedWorker workerToSerialize = (SharedWorker)workerObject;

            //TODO: send string index instead of sending an actual string
            byte[] nameBytes = Encoding.Unicode.GetBytes(workerToSerialize.Name);
            byte[] surenameBytes = Encoding.Unicode.GetBytes(workerToSerialize.Surename);
            byte[] expierienceTimeBytes = BitConverter.GetBytes(workerToSerialize.ExperienceTime);
            byte[] salaryBytes = BitConverter.GetBytes(workerToSerialize.Salary);
            byte[] abilitiesBytes;
            byte[] IDBytes = BitConverter.GetBytes(workerToSerialize.ID);
            byte[] genderBytes = BitConverter.GetBytes((int)workerToSerialize.Gender);

            //This is used so receiving client knows how much bytes of name and surename should be read
            byte[] nameSize = BitConverter.GetBytes(nameBytes.Length);
            byte[] surenameSize = BitConverter.GetBytes(surenameBytes.Length);
            byte[] abilitiesSize;

            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memStream, workerToSerialize.Abilites);
                abilitiesBytes = memStream.ToArray();
            }

            abilitiesSize = BitConverter.GetBytes(abilitiesBytes.Length);

            int workerBytesSize = nameBytes.Length
                                + surenameBytes.Length
                                + nameSize.Length
                                + surenameSize.Length
                                + abilitiesSize.Length
                                + abilitiesBytes.Length
                                + expierienceTimeBytes.Length
                                + salaryBytes.Length
                                + IDBytes.Length
                                + genderBytes.Length;

            byte[] workerBytes = new byte[workerBytesSize];
            int offset = 0;

            Array.Copy(nameSize, 0, workerBytes, offset, nameSize.Length);
            offset += nameSize.Length;
            Array.Copy(nameBytes, 0, workerBytes, offset, nameBytes.Length);
            offset += nameBytes.Length;
            Array.Copy(surenameSize, 0, workerBytes, offset, surenameSize.Length);
            offset += surenameSize.Length;
            Array.Copy(surenameBytes, 0, workerBytes, offset, surenameBytes.Length);
            offset += surenameBytes.Length;
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
            int offset = 0;

            int nameSize = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);
            string name = Encoding.Unicode.GetString(workerBytes, offset, nameSize);
            offset += nameSize;
            int surenameSize = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);
            string surename = Encoding.Unicode.GetString(workerBytes, offset, surenameSize);
            offset += surenameSize;

            int abilitiesSize = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);

            Dictionary<ProjectTechnology, float> abilities;

            using (MemoryStream memStream = new MemoryStream(workerBytes, offset, abilitiesSize))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                abilities = (Dictionary<ProjectTechnology, float>)formatter.Deserialize(memStream);
            }

            offset += abilitiesSize;
            int expierienceTime = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);
            int salary = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);
            int workerID = BitConverter.ToInt32(workerBytes, offset);
            offset += sizeof(int);
            Gender workerGender = (Gender)BitConverter.ToInt32(workerBytes, offset);

            SharedWorker deserializedWorker = new SharedWorker(name, surename, workerGender);

            deserializedWorker.Abilites = abilities;
            deserializedWorker.ExperienceTime = expierienceTime;
            deserializedWorker.Salary = salary;
            deserializedWorker.ID = workerID;

            return deserializedWorker;
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }
}
