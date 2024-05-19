using CryptoLib;
using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Repositories;
using FileStorageApp.Server.SecurityFolder;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using System.Diagnostics;

namespace FileStorageApp.Server.Services
{
    public class UserService
    {
        public UserRepository _userRepo { get; set; }
        public RoleRepository _roleRepo { get; set; }
        public SecurityManager _secManager { get; set; }
        public EmailService _emailService { get; set;}

        public FileFolderRepo _fileFolderRepo { get; set; }
        public UserService(UserRepository userRepository, SecurityManager secManager, RoleRepository roleRepository, EmailService emailService, FileFolderRepo fileFolderRepo)
        {
            _userRepo = userRepository;
            _secManager = secManager;
            _roleRepo = roleRepository;
            _emailService = emailService;
            _fileFolderRepo = fileFolderRepo;
        }

        private void sendVerificatoinCodeViaEmail(User user)
        {
            string? verificationCode = user.VerificationCode;
            if(verificationCode == null)
                throw new Exception("Verification code does not exist!");
            string email = user.Email;
            string subject = "Verification Code";
            string body = "Your verification code is: " + verificationCode;
            _emailService.SendEmail(email, subject, body);
        }
        private bool verifyPassword(string password)
        {
            if (password.Length < 8)
            {
                return false;
            }

            bool hasNumber = false;
            bool hasUppercase = false;
            bool hasSpecialChar = false;

            foreach (char c in password)
            {

                if (char.IsDigit(c))
                {
                    hasNumber = true;
                }

                else if (char.IsUpper(c))
                {
                    hasUppercase = true;
                }

                else if (!char.IsLetterOrDigit(c))
                {
                    hasSpecialChar = true;
                }
            }

            return hasNumber && hasUppercase && hasSpecialChar;
        }
        public async Task<Response> Register(RegisterUser regUser)
        {
            var user = _userRepo.GetUserbyEmail(regUser.Email).Result;
            if (user != null)
                throw new ExceptionModel("Email already exists", 1);
            if (!regUser.Password.Equals(regUser.ConfirmPassword))
                throw new ExceptionModel("Confirm passowrd field is different from password field", 1);

            if (!verifyPassword(regUser.Password))
                throw new ExceptionModel("Password must contain at least 8 characters, one number, one uppercase letter and one special character.", 1);

            byte[] salt = Utils.GenerateRandomBytes(16);

            var newUser = new Entity.User
            {
                FirstName = regUser.FirstName,
                LastName = regUser.LastName,
                Email = regUser.Email,
                Password = Utils.HashTextWithSalt(regUser.Password, salt),
                Salt = Utils.ByteToHex(salt),
                isDeleted = false,
                Roles = new List<Entity.Role>(),
                Pkcs12File = Convert.FromBase64String(regUser.rsaKeys.base64EncPrivKey),
                Base64RSAPublicKey = regUser.rsaKeys.base64PubKey,
                //Files = new List<Entity.FileMetadata>()
                Base64PublicKey = regUser.base64PubKey,
                IsVerified = false,
                VerificationCode = Utils.GenerateRandomNumberAsString(9)
            };
            newUser.Roles.Add(await _roleRepo.getRoleByName("client"));

            try
            {
                sendVerificatoinCodeViaEmail(newUser);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new Exception("The email address is not valid");
            }

            await _userRepo.SaveUser(newUser);
            await _fileFolderRepo.CreateRootFolderForUser(newUser);
           

            return (new Response { Succes = true, Message = "User registered successfully", AccessToken = _secManager.GetNewJwt(newUser) }); 
        }


        public async Task<Response> AddProxy(RegisterProxyDto regProxy)
        {
            var user = _userRepo.GetUserbyEmail(regProxy.ProxyMail).Result;
            if(user != null)
                throw new ExceptionModel("Email already exists for this proxy", 1);

            byte[] salt = Utils.GenerateRandomBytes(16);

            var newProxy = new Entity.User
            {
                FirstName = regProxy.ProxyName,
                LastName = regProxy.ProxyName,
                Email = regProxy.ProxyMail,
                Password = Utils.HashTextWithSalt(regProxy.ProxyPassword, salt),
                Salt = Utils.ByteToHex(salt),
                isDeleted = false,
                Roles = new List<Entity.Role>(),
                //Files = new List<Entity.FileMetadata>()
            };
            newProxy.Roles.Add(await _roleRepo.getRoleByName("proxy"));
            _userRepo.SaveUser(newProxy);

            return (new Response { Succes = true, Message = "Proxy added successfully"}); ;
        }
        public async Task<Response> Login(LoginUser logUser)
        {
            var user = _userRepo.GetUserByEmail(logUser.Email).Result;
            if (user == null)
                throw new ExceptionModel("Login faild!", 1);
            if (!user.Password.Equals(Utils.HashTextWithSalt(logUser.password, Utils.HexToByte(user.Salt))))
                throw new ExceptionModel("Login faild!", 1);
            if (user.IsVerified == false)
            {
                throw new ExceptionModel("Email not verified!", 1);
            }

            return (new Response { Succes = true, Message = "Login successfully", AccessToken = _secManager.GetNewJwt(user) });
        }

        public async Task SendVerificationEmailToLoggedUser(string userEmail)
        {
            var user = _userRepo.GetUserByEmail(userEmail).Result;
            if (user == null)
                throw new Exception("This user does not exist!");
            sendVerificatoinCodeViaEmail(user);
        }

        public async Task<Response> VerifyCode(VerifyCodeDto dto)
        {
            var user = _userRepo.GetUserByEmail(dto.email).Result;
            if (user == null)
                throw new ExceptionModel("Verifying faild!", 1);
            if (!user.Password.Equals(Utils.HashTextWithSalt(dto.password, Utils.HexToByte(user.Salt))))
                throw new ExceptionModel("Verifying faild!", 1);
            if (!user.VerificationCode.Equals(dto.code))
            {
                throw new ExceptionModel("Verifying faild!", 1);
            }
            user.IsVerified = true;
            user.VerifiedAt = DateTime.Now;

            await _userRepo.UpdateUser(user);


            return (new Response { Succes = true, Message = "User is verified", AccessToken = _secManager.GetNewJwt(user) });
        }

        public async Task ResetPassword(string userId, ChangePasswordDto dto)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if (user == null)
                throw new Exception("User does not exist!");
            if (!user.Password.Equals(Utils.HashTextWithSalt(dto.oldPass, Utils.HexToByte(user.Salt))))
            {
                throw new Exception("Old password is not correct!");
            }
            if (dto.newPass.Equals("") || dto.newPass.Equals(" "))
            {
                throw new Exception("The format of new password is not correct!");
            }
            if (!dto.newPass.Equals(dto.confirmNewPass))
            {
                throw new Exception("The passwords do not match!");
            }
            string newHashedPass = Utils.HashTextWithSalt(dto.newPass, Utils.HexToByte(user.Salt));
            user.Password = newHashedPass;

            user.Pkcs12File = Convert.FromBase64String(dto.base64pkcs12);

            await _userRepo.UpdateUser(user);

        }

        public bool isUserAdmin(User user)
        {
            string roleName = _userRepo.GetUserRoleName(user);
            if (roleName == "admin")
                return true;
            return false;
        }

        public async Task<string> GetUserIdFromJWT(string jwt)
        {
            string id =  _secManager.GetUserIdFromJWT(jwt);
            return id;
        }

        public async Task<User>? GetUserById(string id)
        {
            try
            {
                return await _userRepo.GetUserById(Convert.ToInt32(id));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task SaveServerDFKeysForUser(User user, AsymmetricCipherKeyPair serverKeys, string P, string G)
        {

            string serverPubKey = Utils.GetPemAsString(serverKeys.Public);
            string serverPrivKey = Utils.GetPemAsString(serverKeys.Private);

            await _userRepo.SaveServerDFKeysForUser(user, serverPubKey, serverPrivKey, P, G);
            
        }

        public async Task<string> GetPrivateKeyOfServerForUser(string userId)
        {
            User user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            return user.ServerDHPrivate == null ? null : user.ServerDHPrivate;
        }

        public async Task<Dictionary<string, string>> GetParametersForDF(string userId)
        {
            User user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            Dictionary<string, string> stringParams = new Dictionary<string, string>();
            if (user.P == null || user.G == null)
                return stringParams;
            stringParams.Add("P", user.P);
            stringParams.Add("G", user.G);

            return stringParams;
        }

        public async Task SaveSymKeyForUser(string userId, byte[] key)
        {
            try
            {
                await _userRepo.SaveSymKey(Convert.ToInt32(userId), Convert.ToBase64String(key));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //public async Task AddFile(string userId, FileMetadata fileMeta)
        //{
        //    await _userRepo.AddFile(userId, fileMeta);
        //}

        public Task<string> GetUserEmail(string id)
        {
            var user = _userRepo.GetUserById(Convert.ToInt32(id)).Result;
            return Task.FromResult(user.Email);
        }

        public async Task<string> GetUserIdByEmail(string email)
        {
            var user = await _userRepo.GetUserbyEmail(email);
            if (user == null)
                return null;
            return user.Id.ToString();
        }

        public async Task<string?> GetUserPubKey(string email)
        {
            var user = await _userRepo.GetUserbyEmail(email);
            if (user == null)
                return null;
            return user.Base64RSAPublicKey;
        }

        public async Task<RsaDto?> GetRsaKeyPair(string id)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(id));
            if (user == null)
                throw new Exception("User does not exist!");
            if(user.Base64RSAPublicKey == null || user.Pkcs12File == null)
                throw new Exception("User does not have RSA key pair!");
            RsaDto rsaDto = new RsaDto
            {
                base64PubKey = user.Base64RSAPublicKey,
                base64EncPrivKey = Convert.ToBase64String(user.Pkcs12File)
            };
            return rsaDto;
        }

        public async Task<string> GetRecieverPubKey(string receiverEmail)
        {
            User? reciever = await _userRepo.GetUserByEmail(receiverEmail);
            if (reciever == null)
                throw new Exception("Reciever does not exist!");

            return reciever.Base64PublicKey;
        }

        public  async Task<string> GetRecieverKFrag(string userId, string receiverEmail)
        {
            User ? sender = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if(sender == null)
                throw new Exception("Sender does not exist!");
            User ? reciever = await _userRepo.GetUserByEmail(receiverEmail);
            if(reciever == null)
                throw new Exception("Reciever does not exist!");

            string base64KFrag = await _userRepo.GetKFrag(sender, reciever);
            return base64KFrag;
        }

        public async Task SaveKFrag(string userId, KFragDto dto)
        {
            User? sender = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if (sender == null)
                throw new Exception("Sender does not exist!");
            User? reciever = await _userRepo.GetUserByEmail(dto.destEmail);
            if (reciever == null)
                throw new Exception("Reciever does not exist!");

            await _userRepo.SaveKFrag(sender, reciever, dto.base64kfrag);
        }

        public async Task<NameDto> GetFirstAndLastName(string userId)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if (user == null)
                throw new Exception("User does not exist!");
            NameDto nameDto = new NameDto 
            {
                firstName = user.FirstName,
                lastName = user.LastName
            };
            return nameDto;
        }

        public async Task<string> GetPkcs12(string userId)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if (user == null)
                throw new Exception("User does not exist!");
            if (user.Pkcs12File == null)
                throw new Exception("User does not have PKCS12 file!");
            return Convert.ToBase64String(user.Pkcs12File);
        }
    }
}
