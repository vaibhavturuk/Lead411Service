using System.Collections.Generic;
using ServiceLayer.Interfaces;
using System.Threading.Tasks;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Contacts;
using Google.GData.Contacts;
using Google.GData.Client;
using CoreEntities.Helper;
using Google.GData.Extensions;
using System;
using System.Linq;
using CoreEntities.CustomModels;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ServiceLayer.Services
{
    public class GoogleOperation : IOperation
    {
        /// <summary>
        /// Delete contact group from appropriate account
        /// </summary>
        /// <param name="contactRequest"></param>
        /// <returns></returns>
        public async Task<bool> DeleteContactFolder(object contactRequest)
        {
            try
            {
                ContactsRequest contactObject = (ContactsRequest)contactRequest;
                Group group = (Group)await CreateContactFolder(contactObject);
                contactObject.Delete(group);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// List the labels in the user's mailbox.
        /// </summary>
        /// <param name="service">Gmail API service instance.</param>
        /// <param name="userId">User's email address. The special value "me"
        /// can be used to indicate the authenticated user.</param>
        public async Task<List<string>> GetLabels(object service, string userId)
        {
            List<string> labels = new List<string>();
            try
            {
                ListLabelsResponse response = await ((GmailService)service).Users.Labels.List(userId).ExecuteAsync();
                foreach (Label label in response.Labels)
                {
                    labels.Add(label.Id);
                }
            }
            catch //(Exception e)
            {
                //Console.WriteLine("An error occurred: " + e.Message);
            }
            return labels;
        }

        /// <summary>
        /// Get all contact from google contacts to validate on new contact creation to avoid duplicates.
        /// </summary>
        /// <returns></returns>
        public async Task<object> GetContacts(object contactRequest)
        {
            if (contactRequest == null) throw new ArgumentNullException(nameof(contactRequest));
            try
            {
                List<Contact> contactList = new List<Contact>();
                ContactsQuery query = new ContactsQuery(ContactsQuery.CreateContactsUri("default"));
                query.NumberToRetrieve = 5000;
                Feed<Contact> f = ((ContactsRequest)contactRequest).Get<Contact>(query);
                foreach (Contact entry in f.Entries)
                {
                    contactList.Add(entry);
                }
                await Task.FromResult(0);
                return contactList;
            }
            catch
            { return null; }
           
        }

        /// <summary>
        /// Create group
        /// </summary>
        /// <param name="contactsRequest"></param>
        /// <returns></returns>
        public async Task<object> CreateContactFolder(object contactsRequest)
        {
            await Task.FromResult(0);
            if (contactsRequest == null) throw new ArgumentNullException(nameof(contactsRequest));
            if (!IsFolderExists(contactsRequest))
            {
                Group newGroup = new Group { Title = GeneralParameters.Lead411FolderName };
                newGroup.ExtendedProperties.Add(new ExtendedProperty()
                {
                    Name = GeneralParameters.Lead411FolderName,
                    Value = GeneralParameters.Lead411FolderName,
                });
                Group createdGroup = ((ContactsRequest)contactsRequest).Insert(new Uri("https://www.google.com/m8/feeds/groups/default/full"), newGroup);
                return createdGroup;
            }
            else
            {
                Feed<Group> fg = ((ContactsRequest)contactsRequest).GetGroups();
                Group oldGroup = fg.Entries.Where(x => x.Title.ToLower() == GeneralParameters.Lead411FolderName.ToLower()).Select(group => group).FirstOrDefault();
                return oldGroup;
            }
        }

        /// <summary>
        /// Check for group existence
        /// </summary>
        /// <param name="contactsRequest"></param>
        /// <returns></returns>
        public bool IsFolderExists(object contactsRequest)
        {
            Feed<Group> fg = ((ContactsRequest)contactsRequest).GetGroups();
            int count = fg.Entries.Where(x => x.Title.ToLower() == GeneralParameters.Lead411FolderName.ToLower()).ToList().Count;
            if (count > 0) return true; else return false;
        }

        /// <summary>
        /// Create new contact
        /// </summary>
        /// <param name="group"></param>
        /// <param name="contactRequest"></param>
        /// <param name="Lead411EmailParseResult"></param>
        /// <param name="contactList"></param>
        /// <returns></returns>
        //public async Task CreateContact(object group, object contactRequest, Lead411EmailParseResult Lead411EmailParseResult, object contactList)
        //{
        //    try
        //    {
        //        await Task.FromResult(0);
        //        if (Lead411EmailParseResult.SignatureBlockContactCardInfo.LastName == null)
        //        {
        //            Lead411EmailParseResult.SignatureBlockContactCardInfo.LastName = "";
        //        }
        //        if (Lead411EmailParseResult.SignatureBlockContactCardInfo.FirstName == null)
        //        {
        //            Lead411EmailParseResult.SignatureBlockContactCardInfo.FirstName = "";
        //        }

        //        var listCount1 = ((List<Contact>)contactList).Where(x => x.Emails.Where(y => y.Address == Lead411EmailParseResult.SignatureBlockContactCardInfo.EmailAddress).ToList().Count() > 0).ToList().Count();
        //        //var listCount2 = contactList.Where(x => x.Name.FamilyName == Lead411EmailParseResult.SignatureBlockContactCardInfo.LastName
        //        //&& x.Name.GivenName == Lead411EmailParseResult.SignatureBlockContactCardInfo.LastName
        //        //&& Lead411EmailParseResult.SignatureBlockContactCardInfo == null).Count();
        //        if (listCount1 == 0
        //            //&& listCount2 == 0
        //            )
        //        {
        //            if (!string.IsNullOrWhiteSpace(Lead411EmailParseResult.SignatureBlockContactCardInfo.FirstName))
        //            {
        //                Contact newEntry = new Contact
        //                {
        //                    Name = new Name()
        //                    {
        //                        FullName = (Lead411EmailParseResult.SignatureBlockContactCardInfo.FirstName + " " + Lead411EmailParseResult.SignatureBlockContactCardInfo.LastName).Trim(),
        //                        GivenName = Lead411EmailParseResult.SignatureBlockContactCardInfo.FirstName,
        //                        FamilyName = Lead411EmailParseResult.SignatureBlockContactCardInfo.LastName,
        //                    }
        //                };
        //                // Set the contact's name.
        //                newEntry.GroupMembership.Add(new GroupMembership() { HRef = ((Group)group).Id });

        //                //// Set the contact's e-mail addresses.
        //                if (!string.IsNullOrWhiteSpace(Lead411EmailParseResult.SignatureBlockContactCardInfo.EmailAddress))
        //                {
        //                    newEntry.Emails.Add(new EMail()
        //                    {
        //                        Rel = ContactsRelationships.IsWork,
        //                        Address = Lead411EmailParseResult.SignatureBlockContactCardInfo.EmailAddress
        //                    });
        //                }
        //                // Set the contact's phone numbers.
        //                if (Lead411EmailParseResult.SignatureBlockContactCardInfo.Phones != null)
        //                {
        //                    foreach (var v in Lead411EmailParseResult.SignatureBlockContactCardInfo.Phones)
        //                    {
        //                        string relType;
        //                        if (v.Type == DotIFaces.Enums.CodeOnly.ExtractedPhoneType.Fax)
        //                        {
        //                            relType = ContactsRelationships.IsFax;
        //                        }
        //                        else if (v.Type == DotIFaces.Enums.CodeOnly.ExtractedPhoneType.Home)
        //                        {
        //                            relType = ContactsRelationships.IsHome;
        //                        }
        //                        else if (v.Type == DotIFaces.Enums.CodeOnly.ExtractedPhoneType.Mobile)
        //                        {
        //                            relType = ContactsRelationships.IsMobile;
        //                        }
        //                        else if (v.Type == DotIFaces.Enums.CodeOnly.ExtractedPhoneType.Work)
        //                        {
        //                            relType = ContactsRelationships.IsWork;
        //                        }
        //                        else if (v.Type == DotIFaces.Enums.CodeOnly.ExtractedPhoneType.InvalidEnum)
        //                        {
        //                            relType = ContactsRelationships.IsWork;
        //                        }
        //                        else
        //                        {
        //                            relType = ContactsRelationships.IsWork;
        //                        }
        //                        newEntry.Phonenumbers.Add(new PhoneNumber()
        //                        {
        //                            Primary = true,
        //                            Rel = relType,
        //                            Value = v.EssentialPhoneChars,
        //                        });
        //                    }
        //                }
                        
        //                // Set the contact's postal address.
        //                if (Lead411EmailParseResult.SignatureBlockContactCardInfo.Snail != null)
        //                {
        //                    newEntry.PostalAddresses.Add(new StructuredPostalAddress()
        //                    {
        //                        Rel = ContactsRelationships.IsWork,
        //                        Primary = true,
        //                        Street = Lead411EmailParseResult.SignatureBlockContactCardInfo.Snail.StreetAddr.ExtractedText,
        //                        City = Lead411EmailParseResult.SignatureBlockContactCardInfo.Snail.CityName.ExtractedText,
        //                        //Region = Lead411EmailParseResult.SignatureBlockContactCardInfo.Snail.re,
        //                        Postcode = Lead411EmailParseResult.SignatureBlockContactCardInfo.Snail.USZipCode.ExtractedText,
        //                        //Country = "United States",
        //                        Region = Lead411EmailParseResult.SignatureBlockContactCardInfo.Snail.USStateCode.ExtractedText,
        //                    });
        //                }

        //                // Insert the contact.
        //                Uri feedUri = new Uri(ContactsQuery.CreateContactsUri("default"));
        //                ((ContactsRequest)contactRequest)?.Insert(feedUri, newEntry);
        //                ((List<Contact>)contactList).Add(newEntry);
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        // ignored
        //    }
        //}

        public Task<object> GetContactFolder(object contactRequest)
        {
            return null;
        }


        /// <summary>
        /// Push one txt mail to Azure storage Queue
        /// </summary>
        /// <param name="mailList">List<string></param>
        public async Task PushSingleMailInQueue(string mailText)
        {
            ResponseModel response = new ResponseModel();

            if (mailText != null)
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    System.Configuration.ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                if (queueClient != null)
                {
                    CloudQueue queue = queueClient.GetQueueReference("testqueue");

                    // Create the queue if it doesn't already exist.
                    queue.CreateIfNotExists();

                    // Create a message and add it to the queue.
                    CloudQueueMessage msg = new CloudQueueMessage(mailText);
                    queue.AddMessage(msg);
                }

                response.Message = "Push message in Queue from exception section.";
            }
            else
            {
                response.Message = "No emails found to process";
            }
            //return response;
        }
    }
}
