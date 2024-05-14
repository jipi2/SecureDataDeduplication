using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class FolderHierarchy
    {
        public FolderHierarchy() { }
        public FolderHierarchy(string name)
        {
            Name = name;
        }
        public virtual string FullPathName { get; set; }
        public virtual string Name { get; set; }    
        public virtual IList<FolderHierarchy> Children { get; set; } = new ObservableCollection<FolderHierarchy>();
    }
}
