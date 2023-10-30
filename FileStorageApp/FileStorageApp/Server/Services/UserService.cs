using FileStorageApp.Server.Repositories;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;

namespace FileStorageApp.Server.Services
{
    public class UserService
    {
        public UserRepository _userRepo { get; set; }
        public UserService(UserRepository userRepository)
        {
            _userRepo = userRepository;
        }

        public async Task<Response> Register(RegisterUser regUser)
        {
            return await _userRepo.Register(regUser);
        }
        public async Task<Response> Login(LoginUser logUser)
        {
            return await _userRepo.Login(logUser);
        }
    }
}
