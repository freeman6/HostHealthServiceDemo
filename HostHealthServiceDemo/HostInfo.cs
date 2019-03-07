using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostHealthServiceDemo
{
    public class HostInfo
    {
        public Nullable<System.Guid> ServiceID { get; set; }
        public string IPAddress { get; set; }
        public Nullable<double> CPUUsage { get; set; }
        public Nullable<double> MemoryUsage { get; set; }
        public Nullable<double> DiskUsage { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<bool> IISLiveStatus { get; set; }
        public Nullable<bool> SQLLiveStatus { get; set; }
        public string Memo { get; set; }
        public Nullable<double> SystemDiskFreeBytes { get; set; }
        public Nullable<double> DataDiskFreeBytes { get; set; }
    }
}
