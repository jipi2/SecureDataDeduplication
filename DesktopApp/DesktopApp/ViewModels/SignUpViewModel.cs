using DesktopApp.Dto;
using DesktopApp.HttpFolder;
using FileStorageApp.Client;
using Microsoft.Extensions.Configuration;
using Python.Runtime;
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
    public class SignUpViewModel :INotifyCollectionChanged
    {
        private CryptoService _cryptoService;
        private readonly IConfiguration _configuration;

        public SignUpViewModel(IConfiguration configuration)
        {
            _cryptoService = new CryptoService();
            _configuration = configuration;
            Runtime.PythonDLL = "C:\\Users\\Jipi\\AppData\\Local\\Programs\\Python\\Python311\\python311.dll";
        }

        public SignUpViewModel()
        { 
            _cryptoService = new CryptoService();
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged();
                }
            }
        }

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

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                if (_confirmPassword != value)
                {
                    _confirmPassword = value;
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

        private string getBase64PrivateKeyForSharing()
        {
            string key = "";
            /*            Runtime.PythonDLL = _configuration["PythonPath"];*/
       
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                string code = @"
from umbral import SecretKey
import base64

def generatePrivKey():
    user_priv_key = SecretKey.random()
    return base64.b64encode(user_priv_key.to_secret_bytes()).decode('utf-8')
";
                dynamic result = PyModule.FromString("generatePrivKey", code);
                key = result.generatePrivKey();

            }
            return key;

        }

        private string getBase64PubKeyForSharing(string base64key)
        {
            string key = "";
            /*            Runtime.PythonDLL = _configuration["PythonPath"];*/
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                string code = @"
from umbral import SecretKey
import base64

def generatePubKey(base64key):
    user_priv_key = SecretKey.from_bytes(base64.b64decode(base64key))
    user_pub_key = user_priv_key.public_key()
    return base64.b64encode (user_pub_key.__bytes__()).decode('utf-8')
";
                dynamic result = PyModule.FromString("generatePubKey", code);
                key = result.generatePubKey(base64key);
            }
            return key;
        }

        private async Task<RsaDto?> ComputeRSAKeysForUser()
        {
            RsaDto? rsaDto = await _cryptoService.GetRsaDto(_password);
            return rsaDto;
        }
        public async Task<bool> Register()
        {
            if(_password != _confirmPassword)
            {
                throw new Exception("Passwords do not match");
            }
            RsaDto? rsaDto = await ComputeRSAKeysForUser();
            if (rsaDto == null)
            {
                return false;
            }

            string base64KeyPriv = getBase64PrivateKeyForSharing();
            string base64KeyPub = getBase64PubKeyForSharing(base64KeyPriv);
            
            File.WriteAllText("D:\\LicentaProiect\\DesktopApp\\DesktopApp\\.keys" + "\\"+_email+"_privateKey.priv", base64KeyPriv);
            File.WriteAllText("D:\\LicentaProiect\\DesktopApp\\DesktopApp\\.keys" + "\\"+_email+"_publicKey.pub", base64KeyPub);

            RegisterUser regUser = new RegisterUser
            {
                LastName = _lastName,
                FirstName = _firstName,
                Email = _email,
                Password = _password,
                ConfirmPassword = _confirmPassword,
                rsaKeys = rsaDto,
                base64PubKey = base64KeyPub

            };
            var httpClient = HttpServiceCustom.GetApiClient();
            var result = await httpClient.PostAsJsonAsync("api/User/register", regUser);
            if (result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var errorContent = await result.Content.ReadAsStringAsync();
                throw new Exception(errorContent);
            }

            return false;
        }
    }
}
