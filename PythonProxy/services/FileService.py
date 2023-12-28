from Dto.FIleParamsDto import FileParamsDto
from services.AuthenticateService import ProxyClass
from CryptoFolder.Utils import *

class FileService():
    def __init__(self, userToken:str, fileParams:FileParamsDto):
        self.userToken = userToken
        self.userEmail = ""
        self.fileParams = fileParams
        self.authService = ProxyClass()
    
    async def __getUserEmail(self):
        userEmail = await self.authService.getUserEmail(self.userToken)
        if(userEmail == None):
            print("e None")
            raise Exception("JWT not valid")
        print(userEmail)
        self.userEmail = str(userEmail)
        
    def __getFileBase64Tag(self, base64EncFile:str):
        tag = generateHashForTag(base64EncFile)
        return tag
    
    async def computeFileVerification(self):
        await self.__getUserEmail()
        tag = self.__getFileBase64Tag(self.fileParams.base64EncFile)
        await self.authService.getProxyToken()
        # print(self.authService.token)
    
