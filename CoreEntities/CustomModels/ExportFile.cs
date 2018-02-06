using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.CustomModels
{
    public class ExportFile
    {

        public string Recipient { get; set; }
        public string EmployeCode { get; set; }
        public string Status { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string PhoneNo { get; set; }

    }

    public class ExportData
    {
        public List<ExportFile> RecipientStatusList { get; set; }

        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string FileName { get; set; }

        public string PhoneNo { get; set; }

    }




}
