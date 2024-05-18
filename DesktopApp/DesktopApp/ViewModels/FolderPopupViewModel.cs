using DesktopApp.HttpFolder;
using DesktopApp.Models;
using FileStorageApp.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.ViewModels
{
    public class FolderPopupViewModel : BindableObject
    {
        public ObservableCollection<FolderHierarchy> Nodes { get; set; } = new();

        // Property to store the selected item
        private FolderHierarchy _selectedItem;
        public FolderHierarchy SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    OnSelectedFolder(_selectedItem);
                }
            }
        }

        // Constructor to initialize sample data
        public FolderPopupViewModel()
        {
            _selectedItem = new FolderHierarchy();
            _selectedItem.FullPathName = "";
        }

        // Function to be called when an item is selected
        private string OnSelectedFolder(FolderHierarchy selectedItem)
        {
            // Implement your logic here
            if (selectedItem != null)
            {
                _selectedItem = selectedItem;
                return selectedItem.FullPathName;
            }
            return "";
        }

        public async void GetFolders()
        {
            Nodes.Clear();
            string? jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            if (jwt == null)
            {

                return;
            }
            var httpClient = HttpServiceCustom.GetApiClient(jwt);
            FolderHierarchy? node = await httpClient.GetFromJsonAsync<FolderHierarchy>("/api/FileFolder/getAllFolderForUser");
            if (node != null)
            {
                Nodes.Add(node);
            }
        }

        public async Task CreateFolder(string parentFolderPath, string newFolderName)
        {
            try
            {
                string newFolder;
                if (parentFolderPath == "/")
                    newFolder = parentFolderPath + newFolderName;
                else
                    newFolder = parentFolderPath + "/" + newFolderName;

                string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
                var httpClient = HttpServiceCustom.GetApiClient(jwt);
                var response = await httpClient.PostAsJsonAsync("/api/FileFolder/createFolder", newFolder);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Could not create folder");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
