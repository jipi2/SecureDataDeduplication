using DesktopApp.HttpFolder;
using FileStorageApp.Shared.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private string _userEmail;
        public string UserEmail
        {
            get => _userEmail;
            set
            {
                if (_userEmail != value)
                {
                    _userEmail = value;
                    OnPropertyChanged(nameof(UserEmail));

                }
            }
        }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged(nameof(_firstName));

                }
            }
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged(nameof(_lastName));

                }
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(_name));

                }
            }
        }

        private string _initials;
        public string Initials
        {
            get => _initials;
            set
            {
                if (_initials != value)
                {
                    _initials = value;
                    OnPropertyChanged(nameof(_initials));

                }
            }
        }

        private string _mailDomain;
        public string MailDomain
        {
            get => _mailDomain;
            set
            {
                if (_mailDomain != value)
                {
                    _mailDomain = value;
                    OnPropertyChanged(nameof(_mailDomain));

                }
            }
        }

        public ProfileViewModel()
        {
            _userEmail = "";
            _firstName = "";
            _lastName = "";
            _mailDomain = "";
            _name = "";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task GetEmail()
        {
            _userEmail = await SecureStorage.GetAsync(Enums.Symbol.Email.ToString());
        }

        public async Task GetFirstNameAndLastName()
        {
            string? fn = await SecureStorage.GetAsync(Enums.Symbol.FirstName.ToString());
            string? ln = await SecureStorage.GetAsync(Enums.Symbol.LastName.ToString());
            if (fn == null || ln == null)
            {
                string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
                var httpClient = HttpServiceCustom.GetApiClient();
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
                try
                {
                    NameDto response = await httpClient.GetFromJsonAsync<NameDto>("api/User/getFirstAndLastName");
                    if (response != null)
                    {
                        fn = response.firstName;
                        ln = response.lastName;
                    }
                    else
                    {
                        fn = "Error";
                        ln = "Error";
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Connection problems");
                }
            }
            _firstName = fn;
            _lastName = ln;
            _name = _firstName + " " + _lastName;
        }

        public async Task GetInitials()
        {
            _initials = _firstName[0].ToString().ToUpper() + _lastName[0].ToString().ToUpper();
        }

        public async Task GetMailDomain()
        {
            _mailDomain = _userEmail.Split('@')[1];
        }
    }
}

