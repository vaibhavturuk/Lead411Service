using System.Collections.Generic;
using ServiceLayer.Interfaces;
using System.Threading;
using CoreEntities.Helper;
using CoreEntities.CustomModels;
using Google.Apis.Gmail.v1.Data;
using Google.Contacts;
using Google.Apis.Gmail.v1;
using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Requests;
using System.Linq;
using System.Threading.Tasks;
using RepositoryLayer.Repositories.Interfaces;
using CoreEntities.Domain;

namespace ServiceLayer.Services
{
    public class GoogleMailProcess : IMailProcess
    {
        private readonly IAccountRespsitory _iAccount;
        private long _elapsedMs;
        //private long _noOfMailsProccessed;
        private CancellationTokenSource _cts;

        public GoogleMailProcess(
            IOperation operation,
            IAccountRespsitory iAccount
            )
        {
            Operation = operation;
            _iAccount = iAccount;
        }

        public IOperation Operation { get; }

        /// <summary>
        /// Getting emails from the account
        /// </summary>
        public async Task Process(
            object service,
            string userId,
            List<string> labels,
            long userMembershipId,
            object userCredential,
            object contactList,
            object contactRequest)
        {
            _cts = new CancellationTokenSource(GeneralParameters.MaximumSingleRunIndexTimeInMinutes);
            GmailService gmailService = (GmailService)service;
            UserCredential userCredentialObject = (UserCredential)userCredential;

            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            CancellationToken cancellationToken = _cts.Token;

            ContactsRequest contactRequestObject = (ContactsRequest)contactRequest;

            //Create Google Contact request object
            UsersResource.MessagesResource.ListRequest request = gmailService.Users.Messages.List(userId);
            Group group = (Group) await Operation.CreateContactFolder(contactRequest);

            await NewMailProcess(gmailService, userId, labels, userMembershipId, userCredentialObject, contactList, request, group, contactRequestObject, watch, cancellationToken);

        }

        /// <summary>
        /// New mail process
        /// </summary>
        public async Task NewMailProcess(
            GmailService service,
            string userId,
            List<string> labels,
            long userMembershipId,
            UserCredential userCredential,
            object contactList,
            UsersResource.MessagesResource.ListRequest request,
            Group group,
            ContactsRequest contactRequest,
            System.Diagnostics.Stopwatch watch,
            CancellationToken cancellationToken
            )
        {
            try
            {
                //request.LabelIds = labels;
               
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    //Old mail process
                    ListMessagesResponse response = request.Execute(); //
                    request.PageToken = response.NextPageToken;
                    var batchrequest = new BatchRequest(service);
                    if (response.Messages != null)
                    {
                        foreach (Message message in response.Messages)
                        {
                            var req = service.Users.Messages.Get(userId, message.Id);
                            req.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
                            batchrequest.Queue<Message>(req,
                                 (content, error, i, messagesvalue) =>
                                 {
                                     //Lead411EmailParseResult Lead411EmailParseResult = MessageParser.Parse(content);
                                     ////Create new contact
                                     //if (Lead411EmailParseResult.SignatureBlockContactCardInfo != null)
                                     //{
                                     //    Operation.CreateContact(group, contactRequest, Lead411EmailParseResult, contactList);
                                     //}
                                     //_noOfMailsProccessed++;
                                     //if (_noOfMailsProccessed >= GeneralParameters.MaximumSingleRunIndexCount)
                                     //{
                                     //    _cts.Cancel(true);
                                     //}
                                     // Put your callback code here.
                                 });
                            //cancellationToken.ThrowIfCancellationRequested();
                            //Message msg = await service.Users.Messages.Get(userId, message.Id).ExecuteAsync();

                        }
                        await batchrequest.ExecuteAsync(cancellationToken); //
                    }
                } while (!String.IsNullOrWhiteSpace(request.PageToken));
                watch.Stop();
               // _elapsedMs = watch.ElapsedMilliseconds;
               // var lastMailDate = DateTime.UtcNow;

            }
            catch (OperationCanceledException)
            {
                watch.Stop();
                //_elapsedMs = watch.ElapsedMilliseconds;
               // var lastMailDate = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Old mail process
        /// </summary>
        public async Task OldMailProcess(
            GmailService service,
            string userId,
            List<string> labels,
            long mailProcessParentId,
            long userMembershipId,
            UserCredential userCredential,
            MailProcessDates mailProcessDates,
            object contactList,
            UsersResource.MessagesResource.ListRequest request,
            Group group,
            ContactsRequest contactRequest,
            System.Diagnostics.Stopwatch watch,
            CancellationToken cancellationToken
            )
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            Message lastMailProcessed = new Message();
            try
            {
                request = service.Users.Messages.List(userId);
                //request.LabelIds = labels;
                if (mailProcessDates.OldLastProcessedMailDate != null)
                {
                    request.Q = "before:" + mailProcessDates.OldLastProcessedMailDate.Value.ToString("yyyy/MM/dd");
                }

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    //Old mail process
                    ListMessagesResponse response = request.Execute(); //
                    request.PageToken = response.NextPageToken;
                    var batchrequest = new BatchRequest(service);
                    if (response.Messages != null)
                    {
                        foreach (Message message in response.Messages)
                        {
                            var req = service.Users.Messages.Get(userId, message.Id);
                            req.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
                            batchrequest.Queue<Message>(req,
                                 (content, error, i, messagesvalue) =>
                                 {
                                     //Lead411EmailParseResult Lead411EmailParseResult = MessageParser.Parse(content);
                                     ////Create new contact
                                     //if (Lead411EmailParseResult.SignatureBlockContactCardInfo != null)
                                     //{
                                     //    Operation.CreateContact(group, contactRequest, Lead411EmailParseResult, contactList);
                                     //}
                                     //lastMailProcessed = content;
                                     //_noOfMailsProccessed++;
                                     //if (_noOfMailsProccessed >= GeneralParameters.MaximumSingleRunIndexCount)
                                     //{
                                     //    _cts.Cancel(true);
                                     //}
                                     // Put your callback code here.
                                 });
                            //cancellationToken.ThrowIfCancellationRequested();
                            //Message msg = await service.Users.Messages.Get(userId, message.Id).ExecuteAsync();

                        }
                        await batchrequest.ExecuteAsync(cancellationToken);//
                    }

                } while (!String.IsNullOrWhiteSpace(request.PageToken));
                if (string.IsNullOrWhiteSpace(request.PageToken))
                {
                    _iAccount.SaveMailProcessParentCompleted(userMembershipId);
                    watch.Stop();
                    _elapsedMs = watch.ElapsedMilliseconds;
                    DateTime lastMailDate;
                    if (lastMailProcessed.Payload != null)
                    {
                        lastMailDate = Convert.ToDateTime(lastMailProcessed.Payload.Headers.Where(payload => payload.Name == "Date").Select(payload => payload.Value).FirstOrDefault());
                    }
                }
            }
            catch (OperationCanceledException)
            {
                watch.Stop();
                _elapsedMs = watch.ElapsedMilliseconds;
                DateTime lastMailDate;
                if (lastMailProcessed.Payload != null)
                {
                    string date = lastMailProcessed.Payload.Headers.Where(payload => payload.Name == "Date").Select(payload => payload.Value).FirstOrDefault();
                    DateTime newd;
                    if (date != null && DateTime.TryParse(date.Replace("(PDT)", ""), out newd))
                    {
                        lastMailDate = Convert.ToDateTime(newd);
                    }
                    else
                    {
                        if (mailProcessDates.OldLastProcessedMailDate != null)
                            lastMailDate = Convert.ToDateTime(mailProcessDates.OldLastProcessedMailDate);
                        else
                            lastMailDate = DateTime.UtcNow;
                    }

                  
                }
            }
        }

        

    }
}