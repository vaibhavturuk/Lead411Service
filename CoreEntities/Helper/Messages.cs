using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.Helper
{
    public class Messages
    {
        public static string SavedSuccessfully { get; } = "Saved successfully";
        public static string DeletedSuccessfully { get; } = "Deleted successfully";
        public static string UpdatedSuccessfully { get; } = "Updated successfully";
        public static string ProcessCompeleted { get; } = "Process completed successfully";
        public static string ProcessFailed { get; } = "Process failed";
        public static string InvalidToken { get; } = "Invalid Token";
        public static string InvalidAccessType { get; } = "AccessType is not valid";
        public static string RequiredField { get; } = "Required field : ";
        public static string ResetSuccessfully { get; } = "Reset successfully";
    }
}
