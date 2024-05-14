using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Models
{
    public class MyItem
    {
        public MyItem()
        {
        }

        public MyItem(string name) // For easy initialization (optional)
        {
            Name = name;
        }

        public virtual string Name { get; set; }
        public virtual IList<MyItem> Children { get; set; } = new ObservableCollection<MyItem>();
    }
}
