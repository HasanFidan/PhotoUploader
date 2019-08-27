using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoUploader.Common
{
    public class CommonResultModel
    {
        public bool IsOK { get; set; }
        public object Result { get; set; }
        public string Message { get; set; }

        public string extension { get; set; }
    }
}
