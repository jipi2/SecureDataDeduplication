using DesktopApp.Dto;
using DesktopApp.HttpFolder;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace DesktopApp.ViewModels
{
    public class SignInViewModel : INotifyCollectionChanged
    {
        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<int> Login()
        {
            SecureStorage.Default.RemoveAll();

            Debug.WriteLine($"Username: {_email}, Password: {_password}");
            Debug.WriteLine(Enums.Symbol.token.ToString());
            LoginUser logUser = new LoginUser
            {
                Email = _email,
                password = _password
            };

            var httpClient = HttpServiceCustom.GetApiClient();
            var result = await httpClient.PostAsJsonAsync("api/User/login", logUser);
            if (result.IsSuccessStatusCode)
            {
                Response loginResponse = await result.Content.ReadFromJsonAsync<Response>();
                if (loginResponse != null)
                    if (loginResponse.Succes)
                    {
                        string token = loginResponse.AccessToken;
                        if (token != null)
                        {
                            await SecureStorage.SetAsync(Enums.Symbol.Email.ToString(), _email);
                            await SecureStorage.SetAsync(Enums.Symbol.token.ToString(), token);
                        }

                    }
                return 1;
            }
            else
            {
                string message = await result.Content.ReadAsStringAsync();
                if (message.Equals("Email not verified!"))
                    return 2;
                return 0;
            }

        }

        public async Task sendEmail()
        {
            string email = _email;
            var httpClient = HttpServiceCustom.GetApiClient();
            var result = await httpClient.PostAsJsonAsync("/api/User/sendEmail", email);
            if (!result.IsSuccessStatusCode)
            {
                string message = await result.Content.ReadAsStringAsync();
                throw new Exception(message);
            }
        }
    }
    
}

