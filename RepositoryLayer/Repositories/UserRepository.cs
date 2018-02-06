using System;
using CoreEntities.CustomModels.AdminPanel;
using RepositoryLayer.Repositories.Interfaces;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using CoreEntities.Domain;

namespace RepositoryLayer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AzureDbContext _dbcontext;

        public UserRepository()
        {
            if (_dbcontext == null)
            {
                _dbcontext = new AzureDbContext();
                Database.SetInitializer<AzureDbContext>(null);
            }
        }


        public RegisteredUser GetUserList(int pageNumber, int pageSize, string sortBy, bool reverse, string search)
        {
            RegisteredUser vm = new RegisteredUser();
            try
            {
                var users = (from membership in _dbcontext.UserMembership
                                select new RegisteredUser
                                {
                                    EmailId = membership.Email,
                                    FirstName = membership.FirstName,
                                    LastName = membership.LastName,
                                    Provider = membership.Provider,
                                    UserMembershipId = membership.UserMembershipId,
                                    CreatedOn = membership.CreatedOn,
                                    IsActive = membership.IsActive,
                                    
                                }).ToList();

                vm.NoOfRecords = users.Count();

                // Searching
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    users = users.Where(x => x.FirstName.ToLower().Contains(search) ||
                                        x.LastName.ToLower().Contains(search) ||
                                        x.EmailId.ToLower().Contains(search)).ToList();

                    vm.NoOfRecords = users.Count();
                }


                // Sorting ByDescending
                if (reverse)
                {
                    switch (sortBy)
                    {
                        case "firstName":
                            users = users.OrderByDescending(s => s.FirstName).ToList();
                            break;
                        case "lastName":
                            users = users.OrderByDescending(s => s.LastName).ToList();
                            break;
                        case "emailId":
                            users = users.OrderByDescending(s => s.EmailId).ToList();
                            break;
                        default:
                            users = users.OrderByDescending(s => s.Provider).ToList();
                            break;
                    }
                }
                else
                {
                    switch (sortBy)               // Sorting by default Ascending
                    {
                        case "firstName":
                            users = users.OrderBy(s => s.FirstName).ToList();
                            break;
                        case "lastName":
                            users = users.OrderBy(s => s.LastName).ToList();
                            break;
                        case "emailId":
                            users = users.OrderByDescending(s => s.EmailId).ToList();
                            break;
                        default:
                            users = users.OrderBy(s => s.Provider).ToList();
                            break;
                    }
                }

                // Pagination
                var skip = pageSize * (pageNumber - 1);

                var userList = users.Skip(skip).Take(pageSize).ToList();

                vm.RegisteredUsers = userList;

                

                return vm;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public RegisteredUserDetails GetUserDetails(long userMembershipId)
        {
            try
            {
                var authenticationDetailMembershipwise = (from authenticationDetail in _dbcontext.AuthenticationDetail.Where(authDetails => authDetails.UserMembershipId == userMembershipId)
                         join authenticationToken in _dbcontext.AuthenticationToken on authenticationDetail.AuthenticationDetailId equals authenticationToken.AuthenticationDetailId
                         orderby authenticationDetail.AuthenticationDetailId descending
                         select new { authenticationToken.AccessToken, authenticationDetail.RefreshToken }).FirstOrDefault();

                long? noOfIndexerProcessRunForNewMail = 0, totalNoOfMailsProcessed = 0, totalNoOfIndexerRunTillNow = 0, noOfIndexersTakenToComplateOldProcess = 0,
                    totalTimeTakenByProcessTillNow = 0;
                DateTime? oldProcessCompletedDate = null, lastDateOfIndexerRun = null;
                bool isOldProcessCompleted = false;


                var userDetails = (from membership in _dbcontext.UserMembership
                                   where membership.UserMembershipId == userMembershipId
                                   select new RegisteredUserDetails
                                   {
                                       EmailId = membership.Email,
                                       FirstName = membership.FirstName,
                                       LastName = membership.LastName,
                                       Provider = membership.Provider,
                                       UserMembershipId = membership.UserMembershipId,
                                       CreatedOn = membership.CreatedOn,
                                       IsActive = membership.IsActive,
                                      // ApplicationType = applicationType,
                                      // CurrentAccessToken = (authenticationDetailMembershipwise != null) ? ((authenticationDetailMembershipwise.AccessToken != null) ? authenticationDetailMembershipwise.AccessToken : "NotAvailable") : "NotAvailable",
                                       CurrentAccessToken = authenticationDetailMembershipwise.AccessToken,
                                       CurrentRefreshToken = authenticationDetailMembershipwise.RefreshToken,
                                       IsOldProcessCompleted = isOldProcessCompleted,
                                       NoOfIndexerProcessRunForNewMail = noOfIndexerProcessRunForNewMail,
                                       TotalNoOfMailsProcessed = totalNoOfMailsProcessed,
                                       TotalNoOfIndexerRunTillNow = totalNoOfIndexerRunTillNow,
                                       LastDateOfIndexerRun = lastDateOfIndexerRun,
                                       NoOfIndexersTakenToComplateOldProcess = noOfIndexersTakenToComplateOldProcess,
                                       TotalTimeTakenByProcessTillNow = totalTimeTakenByProcessTillNow,

                                      // TimeTakenToFinishedOldMailProcess = timeTakenToFinishedOldMailProcess,
                                       OldProcessCompletedDate = oldProcessCompletedDate
                                   }).FirstOrDefault();
                return userDetails;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
