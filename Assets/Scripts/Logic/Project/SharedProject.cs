using System;
using System.Collections.Generic;
using UnityEngine;
using ITCompanySimulation.Core;

namespace ITCompanySimulation.Project
{
    public class SharedProject : IdentifiableObject
    {
        /*Private consts fields*/

        /*Private fields*/

        private int m_CompletionTime;
        protected static ProjectGenerationData GenerationData;

        /*Public consts fields*/

        /*Public fields*/

        public string Name { get; set; }
        /// <summary>
        /// How much money company will receive after finishing project
        /// </summary>
        public int CompletionBonus { get; set; }
        /// <summary>
        /// In how many days project should be completed
        /// </summary>
        public int CompletionTime
        {
            get
            {
                return m_CompletionTime;
            }

            set
            {
                m_CompletionTime = value;
                CompletionTimeUpdated?.Invoke(this);
            }
        }
        /// <summary>
        /// After exceeding completion time company will have to pay penalty
        /// </summary>
        public int CompletionTimeExceededPenalty
        {
            get
            {
                return (int)(CompletionBonus * 0.25f);
            }
        }
        /// <summary>
        /// Stores index of name from project's name table.
        /// Used for serialization
        /// </summary>
        public int NameIndex { get; set; }
        /// <summary>
        /// UI icon associated with this project
        /// </summary>
        public Sprite Icon { get; set; }
        /// <summary>
        /// Stores index of name from project's icon table.
        /// Used for serialization
        /// </summary>
        public int IconIndex { get; set; }
        public List<ProjectTechnology> UsedTechnologies { get; set; }
        public event SharedProjectAction CompletionTimeUpdated;

        /*Private methods*/

        /*Public methods*/

        public static byte[] Serialize(object projectObject)
        {
            SharedProject projectToSerialize = (SharedProject)projectObject;

            byte[] nameIndexBytes = BitConverter.GetBytes(projectToSerialize.NameIndex);
            byte[] iconIndexBytes = BitConverter.GetBytes(projectToSerialize.IconIndex);
            byte[] IDBytes = BitConverter.GetBytes(projectToSerialize.ID);
            byte[] completeBonusBytes = BitConverter.GetBytes(projectToSerialize.CompletionBonus);
            byte[] completionTimeBytes = BitConverter.GetBytes(projectToSerialize.CompletionTime);
            byte[] technologiesBytes = new byte[projectToSerialize.UsedTechnologies.Count];

            for (int i = 0; i < projectToSerialize.UsedTechnologies.Count; i++)
            {
                technologiesBytes[i] = (byte)projectToSerialize.UsedTechnologies[i];
            }

            //Used to store number of bytes used for technologies
            byte[] technologiesBytesSize = BitConverter.GetBytes(technologiesBytes.Length);

            int projectBytesSize = nameIndexBytes.Length
                                 + iconIndexBytes.Length
                                 + IDBytes.Length
                                 + completeBonusBytes.Length
                                 + completionTimeBytes.Length
                                 + technologiesBytes.Length
                                 + technologiesBytesSize.Length;

            byte[] projectBytes = new byte[projectBytesSize];
            int offset = 0;

            Array.Copy(nameIndexBytes, 0, projectBytes, offset, nameIndexBytes.Length);
            offset += nameIndexBytes.Length;
            Array.Copy(iconIndexBytes, 0, projectBytes, offset, iconIndexBytes.Length);
            offset += iconIndexBytes.Length;
            Array.Copy(IDBytes, 0, projectBytes, offset, IDBytes.Length);
            offset += IDBytes.Length;
            Array.Copy(completeBonusBytes, 0, projectBytes, offset, completeBonusBytes.Length);
            offset += completeBonusBytes.Length;
            Array.Copy(completionTimeBytes, 0, projectBytes, offset, completionTimeBytes.Length);
            offset += completionTimeBytes.Length;
            Array.Copy(technologiesBytesSize, 0, projectBytes, offset, technologiesBytesSize.Length);
            offset += technologiesBytesSize.Length;
            Array.Copy(technologiesBytes, 0, projectBytes, offset, technologiesBytes.Length);

            return projectBytes;
        }

        public static object Deserialize(byte[] projectBytes)
        {
            if (null == GenerationData)
            {
                GenerationData = SimulationManager.Instance.gameObject.GetComponent<ProjectsMarket>().GenerationData;
            }

            int offset = 0;
            int nameIndex;
            int iconIndex;
            int ID;
            int completeBonus;
            int completionTime;
            int technologiesSize;
            List<ProjectTechnology> technologies = new List<ProjectTechnology>();

            nameIndex = BitConverter.ToInt32(projectBytes, offset);
            offset += sizeof(int);
            iconIndex = BitConverter.ToInt32(projectBytes, offset);
            offset += sizeof(int);
            ID = BitConverter.ToInt32(projectBytes, offset);
            offset += sizeof(int);
            completeBonus = BitConverter.ToInt32(projectBytes, offset);
            offset += sizeof(int);
            completionTime = BitConverter.ToInt32(projectBytes, offset);
            offset += sizeof(int);
            technologiesSize = BitConverter.ToInt32(projectBytes, offset);
            offset += sizeof(int);

            for (int i = 0; i < technologiesSize; i++)
            {
                ProjectTechnology technology = (ProjectTechnology)projectBytes[offset];
                technologies.Add(technology);
                offset += sizeof(byte);
            }

            SharedProject deserializedProject = new SharedProject(GenerationData.Names[nameIndex]);
            deserializedProject.ID = ID;
            deserializedProject.CompletionBonus = completeBonus;
            deserializedProject.UsedTechnologies = technologies;
            deserializedProject.CompletionTime = completionTime;
            deserializedProject.IconIndex = iconIndex;
            deserializedProject.Icon = GenerationData.Icons[iconIndex];
            ;
            return deserializedProject;
        }

        public SharedProject(string projectName)
        {
            if (null == GenerationData)
            {
                GenerationData = SimulationManager.Instance.gameObject.GetComponent<ProjectsMarket>().GenerationData;
            }

            this.Name = projectName;
            UsedTechnologies = new List<ProjectTechnology>();
        }
    }
}
