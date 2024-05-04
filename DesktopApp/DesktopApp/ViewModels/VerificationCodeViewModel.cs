using DesktopApp.Dto;
using DesktopApp.HttpFolder;
using FileStorageApp.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.ViewModels
{
    public class VerificationCodeViewModel : INotifyCollectionChanged
    {

        public VerificationCodeViewModel()
        {
           
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task VerifyCode(string email, string pass, string code ,bool fromRegisterPage)
        {
            VerifyCodeDto dto = new VerifyCodeDto
            {
                email = email,
                password = pass,
                code = code
            };

            var httpClient = HttpServiceCustom.GetApiClient();
            var result = await httpClient.PostAsJsonAsync("api/User/verifyCode", dto);
            if(result.IsSuccessStatusCode)
            {
                Response resp = await result.Content.ReadFromJsonAsync<Response>();
                if (fromRegisterPage == false)
                {
                    await SecureStorage.SetAsync(Enums.Symbol.Email.ToString(), email);
                    await SecureStorage.SetAsync(Enums.Symbol.token.ToString(), resp.AccessToken);
                }
            }
            else
                throw new Exception("Invalid code!");
        }
    }
}
