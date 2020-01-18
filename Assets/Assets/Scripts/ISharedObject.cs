/// <summary>
/// Interface for objects that can be shared between many clients
/// </summary>
public interface ISharedObject
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    /// <summary>
    /// ID used for identifying object 
    /// between different clients (ID of
    /// given object is same for all clients)
    /// </summary>
    int ID { get; set; }

    /*Private methods*/

    /*Public methods*/
}
