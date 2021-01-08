using UnityEngine;

/// <summary>
/// Component used to store different types of resources. This component is used to access resources via 
/// serialized fields instead of loading them from storage manually
/// </summary>

namespace ITCompanySimulation.UI
{
    public class ResourceHolder : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        public Sprite[] MaleCharactersAvatars
        {
            get
            {
                return m_MaleCharactersAvatars;
            }
        }
        public Sprite[] FemaleCharactersAvatars
        {
            get
            {
                return m_FemaleCharactersAvatars;
            }
        }

        /*Private methods*/

        [SerializeField]
        private Sprite[] m_MaleCharactersAvatars;
        [SerializeField]
        private Sprite[] m_FemaleCharactersAvatars;

        /*Public methods*/
    }
}
