using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Models
{
    public class SimpleFileHierarchyModel
    {
        public SimpleFileHierarchyModel()
        {
            
        }
        public SimpleFileHierarchyModel(string name)
        {
            Name = name;
        }

        public virtual string FullPathName { get; set; }
        public virtual string Name { get; set; }
        public virtual bool IsFolder { get; set; }
        public virtual IList<SimpleFileHierarchyModel> Children { get; set; } = new ObservableCollection<SimpleFileHierarchyModel>();
    }
}
