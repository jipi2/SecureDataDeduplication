using CryptoLib;
using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Repositories;
using FileStorageApp.Server.SecurityFolder;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace FileStorageApp.Server.Services
{
    public class UserService
    {
        public UserRepository _userRepo { get; set; }
        public SecurityManager _secManager { get; set; }
        public UserService(UserRepository userRepository, SecurityManager secManager)
        {
            _userRepo = userRepository;
            _secManager = secManager;
        }

        public async Task<Response> Register(RegisterUser regUser)
        {
            return await _userRepo.Register(regUser);
        }
        public async Task<Response> Login(LoginUser logUser)
        {
            return await _userRepo.Login(logUser);
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
            string id = _secManager.GetUserIdFromJWT(jwt);
            return id;
        }

        public async Task<User> GetUserById(string id)
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
    }
}
