using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.CustomModels
{
    public class FileDetails
    {
        public List<EmailFormat> FileList { get; set; }
        public long EmailTempletId { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }

        public long Total { get; set; }
        public long HardBounce { get; set; }
        public long Sent { get; set; }
        public long SoftBounce { get; set; }
        public bool IsSoftBounce { get; set; }
        public bool InProcess { get; set; }


        public DateTime CreatedOn { get; set; }

    }

}
