using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskVisualizer
{
    [ImplementPropertyChanged]
    public class TreeMapModel
    {
        public string CurrentDir { get; set; }

        public int SliderValue { get; set; }

        public bool IsAItemSelected { get; set; }

        public RelayCommand BackButton { get; set; }
        public RelayCommand DeleteButton { get; set; }
        public RelayCommand ListboxItemLeftButton { get; set; }
        public RelayCommand SliderValueChanged { get; set; }
        
    }
}
