using CryptoLib;
using DesktopApp.HttpFolder;
using FileStorageApp.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.ViewModels
{
    public class ResetPasswordViewModel : INotifyCollectionChanged
    {
        private string _oldPass;
        public string OldPass
        {
            get { return _oldPass; }
            set
            {
                if (_oldPass != value)
                {
                    _oldPass = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _newPass;
        public string NewPass
        {
            get { return _newPass; }
            set
            {
                if (_newPass != value)
                {
                    _newPass = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _confirmNewPass;
        public string ConfirmNewPass
        {
            get { return _confirmNewPass; }
            set
            {
                if (_confirmNewPass != value)
                {
                    _confirmNewPass = value;
                    OnPropertyChanged();
                }
            }
        }

        public ResetPasswordViewModel()
        {
            _oldPass = "";
            _newPass = "";
            _confirmNewPass = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task ResetPassword()
        {
            if (_newPass.Equals(" ") || _newPass.Equals(""))
            {
                throw new Exception("You can not have white spaces in the password!");
            }
            if (!_newPass.Equals(_confirmNewPass))
            {
                throw new Exception("Passwords do not match!");
            }

            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetApiClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            var result = await httpClient.GetAsync("api/User/pkcs12");
            if (!result.IsSuccessStatusCode)
                throw new Exception("Something went wrong, please try again later");
            string base64pkcs12 = await result.Content.ReadAsStringAsync();

            byte[]? pkcs12;
            Utils.ChangePkcs12Password(Convert.FromBase64String(base64pkcs12), _oldPass, _newPass, out pkcs12);
            if (pkcs12 == null)
            {
                throw new Exception("Old password is not valid");
            }

            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            ChangePasswordDto dto = new ChangePasswordDto
            {
                oldPass = _oldPass,
                newPass = _newPass,
                confirmNewPass = _confirmNewPass,
                base64pkcs12 = Convert.ToBase64String(pkcs12)
            }; 
          
            var response = await httpClient.PostAsJsonAsync("api/User/resetPassword", dto);
            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();
                throw new Exception(message);
            }
         
        }
    }
}
