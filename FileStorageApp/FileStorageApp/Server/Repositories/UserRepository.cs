using FileStorageApp.Server.Database;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.EntityFrameworkCore;
using CryptoLib;
using FileStorageApp.Server.Services;
using FileStorageApp.Server.Entity;
using Azure.Core;
using FileStorageApp.Server.SecurityFolder;

namespace FileStorageApp.Server.Repositories
{
    public class UserRepository
    {
        DataContext _context;
        RoleService _roleService;
        SecurityManager _securityManager;

        private readonly int saltLength = 16;
        public UserRepository(DataContext context, RoleService roleService, SecurityManager securityManager)
        {
            _context = context;
            _roleService = roleService;
            _securityManager = securityManager; 
        }

        public async Task<Response> Register(RegisterUser regUser)
        {
            var user = await _context.Users.Where(u => u.Email.ToLower().Equals(regUser.Email)).FirstOrDefaultAsync();
            if (user != null)
                return (new Response { Succes = false, Message = "Email already exists" });
            if (!regUser.Password.Equals(regUser.Password))
                return (new Response { Succes = false, Message = "Confirm passowrd field is different from password field" });

            byte[] salt = Utils.generateRandomByres(16);

            var newUser = new Entity.User
            {
                FirstName = regUser.FirstName,
                LastName = regUser.LastName,
                Email = regUser.Email,
                Password = Utils.HashTextWithSalt(regUser.Password, salt),
                Salt = Utils.ByteToHex(salt),
                isDeleted = false,
                Roles = new List<Entity.Role>()
            };
            newUser.Roles.Add( await _roleService.GetRoleByName("client"));
            
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return (new Response { Succes = true, Message = "User registered successfully" , AccessToken = _securityManager.GetNewJwt(newUser)});
        }

        public async Task<Response> Login(LoginUser logUser)
        {
            var user = await _context.Users.Include(u => u.Roles).Where(u => u.Email.ToLower().Equals(logUser.Email)).FirstOrDefaultAsync();
            if (user == null)
                return (new Response { Succes = false, Message = "Login faild" });
            if (!user.Password.Equals(Utils.HashTextWithSalt(logUser.password, Utils.HexToByte(user.Salt))))
                return (new Response { Succes = false, Message = "Login faild" });

            return (new Response { Succes = true, Message = "Login successfully", AccessToken = _securityManager.GetNewJwt(user)});

        }

        public string GetUserRoleName(User user)
        {
            return user.Roles[0].RoleName;
        }
    }
}
