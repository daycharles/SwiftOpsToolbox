using SwiftOpsToolbox.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftOpsToolbox.Services
{
    public interface IClipboardService
    {
        ObservableCollection<ClipboardItem> Items { get; }
        void Start();
        void Stop();
        void Clear();
    }
}
