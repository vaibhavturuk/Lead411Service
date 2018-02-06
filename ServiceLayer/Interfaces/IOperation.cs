using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interfaces
{
    public interface IOperation
    {
        //IEnumerable<string> GetMessages();
        Task<List<string>> GetLabels(object service, string userId);
        Task<object> GetContacts(object contactRequest);
        Task<object> CreateContactFolder(object contactsRequest);
        bool IsFolderExists(object contactsRequest);

        //Task CreateContact(object group, object contactRequest, Lead411EmailParseResult Lead411EmailParseResult,
        //    object contactList);

        Task<bool> DeleteContactFolder(object contactRequest);
        Task<object> GetContactFolder(object contactRequest);
        Task PushSingleMailInQueue(string mailText);
    }
}
