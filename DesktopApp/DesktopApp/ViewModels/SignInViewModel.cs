using CryptoLib;
using DesktopApp.Dto;
using DesktopApp.HttpFolder;
using DesktopApp.KeysService;

using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;


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

        private async void saveKeysInRam(string password,string base64pkcs12)
        {
            string? ecPrivateKeybase64;
            RSA? rsaPrivateKey;
            RSA? rsaPublicKey;
            Utils.ReadKeysFromPkcs12(Convert.FromBase64String(base64pkcs12), password, out ecPrivateKeybase64,
                                     out rsaPrivateKey, out rsaPublicKey);
            RSAKeyService.rsaPrivateKey = rsaPrivateKey;
            RSAKeyService.rsaPublicKey = rsaPublicKey;
            await SecureStorage.SetAsync(Enums.Symbol.ECPrivateKeyBase64.ToString(), ecPrivateKeybase64);
        }
        public async Task<int> Login()
        {
            _email = "dreizenpaco@yahoo.com";
            _password = "dreizen";

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
                            httpClient.DefaultRequestHeaders.Remove("Authorization");
                            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                            result = await httpClient.GetAsync("api/User/pkcs12");
                            if (!result.IsSuccessStatusCode)
                                throw new Exception("Something went wrong, please try again later");
                            string base64pkcs12 = await result.Content.ReadAsStringAsync();
                            saveKeysInRam(_password, base64pkcs12);
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

