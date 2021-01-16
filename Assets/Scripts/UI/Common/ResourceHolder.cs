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

        public Sprite[] MaleCharactersAvatars;
        public Sprite[] FemaleCharactersAvatars;
        //Index in this arrays should match index in project names array
        public Sprite[] ProjectsIcons;

        /*Private methods*/

        /*Public methods*/
    }
}
